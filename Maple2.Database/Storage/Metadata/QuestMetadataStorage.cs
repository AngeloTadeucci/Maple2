using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Context;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class QuestMetadataStorage(MetadataContext context) : MetadataStorage<int, QuestMetadata>(context, CACHE_SIZE), ISearchable<QuestMetadata> {
    private const int CACHE_SIZE = 8000; // 4.903 quests are in the database with NA feature, 7.2k quests in xmls
    private bool isInitialized;

    public bool TryGet(int id, [NotNullWhen(true)] out QuestMetadata? quest) {
        if (Cache.TryGet(id, out quest)) {
            return true;
        }

        lock (Context) {
            // Double-checked locking
            if (Cache.TryGet(id, out quest)) {
                return true;
            }

            quest = Context.QuestMetadata.Find(id);

            if (quest is null) {
                return false;
            }

            Cache.AddReplace(id, quest);
        }

        return true;
    }

    public IEnumerable<QuestMetadata> GetQuests() {
        if (isInitialized) {
            return Cache.All().Values;
        }

        lock (Context) {
            List<QuestMetadata> allQuestsList = Context.QuestMetadata.ToList();
            if (allQuestsList.Count > CACHE_SIZE) {
                // leaving this exception in case more quests are added in the future
                throw new Exception("Cache size exceeded the limit.");
            }

            foreach (QuestMetadata quest in allQuestsList) {
                Cache.AddReplace(quest.Id, quest);
            }

            isInitialized = true;
            return allQuestsList;
        }
    }

    public IEnumerable<QuestMetadata> GetQuestsByNpc(int npcId) {
        return GetQuests().Where(x => x.Basic.StartNpc == npcId).ToList();
    }

    public IEnumerable<QuestMetadata> GetQuestsByType(QuestType type) {
        return GetQuests().Where(x => x.Basic.Type == type);
    }

    public IEnumerable<QuestMetadata> GetQuestsByChapter(int chapterId) {
        return GetQuests().Where(x => x.Basic.ChapterId == chapterId);
    }

    public List<QuestMetadata> Search(string name) {
        return GetQuests().Where(x => x.Name != null && x.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
