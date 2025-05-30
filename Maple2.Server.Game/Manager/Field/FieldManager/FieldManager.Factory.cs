using System.Collections.Concurrent;
using System.Diagnostics;
using Autofac;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Dungeon;
using Maple2.Model.Metadata;
using Serilog;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

namespace Maple2.Server.Game.Manager.Field;

public partial class FieldManager {
    public sealed class Factory : IDisposable {
        #region Autofac Autowired
        // ReSharper disable MemberCanBePrivate.Global
        public GameStorage GameStorage { get; init; } = null!;
        public required WorldClient World { get; init; }
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

        private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, FieldManager>> fields = []; // Key MapId, RoomId
        private readonly ConcurrentDictionary<long, HomeFieldManager> homes = []; // Key: OwnerId
        private readonly ConcurrentDictionary<int, DungeonFieldManager> dungeons = []; // Key: RoomId
        private readonly ConcurrentDictionary<int, DungeonFieldManager> subDungeonFields = []; // Key: RoomId. This is for rooms created by the dungeon lobby.

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
        public FieldManager? Get(int mapId = 0, long ownerId = 0, int roomId = 0) {
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
        public FieldManager? Create(int mapId, long ownerId = 0, int roomId = 0, InstanceFieldMetadata? instanceFieldMetadata = null) {
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

            switch (instanceFieldMetadata?.Type) {
                case InstanceType.ugcMap:
                    HomeFieldManager homeField = CreateHome(ownerId, metadata, ugcMetadata, entities, NpcMetadata, roomId);
                    logger.Debug("Home Field:{MapId} OwnerId:{OwnerId} Room:{RoomId} initialized in {Time}ms", mapId, ownerId, roomId, sw.ElapsedMilliseconds);

                    if (roomId < 0) { // Decor Planner or Blueprint Designer
                        AddField(homeField);
                    } else {
                        homes.TryAdd(ownerId, homeField);
                    }
                    return homeField;
                    /*case InstanceType.DungeonLobby:
                        DungeonFieldManager? dungeonField = CreateDungeon(ServerTableMetadata.InstanceFieldTable.DungeonRooms[roomId], ownerId);
                        logger.Debug("Dungeon Field:{MapId} OwnerId:{OwnerId} Room:{RoomId} initialized in {Time}ms", mapId, ownerId, roomId, sw.ElapsedMilliseconds);
                        return dungeonField;*/
            }

            var field = new FieldManager(metadata, ugcMetadata, entities, NpcMetadata);
            context.InjectProperties(field);

            // Add to fields
            AddField(field);

            logger.Debug("Field:{MapId} OwnerId:{OwnerId} Room:{RoomId} initialized in {Time}ms", mapId, ownerId, field.RoomId, sw.ElapsedMilliseconds);
            return field;

            void AddField(FieldManager field) {
                fields.AddOrUpdate(
                    mapId,
                    _ => new ConcurrentDictionary<int, FieldManager>([new KeyValuePair<int, FieldManager>(field.RoomId, field)]),
                    (_, existingOwnerFields) => {
                        existingOwnerFields.AddOrUpdate(field.RoomId, field, (_, __) => field);
                        return existingOwnerFields;
                    });
            }
        }

        private HomeFieldManager CreateHome(long ownerId, MapMetadata mapMetadata, UgcMapMetadata ugcMetadata, MapEntityMetadata entities, NpcMetadataStorage npcMetadata, int roomId) {
            using GameStorage.Request db = GameStorage.Context();
            Home? home = db.GetHome(ownerId);
            if (home == null) {
                logger.Error("Loading invalid Home:{OwnerId}", ownerId);
                home = new Home();
            }

            var field = new HomeFieldManager(home, mapMetadata, ugcMetadata, entities, npcMetadata);
            context.InjectProperties(field);

            return field;
        }

        public DungeonFieldManager? CreateDungeon(DungeonRoomMetadata dungeonMetadata, long ownerId, int size = 1, int partyId = 0) {
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

            var dungeonRoomRecord = new DungeonRoomRecord(dungeonMetadata);
            var lobbyField = new DungeonFieldManager(dungeonMetadata, mapMetadata, ugcMetadata, entities, NpcMetadata, ownerId, size, partyId) {
                DungeonRoomRecord = dungeonRoomRecord,
                FieldType = FieldType.Default,
            };
            context.InjectProperties(lobbyField);
            lobbyField.Init();

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
                var field = new DungeonFieldManager(dungeonMetadata, mapMetadata, ugcMetadata, entities, NpcMetadata, ownerId, size, partyId) {
                    Lobby = lobbyField,
                    DungeonRoomRecord = dungeonRoomRecord,
                    FieldType = FieldType.Dungeon,
                };
                context.InjectProperties(field);
                field.Init();

                lobbyField.RoomFields.TryAdd(fieldId, field);
                subDungeonFields.TryAdd(field.RoomId, field);
            }

            dungeons.TryAdd(lobbyField.RoomId, lobbyField);

            return lobbyField;
        }

