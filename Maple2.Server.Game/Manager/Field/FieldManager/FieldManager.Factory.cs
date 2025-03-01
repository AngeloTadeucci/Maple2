using System.Collections.Concurrent;
using System.Diagnostics;
using Autofac;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Database.Storage.Metadata;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model.Room;
using Serilog;

namespace Maple2.Server.Game.Manager.Field;

public partial class FieldManager {
    public sealed class Factory : IDisposable {
        #region Autofac Autowired
        // ReSharper disable MemberCanBePrivate.Global
        public GameStorage GameStorage { get; init; } = null!;
        public required MapMetadataStorage MapMetadata { private get; init; }
        public required MapEntityStorage MapEntities { private get; init; }
        public required MapDataStorage MapData { private get; init; }
        public required ServerTableMetadataStorage ServerTableMetadata { private get; init; }
        public required NpcMetadataStorage NpcMetadata { get; init; } = null!;
        // ReSharper restore All
        #endregion

        private readonly ILogger logger = Log.Logger.ForContext<Factory>();

        private readonly IComponentContext context;
        //private readonly Fields fields;

        private readonly CancellationTokenSource cancel;
        private readonly Thread thread;

        private readonly ConcurrentDictionary<int, SemaphoreSlim> mapLocks;

        private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, FieldManager>> fields = [];
        private readonly ConcurrentDictionary<long, HomeFieldManager> homes = [];
        private readonly ConcurrentDictionary<long, DungeonFieldManager> dungeons = [];

        public Factory(IComponentContext context) {
            this.context = context;
            mapLocks = new ConcurrentDictionary<int, SemaphoreSlim>();


            cancel = new CancellationTokenSource();
            thread = new Thread(DisposeLoop);
            thread.Start();
        }

        /// <summary>
        /// Get player home map field or any player owned map. If not found, create a new field.
        /// </summary>
        public FieldManager? Get(int mapId, long ownerId = 0, int roomId = 0) {
            if (ServerTableMetadata.InstanceFieldTable.Entries.ContainsKey(mapId)) {
                if (ownerId == 0 && roomId == 0) {
                    return Create(mapId);
                }
            }
            SemaphoreSlim mapLock = GetMapLock(mapId);
            mapLock.Wait();
            FieldManager? field;
            try {
                field = GetInternal(mapId, ownerId, roomId);
            } finally {
                mapLock.Release();
            }
            field?.Init();
            return field;
        }

        /// <summary>
        /// Create a new Field instance for the given mapId.
        /// If ownerId is provided, it will be a house field. (Player or Guild house)
        /// If no ownerId is provided, it belongs to the "server".
        /// roomId can be used to create a new instance of the map.
        /// </summary>
        public FieldManager? Create(int mapId, long ownerId = 0, int roomId = 0) {
            var sw = new Stopwatch();
            sw.Start();
            if (!MapMetadata.TryGet(mapId, out MapMetadata? metadata)) {
                logger.Error("Loading invalid Map:{MapId}", mapId);
                return null;
            }

            if (!MapMetadata.TryGetUgc(mapId, out UgcMapMetadata? ugcMetadata)) {
                ugcMetadata = new UgcMapMetadata(mapId, new Dictionary<int, UgcMapGroup>());
            }

            MapEntityMetadata? entities = MapEntities.Get(metadata.XBlock);
            if (entities == null) {
                throw new InvalidOperationException($"Failed to load entities for map: {mapId}");
            }

            if (Constant.DefaultHomeMapId == mapId) {
                HomeFieldManager homeField = CreateHome(ownerId, metadata, ugcMetadata, entities, NpcMetadata);
                logger.Debug("Home Field:{MapId} OwnerId:{OwnerId} Room:{RoomId} initialized in {Time}ms", mapId, ownerId, roomId, sw.ElapsedMilliseconds);
                return homeField;
            }

            var field = new FieldManager(metadata, ugcMetadata, entities, NpcMetadata);
            context.InjectProperties(field);

            // Add to fields
            fields.AddOrUpdate(
                mapId,
                _ => new ConcurrentDictionary<int, FieldManager>([new KeyValuePair<int, FieldManager>(roomId, field)]),
                (_, existingOwnerFields) => {
                    existingOwnerFields.AddOrUpdate(roomId, field, (_, __) => field);
                    return existingOwnerFields;
                });

            logger.Debug("Field:{MapId} OwnerId:{OwnerId} Room:{RoomId} initialized in {Time}ms", mapId, ownerId, field.RoomId, sw.ElapsedMilliseconds);
            return field;
        }

