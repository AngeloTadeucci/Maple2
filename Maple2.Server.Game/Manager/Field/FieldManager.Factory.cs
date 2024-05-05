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
        public required AiMetadataStorage NpcAi { private get; init; }
        public required ServerTableMetadataStorage ServerTableMetadata { private get; init; }
        // ReSharper restore All
        #endregion

        private readonly ILogger logger = Log.Logger.ForContext<Factory>();

        private readonly IComponentContext context;
        private readonly ConcurrentDictionary<(int MapId, long OwnerId), FieldManager> fields;

        public Factory(IComponentContext context) {
            this.context = context;

            fields = new ConcurrentDictionary<(int, long), FieldManager>();
        }

        public FieldManager? Get(int mapId, long ownerId = 0) {
            var sw = new Stopwatch();
            sw.Start();
            if (!MapMetadata.TryGet(mapId, out MapMetadata? metadata)) {
                logger.Error("Loading invalid Map:{MapId}", mapId);
                return null;
            }

            if (!MapMetadata.TryGetUgc(mapId, out UgcMapMetadata? ugcMetadata)) {
                ugcMetadata = new UgcMapMetadata(mapId, new Dictionary<int, UgcMapGroup>());
            }

            // ReSharper disable once HeapView.CanAvoidClosure, defer instantiation unless it's needed.
            if (!fields.TryGetValue((mapId, ownerId), out FieldManager? field)) {
                MapEntityMetadata? entities = MapEntities.Get(metadata.XBlock);
                if (entities == null) {
                    throw new InvalidOperationException($"Failed to load entities for map: {mapId}");
                }

                field = new FieldManager(metadata, ugcMetadata, entities, ownerId);
                context.InjectProperties(field);
                field.Init();

                ServerTableMetadata.InstanceFieldTable.Entries.TryGetValue(mapId, out InstanceFieldMetadata? instanceField);
                if (instanceField?.Type == InstanceType.solo) {
                    fields[(mapId, NextGlobalId())] = field;
                } else {
                    fields[(mapId, ownerId)] = field;
                }
            }

            logger.Debug("Field:{MapId} Instance:{InstanceId} initialized in {Time}ms", mapId, field.InstanceId, sw.ElapsedMilliseconds);
            return field;
        }

        public void Dispose() {
            foreach (FieldManager manager in fields.Values) {
                manager.Dispose();
            }

            fields.Clear();
        }
    }
}