        public MigrationError DestroyDungeon(int roomId) {
            if (!dungeons.TryGetValue(roomId, out DungeonFieldManager? dungeon)) {
                return MigrationError.s_move_err_dungeon_not_exist;
            }

            return DisposeDungeon(dungeon);
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

                foreach (HomeFieldManager homeField in homes.Values) {
                    if (!homeField.Players.IsEmpty) {
                        logger.Verbose("Home Field {MapId} Owner: {OwnerId} has players", homeField.MapId, homeField.OwnerId);
                        homeField.fieldEmptySince = null;
                        continue;
                    }

                    if (homeField.fieldEmptySince is null) {
                        logger.Verbose("Home Field {MapId} Owner: {OwnerId} is empty, starting timer", homeField.MapId, homeField.OwnerId);
                        homeField.fieldEmptySince = DateTime.UtcNow;
                    } else if (DateTime.UtcNow - homeField.fieldEmptySince > Constant.FieldDisposeEmptyTime) {
                        logger.Debug("Field {MapId} {OwnerId} has been empty for more than {Time}, disposing", homeField.MapId, homeField.OwnerId, Constant.FieldDisposeEmptyTime);
                        homeField.Dispose();
                    }
                }

                foreach (long ownerId in homes.Keys) {
                    if (homes[ownerId].Disposed) {
                        bool success = homes.TryRemove(ownerId, out _);
                        logger.Debug("Home {OwnerId} removed: {Success}", ownerId, success);
                    }
                }

                foreach (DungeonFieldManager dungeonField in dungeons.Values) {
                    DisposeDungeon(dungeonField);
                }

                logger.Verbose("Field dispose loop sleeping for {Interval}ms", Constant.FieldDisposeLoopInterval);
                try {
                    Task.Delay(Constant.FieldDisposeLoopInterval, cancel.Token).Wait(cancel.Token);
                } catch {
                    /* Do nothing */
                }
            }
        }

        private MigrationError DisposeDungeon(DungeonFieldManager dungeon) {
            int playerCount = dungeon.Players.Values.Count + dungeon.RoomFields.Values.Sum(x => x.Players.Values.Count);
            if (playerCount > 0) {
                return MigrationError.s_move_err_InsideDungeonUser;
            }

            dungeon.Dispose();

            List<int> roomIds = dungeon.RoomFields.Values.Select(x => x.RoomId).ToList();
            foreach (DungeonFieldManager roomField in dungeon.RoomFields.Values) {
                roomField.Dispose();
                subDungeonFields.TryRemove(roomField.RoomId, out _);
            }
            dungeons.TryRemove(dungeon.RoomId, out _);
            logger.Debug("Dungeon disposed room Ids: {lobbyRoomId}, {roomIds}", dungeon.RoomId, string.Join(",", roomIds));
            return MigrationError.ok;
        }

        public void Dispose() {
            foreach (ConcurrentDictionary<int, FieldManager> fields in fields.Values) {
                foreach (FieldManager field in fields.Values) {
                    field.Dispose();
                }
            }

            foreach (DungeonFieldManager dungeon in dungeons.Values) {
                dungeon.Dispose();
            }

            foreach (DungeonFieldManager subDungeon in subDungeonFields.Values) {
                subDungeon.Dispose();
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
            subDungeonFields.Clear();
            cancel.Cancel();
            thread.Join();
        }

        private SemaphoreSlim GetMapLock(int mapId) {
            return mapLocks.GetOrAdd(mapId, _ => new SemaphoreSlim(1, 1));
        }

        private FieldManager? GetInternal(int mapId, long ownerId = 0, int roomId = 0) {
            if (roomId != 0) { // Specified room
                FieldManager? foundField = FindRoom();
                if (foundField != null) {
                    return foundField;
                }
            }

            if (ServerTableMetadata.InstanceFieldTable.Entries.TryGetValue(mapId, out InstanceFieldMetadata? metadata)) {
                switch (metadata.Type) {
                    case InstanceType.ugcMap: // this is both homes and UGD. Currently just doing homes.
                        if (roomId != 0) { // User wants to go into decorplanner or blueprint designer
                            return Create(mapId, ownerId: ownerId, roomId: roomId, instanceFieldMetadata: metadata);
                        }
                        if (!homes.TryGetValue(ownerId, out HomeFieldManager? homeField)) {
                            return Create(mapId, ownerId: ownerId, roomId: roomId, instanceFieldMetadata: metadata);
                        }
                        return homeField;
                    case InstanceType.DungeonLobby:
                        return Create(mapId, ownerId: ownerId, roomId: roomId); // TODO: this should never happen.
                    case InstanceType.channelScale:
                        if (fields.TryGetValue(mapId, out ConcurrentDictionary<int, FieldManager>? scalingFields) && !scalingFields.IsEmpty) {
                            return scalingFields.Values.FirstOrDefault(f => !f.Disposed);
                        }
                        break;
                    default:
                        if (ownerId == 0 && roomId == 0) {
                            return Create(mapId);
                        }
                        break;
                }
            }

            if (!fields.TryGetValue(mapId, out ConcurrentDictionary<int, FieldManager>? mapFields)) {
                return Create(mapId, ownerId: ownerId, roomId: roomId);
            }

            if (roomId == 0 && !mapFields.IsEmpty) {
                FieldManager? firstField = mapFields.Values.FirstOrDefault();
                if (firstField != null) {
                    return firstField;
                }
            }

            return mapFields.TryGetValue(roomId, out FieldManager? field) ? field :
                Create(mapId, ownerId: ownerId, roomId: roomId);

            FieldManager? FindRoom() {
                if (fields.TryGetValue(mapId, out ConcurrentDictionary<int, FieldManager>? roomFields) && roomFields.TryGetValue(roomId, out FieldManager? field)) {
                    return field;
                }

                if (subDungeonFields.TryGetValue(roomId, out DungeonFieldManager? subDungeonField)) {
                    return subDungeonField;
                }

                return dungeons.TryGetValue(roomId, out DungeonFieldManager? dungeonField) ? dungeonField : null;
            }
        }
    }
}
