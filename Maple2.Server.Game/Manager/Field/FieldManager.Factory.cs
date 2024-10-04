using System.Collections.Concurrent;
using System.Diagnostics;
using Autofac;
using Maple2.Database.Storage;
using Maple2.Database.Storage.Metadata;
using Maple2.Model.Metadata;
using Serilog;

namespace Maple2.Server.Game.Manager.Field;

public partial class FieldManager {
    public sealed class Factory : IDisposable {
        #region Autofac Autowired
        // ReSharper disable MemberCanBePrivate.Global
        public required MapMetadataStorage MapMetadata { private get; init; }
        public required MapEntityStorage MapEntities { private get; init; }
        public required MapDataStorage MapData { private get; init; }
        public required ServerTableMetadataStorage ServerTableMetadata { private get; init; }
        public required NpcMetadataStorage NpcMetadata { get; init; } = null!;
        // ReSharper restore All
        #endregion

        private readonly ILogger logger = Log.Logger.ForContext<Factory>();

        private readonly IComponentContext context;
        private readonly Fields fields;

        private readonly CancellationTokenSource cancel;
        private readonly Thread thread;

        private readonly ConcurrentDictionary<int, SemaphoreSlim> mapLocks;

        public Factory(IComponentContext context) {
            this.context = context;
            mapLocks = new ConcurrentDictionary<int, SemaphoreSlim>();

            fields = new Fields();
            cancel = new CancellationTokenSource();
            thread = new Thread(DisposeLoop);
            thread.Start();
        }

        /// <summary>
        /// Get player home map field or any player owned map. If not found, create a new field.
        /// </summary>
        public FieldManager? Get(int mapId, long ownerId = 0, int instanceId = 0) {
            SemaphoreSlim mapLock = GetMapLock(mapId);
            FieldManager? field;
            try {
                field = GetInternal(mapId, ownerId, instanceId);
            } finally {
                mapLock.Release();
            }
            field?.Init();
            return field;
        }


        /// <summary>
        /// Get map field instance. If not found, create a new field. If the map is defined as instanced, it will create a new instance.
        /// Else, it will return the first instance found if no instanceId is provided.
        /// </summary>
        public FieldManager? Get(int mapId, int instanceId) {
            SemaphoreSlim mapLock = GetMapLock(mapId);
            FieldManager? field;
            try {
                field = GetInternal(mapId, instanceId);
            } finally {
                mapLock.Release();
            }
            field?.Init();
            return field;
        }

        /// <summary>
        /// Create a new FieldManager instance for the given mapId.
        /// If ownerId is provided, it will be a house field. (Player or Guild house)
        /// If no ownerId is provided, it belongs to the "server".
        /// InstanceId can be used to create a new instance of the map.
        /// </summary>
        public FieldManager? Create(int mapId, long ownerId = 0, int instanceId = 0) {
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
            var field = new FieldManager(metadata, ugcMetadata, entities, NpcMetadata, ownerId);
            context.InjectProperties(field);

            OwnerFields ownerFields = fields.GetOrAdd(mapId, new OwnerFields());
            InstancedFields instancedFields = ownerFields.GetOrAdd(ownerId, new InstancedFields());
            instancedFields.GetOrAdd(instanceId, field);

            logger.Debug("Field:{MapId} OwnerId:{OwnerId} Instance:{InstanceId} initialized in {Time}ms", mapId, ownerId, field.InstanceId, sw.ElapsedMilliseconds);
            return field;
        }