        private HomeFieldManager CreateHome(long ownerId, MapMetadata mapMetadata, UgcMapMetadata ugcMetadata, MapEntityMetadata entities, NpcMetadataStorage npcMetadata) {
            using GameStorage.Request db = GameStorage.Context();
            Home? home = db.GetHome(ownerId);
            if (home == null) {
                logger.Error("Loading invalid Home:{OwnerId}", ownerId);
                home = new Home();
            }

            var field = new HomeFieldManager(home, mapMetadata, ugcMetadata, entities, npcMetadata);
            context.InjectProperties(field);
            homes.TryAdd(ownerId, field);
            return field;
        }

        public DungeonFieldManager? CreateDungeon(DungeonRoomTable.DungeonRoomMetadata dungeonMetadata) {
            var sw = new Stopwatch();
            sw.Start();

            if (!MapMetadata.TryGet(dungeonMetadata.LobbyFieldId, out MapMetadata? mapMetadata)) {
                logger.Error("Loading invalid Lobby Field Map:{MapId}", dungeonMetadata.LobbyFieldId);
                return null;
            }

            if (!MapMetadata.TryGetUgc(dungeonMetadata.LobbyFieldId, out UgcMapMetadata? ugcMetadata)) {
                ugcMetadata = new UgcMapMetadata(dungeonMetadata.LobbyFieldId, new Dictionary<int, UgcMapGroup>());
            }

            MapEntityMetadata? entities = MapEntities.Get(mapMetadata.XBlock);
            if (entities == null) {
                throw new InvalidOperationException($"Failed to load entities for map: {dungeonMetadata.LobbyFieldId}");
            }
            var lobbyField = new DungeonFieldManager(dungeonMetadata, mapMetadata, ugcMetadata, entities, NpcMetadata);
            context.InjectProperties(lobbyField);

            foreach (int fieldId in dungeonMetadata.FieldIds) {
                if (!MapMetadata.TryGet(fieldId, out mapMetadata)) {
                    logger.Error("Loading invalid Dungeon Field Map:{MapId}", fieldId);
                    continue;
                }

                if (!MapMetadata.TryGetUgc(fieldId, out ugcMetadata)) {
                    ugcMetadata = new UgcMapMetadata(fieldId, new Dictionary<int, UgcMapGroup>());
                }

                entities = MapEntities.Get(mapMetadata.XBlock);
                if (entities == null) {
                    throw new InvalidOperationException($"Failed to load entities for map: {fieldId}");
                }
                var field = new DungeonFieldManager(dungeonMetadata, mapMetadata, ugcMetadata, entities, NpcMetadata);
                context.InjectProperties(field);

                lobbyField.RoomFields.TryAdd(fieldId, field);
            }

            return lobbyField;
        }

