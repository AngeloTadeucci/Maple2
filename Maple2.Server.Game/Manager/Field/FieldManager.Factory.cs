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
        // ReSharper restore All
        #endregion

        private readonly ILogger logger = Log.Logger.ForContext<Factory>();

        private readonly IComponentContext context;
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<long, FieldManager>> homeFields; // K1: MapId, K2: OwnerId
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<int, FieldManager>> fields; // K1: MapId, K2: InstanceId

        public Factory(IComponentContext context) {
            this.context = context;

            fields = new ConcurrentDictionary<int, ConcurrentDictionary<int, FieldManager>>();
            homeFields = new ConcurrentDictionary<int, ConcurrentDictionary<long, FieldManager>>();
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
            var field = new FieldManager(metadata, ugcMetadata, entities, ownerId);
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
        }
    }
}
