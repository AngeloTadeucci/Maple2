using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Tools.Extensions;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public List<Medal> GetMedals(long ownerId) {
            return Context.Medal.Where(medal => medal.OwnerId == ownerId)
                .AsEnumerable()
                .Select(ToMedal)
                .WhereNotNull()
                .ToList();
        }

        public Medal? CreateMedal(long ownerId, Medal medal) {
            Model.Medal model = medal;
            model.OwnerId = ownerId;
            model.Id = 0;
            Context.Medal.Add(model);

            return Context.TrySaveChanges() ? ToMedal(model) : null;
        }

        public bool SaveMedals(long ownerId, params Medal[] medals) {
            var models = new Model.Medal[medals.Length];
            for (int i = 0; i < medals.Length; i++) {
                if (medals[i].Uid == 0) {
                    continue;
                }

                models[i] = medals[i];
                models[i].OwnerId = ownerId;
                Context.Medal.Update(models[i]);
            }

            return Context.TrySaveChanges();
        }

        // Converts model to item if possible, otherwise returns null.
        private Medal? ToMedal(Model.Medal? model) {
            if (model == null) {
                return null;
            }

            return game.tableMetadata.SurvivalSkinInfoTable.Entries.TryGetValue(model.MedalId, out MedalType medalType) ? model.Convert(medalType) : null;
        }
    }
}