        /// <summary>
        /// Disposes the field managers that have no players and have been empty for more than 10 minutes.
        /// </summary>
        private void DisposeLoop() {
            while (!cancel.IsCancellationRequested) {
                foreach (FieldManager fieldManager in fields.AllFields) {
                    if (!fieldManager.Players.IsEmpty) {
                        logger.Verbose("Field {MapId} {InstanceId} has players", fieldManager.MapId, fieldManager.InstanceId);
                        fieldManager.fieldEmptySince = null;
                        continue;
                    }

                    Model.RoomTimer? roomTimer = fieldManager.RoomTimer;
                    if (roomTimer is null) {
                        if (fieldManager.fieldEmptySince is null) {
                            logger.Verbose("Field {MapId} {InstanceId} is empty, starting timer", fieldManager.MapId, fieldManager.InstanceId);
                            fieldManager.fieldEmptySince = DateTime.UtcNow;
                        } else if (DateTime.UtcNow - fieldManager.fieldEmptySince > Constant.FieldDisposeEmptyTime) {
                            logger.Verbose("Field {MapId} {InstanceId} has been empty for more than {Time}, disposing", fieldManager.MapId, fieldManager.InstanceId, Constant.FieldDisposeEmptyTime);
                            fieldManager.Dispose();
                        }
                        continue;
                    }

                    if (roomTimer.Expired(fieldManager.FieldTick)) {
                        logger.Verbose("Field {MapId} {InstanceId} room timer expired, disposing", fieldManager.MapId, fieldManager.InstanceId);
                        fieldManager.Dispose();
                    } else {
                        logger.Verbose("Field {MapId} {InstanceId} room timer has not expired", fieldManager.MapId, fieldManager.InstanceId);
                    }
                }

                // remove fields disposed
                foreach (int mapId in fields.Keys) {
                    foreach (long ownerId in fields[mapId].Keys) {
                        foreach (int instanceId in fields[mapId][ownerId].Keys) {
                            if (fields[mapId][ownerId][instanceId].Disposed) {
                                fields[mapId][ownerId].TryRemove(instanceId, out _);
                            }
                        }
                    }
                }

                logger.Verbose("FieldManager dispose loop sleeping for {Interval}ms", Constant.FieldDisposeLoopInterval);
                try {
                    Task.Delay(Constant.FieldDisposeLoopInterval, cancel.Token).Wait(cancel.Token);
                } catch {
                    /* Do nothing */
                }
            }
        }

        public void Dispose() {
            foreach (OwnerFields manager in fields.Values) {
                foreach (InstancedFields fieldManager in manager.Values) {
                    foreach (FieldManager field in fieldManager.Values) {
                        field.Dispose();
                    }
                }
            }

            foreach (KeyValuePair<int, SemaphoreSlim> locks in mapLocks) {
                locks.Value.Dispose();
            }

            fields.Clear();
            cancel.Cancel();
            thread.Join();
        }

        private SemaphoreSlim GetMapLock(int mapId) {
            return mapLocks.GetOrAdd(mapId, _ => new SemaphoreSlim(1, 1));
        }

        private FieldManager? GetInternal(int mapId, long ownerId = 0, int instanceId = 0) {
            if (ownerId == 0 && instanceId == 0) {
                return Create(mapId);
            }

            if (!fields.TryGetValue(mapId, out OwnerFields? ownerFields)) {
                return Create(mapId, ownerId: ownerId, instanceId: instanceId);
            }

            if (!ownerFields.TryGetValue(ownerId, out InstancedFields? instancedFields)) {
                return Create(mapId, ownerId: ownerId, instanceId: instanceId);
            }

            return instancedFields.TryGetValue(instanceId, out FieldManager? field) ? field :
                Create(mapId, ownerId: ownerId, instanceId: instanceId);
        }

        private FieldManager? GetInternal(int mapId, int instanceId) {
            if (!fields.TryGetValue(mapId, out OwnerFields? ownerFields)) {
                return Create(mapId, ownerId: 0, instanceId: instanceId);
            }

            if (!ownerFields.TryGetValue(0, out InstancedFields? instancedFields)) {
                return Create(mapId, ownerId: 0, instanceId: instanceId);
            }

            return instancedFields.TryGetValue(instanceId, out FieldManager? field) ? field :
                Create(mapId, ownerId: 0, instanceId: instanceId);
        }

        public class InstancedFields : ConcurrentDictionary<int, FieldManager> {
        }

        public class OwnerFields : ConcurrentDictionary<long, InstancedFields> {
        }

        public class Fields : ConcurrentDictionary<int, OwnerFields> {
            public IEnumerable<FieldManager> AllFields {
                get {
                    foreach (OwnerFields ownerFields in Values) {
                        foreach (InstancedFields instancedFields in ownerFields.Values) {
                            foreach (FieldManager fieldManager in instancedFields.Values) {
                                yield return fieldManager;
                            }
                        }
                    }
                }
            }
        }
    }
}