        /// <summary>
        /// Disposes the field managers that have no players and have been empty for more than 10 minutes.
        /// </summary>
        private void DisposeLoop() {
            while (!cancel.IsCancellationRequested) {
                foreach ((int mapId, ConcurrentDictionary<int, FieldManager> fieldList) in fields) {
                    foreach ((int roomId, FieldManager field) in fieldList) {
                        if (!field.Players.IsEmpty) {
                            logger.Verbose("Field {MapId} {RoomId} has players", field.MapId, field.RoomId);
                            field.fieldEmptySince = null;
                            continue;
                        }

                        if (field.fieldAdBalloons.Values
                            .Any(fieldInteract => fieldInteract.Object is InteractBillBoardObject billboard && DateTime.Now.ToEpochSeconds() < billboard.ExpirationTime)) {
                            logger.Verbose("Field {MapId} {RoomId} has ad balloons", field.MapId, field.RoomId);
                            field.fieldEmptySince = null;
                            continue;
                        }

                        Model.RoomTimer? roomTimer = field.RoomTimer;
                        if (roomTimer is null) {
                            if (field.fieldEmptySince is null) {
                                logger.Verbose("Field {MapId} {RoomId} is empty, starting timer", field.MapId, field.RoomId);
                                field.fieldEmptySince = DateTime.UtcNow;
                            } else if (DateTime.UtcNow - field.fieldEmptySince > Constant.FieldDisposeEmptyTime) {
                                logger.Debug("Field {MapId} {RoomId} has been empty for more than {Time}, disposing", field.MapId, field.RoomId, Constant.FieldDisposeEmptyTime);
                                field.Dispose();
                            }
                            continue;
                        }

                        if (roomTimer.Expired(field.FieldTick)) {
                            logger.Debug("Field {MapId} {RoomId} room timer expired, disposing", field.MapId, field.RoomId);
                            field.Dispose();
                        } else {
                            logger.Verbose("Field {MapId} {RoomId} room timer has not expired", field.MapId, field.RoomId);
                        }
                    }
                }

                // remove fields disposed
                foreach (int mapId in fields.Keys) {
                    foreach ((int roomId, FieldManager fieldManager) in fields[mapId]) {
                        if (fieldManager.Disposed) {
                            bool success = fields[mapId].TryRemove(roomId, out _);
                            logger.Debug("Field {MapId}, RoomId {RoomId}, removed: {Success}", mapId, roomId, success);
                        }
                    }
                }

                foreach (long ownerId in homes.Keys) {
                    if (homes[ownerId].Disposed) {
                        bool success = homes.TryRemove(ownerId, out _);
                        logger.Debug("Home {OwnerId} removed: {Success}", ownerId, success);
                    }
                }

                logger.Verbose("Field dispose loop sleeping for {Interval}ms", Constant.FieldDisposeLoopInterval);
                try {
                    Task.Delay(Constant.FieldDisposeLoopInterval, cancel.Token).Wait(cancel.Token);
                } catch {
                    /* Do nothing */
                }
            }
        }

        public void Dispose() {
            foreach (ConcurrentDictionary<int, FieldManager> fields in fields.Values) {
                foreach (FieldManager field in fields.Values) {
                    field.Dispose();
                }
            }

            foreach (HomeFieldManager home in homes.Values) {
                home.Dispose();
            }

            foreach (KeyValuePair<int, SemaphoreSlim> locks in mapLocks) {
                locks.Value.Dispose();
            }

            fields.Clear();
            homes.Clear();
            dungeons.Clear();
            cancel.Cancel();
            thread.Join();
        }

        private SemaphoreSlim GetMapLock(int mapId) {
            return mapLocks.GetOrAdd(mapId, _ => new SemaphoreSlim(1, 1));
        }

        private FieldManager? GetInternal(int mapId, long ownerId = 0, int roomId = 0) {
            if (mapId == Constant.DefaultHomeMapId) {
                if (!homes.TryGetValue(ownerId, out HomeFieldManager? homeField)) {
                    return Create(mapId, ownerId: ownerId, roomId: roomId);
                }

                return homeField;
            }

            if (!fields.TryGetValue(mapId, out ConcurrentDictionary<int, FieldManager>? mapFields)) {
                return Create(mapId, ownerId: ownerId, roomId: roomId);
            }

            return mapFields.TryGetValue(roomId, out FieldManager? field) ? field :
                Create(mapId, ownerId: ownerId, roomId: roomId);
        }
    }
}
