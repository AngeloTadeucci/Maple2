using Maple2.Database.Context;
using Maple2.Model.Metadata;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maple2.Database.Storage;

public class AiMetadataStorage(MetadataContext context) : MetadataStorage<string, AiMetadata>(context, CACHE_SIZE) {
    private const int CACHE_SIZE = 2500; // ~2.2k total items

    public bool TryGet(string name, [NotNullWhen(true)] out AiMetadata? npcAi) {
        if (Cache.TryGet(name, out npcAi)) {
            return true;
        }

        lock (Context) {
            // Double-checked locking
            if (Cache.TryGet(name, out npcAi)) {
                return true;
            }

            npcAi = Context.AiMetadata.Find(name);

            if (npcAi == null) {
                return false;
            }

            Cache.AddReplace(name, npcAi);
        }

        return true;
    }

    public IEnumerable<AiMetadata> GetAis() {
        lock (Context) {
            return Context.AiMetadata.ToList();
        }
    }
}
