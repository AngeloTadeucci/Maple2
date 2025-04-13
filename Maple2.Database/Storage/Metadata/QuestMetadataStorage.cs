using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Context;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class QuestMetadataStorage(MetadataContext context) : MetadataStorage<int, QuestMetadata>(context, CACHE_SIZE), ISearchable<QuestMetadata> {
    private const int CACHE_SIZE = 2500; // ~2.2k total items

    private readonly ConcurrentDictionary<int, QuestMetadata> allQuests = [];

    public bool TryGet(int id, [NotNullWhen(true)] out QuestMetadata? quest) {
        if (allQuests.IsEmpty) {
            GetAllQuests();
        }

        if (Cache.TryGet(id, out quest)) {
            return true;
        }

        lock (Context) {
            // Double-checked locking
            if (Cache.TryGet(id, out quest)) {
                return true;
            }

            if (!allQuests.TryGetValue(id, out quest)) {
                return false;
            }

            Cache.AddReplace(id, quest);
        }

        return true;
    }

    private void GetAllQuests() {
        if (!allQuests.IsEmpty) {
            return;
        }

        lock (Context) {
            if (!allQuests.IsEmpty) {
                return;
            }

            List<QuestMetadata> allQuestsList = Context.QuestMetadata.ToList();
            foreach (QuestMetadata quest in allQuestsList) {
                allQuests.TryAdd(quest.Id, quest);
            }
        }
    }

    public IEnumerable<QuestMetadata> GetQuests() {
        lock (Context) {
            if (!allQuests.IsEmpty) {
                return allQuests.Values;
            }

            GetAllQuests();

            return allQuests.Values;
        }
    }

    public IEnumerable<QuestMetadata> GetQuestsByNpc(int npcId) {
        return allQuests.Values.Where(x => x.Basic.StartNpc == npcId).ToList();
    }

    public IEnumerable<QuestMetadata> GetQuestsByType(QuestType type) {
        return allQuests.Values.Where(x => x.Basic.Type == type);
    }

    public List<QuestMetadata> Search(string name) {
        return allQuests.Values.Where(x => x.Name != null && x.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
