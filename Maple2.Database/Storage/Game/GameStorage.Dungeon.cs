using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Game.Dungeon;
using Maple2.Tools.Extensions;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public Dictionary<int, DungeonRecord> GetDungeonRecords(long ownerId) {
            return Context.DungeonRecord.Where(record => record.OwnerId == ownerId)
                .AsEnumerable()
                .Select<Model.DungeonRecord, DungeonRecord>(record => record)
                .ToDictionary(record => record.DungeonId);
        }

        public DungeonRecord? CreateDungeonRecord(DungeonRecord dungeonRecord, long ownerId) {
            Model.DungeonRecord model = dungeonRecord;
            model.OwnerId = ownerId;
            Context.DungeonRecord.Add(model);

            return Context.TrySaveChanges() ? model : null;
        }

        public bool SaveDungeonRecords(long ownerId, params DungeonRecord[] records) {
            var models = new Model.DungeonRecord[records.Length];
            for (int i = 0; i < records.Length; i++) {
                models[i] = records[i];
                models[i].OwnerId = ownerId;
                Context.DungeonRecord.Update(models[i]);
            }

            return Context.TrySaveChanges();
        }
    }
}
