using System.Collections.Concurrent;
using System.Diagnostics;
using Autofac;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Serilog;

namespace Maple2.Server.Game.Manager.Field;

public partial class FieldManager {
    public sealed class Factory : IDisposable {
        #region Autofac Autowired
        // ReSharper disable MemberCanBePrivate.Global
        public required MapMetadataStorage MapMetadata { private get; init; }
        public required MapEntityStorage MapEntities { private get; init; }
        public required ServerTableMetadataStorage ServerTableMetadata { private get; init; }
        public required NpcMetadataStorage NpcMetadata { get; init; } = null!;
        // ReSharper restore All
        #endregion

        private readonly ILogger logger = Log.Logger.ForContext<Factory>();

        private readonly IComponentContext context;
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<long, FieldManager>> homeFields; // K1: MapId, K2: OwnerId
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, FieldManager>> fields; // K1: MapId, K2: InstanceId

        private readonly CancellationTokenSource cancel;
        private readonly Thread thread;

        public Factory(IComponentContext context) {
            this.context = context;

            fields = new ConcurrentDictionary<int, ConcurrentDictionary<int, FieldManager>>();
            homeFields = new ConcurrentDictionary<int, ConcurrentDictionary<long, FieldManager>>();
            cancel = new CancellationTokenSource();
            thread = new Thread(DisposeLoop);
            thread.Start();
        }

        /// <summary>
        /// Get player home map field or any player owned map. If not found, create a new field.
        /// </summary>
        public FieldManager? Get(int mapId, long ownerId) {
            if (homeFields.TryGetValue(mapId, out ConcurrentDictionary<long, FieldManager>? ownerFields)) {
                return ownerFields.TryGetValue(ownerId, out FieldManager? field)
                    ? field : Create(mapId, ownerId);
            }

            return Create(mapId, ownerId);
        }

        /// <summary>
        /// Get map field instance. If not found, create a new field. If the map is defined as instanced, it will create a new instance.
        /// Else, it will return the first instance found if no instanceId is provided.
        /// </summary>
        public FieldManager? Get(int mapId, int instanceId = 0) {
            ConcurrentDictionary<int, FieldManager> mapFields = fields.GetOrAdd(mapId, new ConcurrentDictionary<int, FieldManager>());

            if (ServerTableMetadata.InstanceFieldTable.Entries.ContainsKey(mapId)) {
                return GetOrCreateField(mapFields, mapId, instanceId);
            }

            // Get first result if possible
            FieldManager? firstField = mapFields.FirstOrDefault().Value;
            return firstField ??
                   // Map is not intentionally an instance, and no fields are found
                   GetOrCreateField(mapFields, mapId, instanceId);

        }

        private FieldManager? GetOrCreateField(ConcurrentDictionary<int, FieldManager> mapFields, int mapId, int instanceId) {
            return mapFields.TryGetValue(instanceId, out FieldManager? field) ? field : Create(mapId, instanceId);
        }

        /// <summary>
        /// Create a new FieldManager instance for the given mapId. If ownerId is provided, it will be a ugc map.
        /// </summary>
        public FieldManager? Create(int mapId, long ownerId = 0) {
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
            field.Init();


            if (ownerId > 0) {
                if (homeFields.TryGetValue(mapId, out ConcurrentDictionary<long, FieldManager>? ownerFields)) {
                    ownerFields[ownerId] = field;
                } else {
                    homeFields[mapId] = new ConcurrentDictionary<long, FieldManager> {
                        [ownerId] = field
                    };
                }
            } else {
                if (fields.TryGetValue(mapId, out ConcurrentDictionary<int, FieldManager>? mapFields)) {
                    mapFields[field.InstanceId] = field;
                } else {
                    fields[mapId] = new ConcurrentDictionary<int, FieldManager> {
                        [field.InstanceId] = field
                    };
                }
            }


            logger.Debug("Field:{MapId} Instance:{InstanceId} initialized in {Time}ms", mapId, field.InstanceId, sw.ElapsedMilliseconds);
            return field;
        }

        /// <summary>
        /// Disposes the field managers that have no players and have been empty for more than 10 minutes.
        /// </summary>
        private void DisposeLoop() {
            while (!cancel.IsCancellationRequested) {
                foreach (ConcurrentDictionary<int, FieldManager> manager in fields.Values) {
                    foreach (FieldManager fieldManager in manager.Values) {
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
                            } else if (DateTime.UtcNow - fieldManager.fieldEmptySince > TimeSpan.FromMinutes(3)) {
                                logger.Verbose("Field {MapId} {InstanceId} has been empty for more than 10 minutes, disposing", fieldManager.MapId, fieldManager.InstanceId);
                                fieldManager.Dispose();
                            }
                            continue;
                        }

                        if (roomTimer is not null && roomTimer.Expired(fieldManager.FieldTick) == true) {
                            logger.Verbose("Field {MapId} {InstanceId} room timer expired, disposing", fieldManager.MapId, fieldManager.InstanceId);
                            fieldManager.Dispose();
                        } else {
                            logger.Verbose("Field {MapId} {InstanceId} room timer has not expired", fieldManager.MapId, fieldManager.InstanceId);
                        }
                    }
                }

                // remove fields disposed
                foreach (int mapId in fields.Keys) {
                    foreach (int instanceId in fields[mapId].Keys) {
                        if (fields[mapId][instanceId].cancel.IsCancellationRequested) {
                            fields[mapId].TryRemove(instanceId, out _);
                        }
                    }
                }

                logger.Verbose("FieldManager dispose loop sleeping for 1 minute");
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }

        public void Dispose() {
            foreach (ConcurrentDictionary<int, FieldManager> manager in fields.Values) {
                foreach (FieldManager fieldManager in manager.Values) {
                    fieldManager.Dispose();
                }
            }

            foreach (ConcurrentDictionary<long, FieldManager> manager in homeFields.Values) {
                foreach (FieldManager fieldManager in manager.Values) {
                    fieldManager.Dispose();
                }
            }

            fields.Clear();
            homeFields.Clear();
            cancel.Cancel();
            thread.Join();
        }
    }
}
