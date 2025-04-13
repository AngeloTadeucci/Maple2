using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Maple2.Database.Context;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Database.Storage;

public class QuestMetadataStorage(MetadataContext context) : MetadataStorage<int, QuestMetadata>(context, CACHE_SIZE), ISearchable<QuestMetadata> {
    private const int CACHE_SIZE = 2500; // ~2.2k total items

    private readonly Dictionary<int, QuestMetadata> allQuests = [];

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

            if (quest == null) {
                return false;
            }

            Cache.AddReplace(id, quest);
        }

        return true;
    }

    public IEnumerable<QuestMetadata> GetQuests() {
        lock (Context) {
            if (allQuests.Count > 0) {
                return allQuests.Values;
            }

            List<QuestMetadata> allQuestsList = Context.QuestMetadata.ToList();
            foreach (QuestMetadata quest in allQuestsList) {
                allQuests.Add(quest.Id, quest);
            }
            return allQuestsList;
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
