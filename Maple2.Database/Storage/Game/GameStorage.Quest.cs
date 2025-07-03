using Maple2.Database.Extensions;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public Quest? CreateQuest(long ownerId, Quest quest) {
            Model.Quest model = quest;
            model.OwnerId = ownerId;
            Context.Quest.Add(model);

            return Context.TrySaveChanges() ? ToQuest(model) : null;
        }

        public IDictionary<int, Quest> GetQuests(long ownerId) {
            return Context.Quest.Where(quest => quest.OwnerId == ownerId)
                .AsEnumerable()
                .Select(ToQuest)
                .Where(quest => quest != null)
                .ToDictionary(quest => quest!.Id, quest => quest!);
        }

        public bool DeleteQuest(long ownerId, int questId) {
            Context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

            Model.Quest? quest = Context.Quest.Find(ownerId, questId);
            if (quest == null) {
                return false;
            }

            Context.Quest.Remove(quest);
            return SaveChanges();
        }

        public bool SaveQuests(long ownerId, ICollection<Quest> quests) {
            foreach (Quest quest in quests) {
                Model.Quest model = quest;
                model.OwnerId = ownerId;

                Context.Quest.Update(model);
            }

            return Context.TrySaveChanges();
        }

        // Converts model to quest if possible, otherwise returns null.
        private Quest? ToQuest(Model.Quest? model) {
            if (model == null) {
                return null;
            }

            return game.questMetadata.TryGet(model.Id, out QuestMetadata? metadata) ? model.Convert(metadata) : null;
        }
    }
}
