using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Maple2.Database.Context;
using Maple2.Model.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Database.Storage;

public class QuestMetadataStorage(MetadataContext context) : MetadataStorage<int, QuestMetadata>(context, CACHE_SIZE), ISearchable<QuestMetadata> {
    private const int CACHE_SIZE = 2500; // ~2.2k total items

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
            return Context.QuestMetadata.ToList();
        }
    }

    public IEnumerable<QuestMetadata> GetQuestsByNpc(int npcId) {
        lock (Context) {
            return Context.QuestMetadata
                .FromSqlRaw($"SELECT * FROM `quest` WHERE JSON_EXTRACT(Basic, '$.StartNpc')={npcId}")
                .ToList();
        }
    }

    public List<QuestMetadata> Search(string name) {
        lock (Context) {
            return Context.QuestMetadata
                .Where(quest => EF.Functions.Like(quest.Name!, $"%{name}%"))
                .ToList();
        }
    }
}
