using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Caching;
using Maple2.Database.Context;
using Maple2.Model.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Database.Storage;

public class SkillMetadataStorage : MetadataStorage<(int, short), SkillMetadata>, ISearchable<StoredSkillMetadata> {
    private const int CACHE_SIZE = 23000; // ~22k total skill levels
    private const int EFFECT_CACHE_SIZE = 15000;  // ~14.5k total additional effect levels

    protected readonly LRUCache<(int Id, short Level), AdditionalEffectMetadata> EffectCache;

    public SkillMetadataStorage(MetadataContext context) : base(context, CACHE_SIZE) {
        EffectCache = new(EFFECT_CACHE_SIZE, (int) (EFFECT_CACHE_SIZE * 0.05));
    }

    public bool TryGet(int id, short level, [NotNullWhen(true)] out SkillMetadata? skill) {
        if (Cache.TryGet((id, level), out skill)) {
            return true;
        }

        lock (Context) {
            // Double-checked locking
            if (Cache.TryGet((id, level), out skill)) {
                return true;
            }

            StoredSkillMetadata? storedSkill = Context.SkillMetadata.Find(id);

            if (storedSkill == null) {
                return false;
            }

            foreach ((short dataLevel, SkillMetadataLevel data) in storedSkill.Levels) {
                var metadata = new SkillMetadata(id, dataLevel, storedSkill.Name, storedSkill.Property, storedSkill.State, data);
                Cache.AddReplace((id, dataLevel), metadata);

                if (dataLevel == level) {
                    skill = metadata;
                }
            }
        }

        return skill != null;
    }

    public bool TryGetEffect(int id, short level, [NotNullWhen(true)] out AdditionalEffectMetadata? effect) {
        if (EffectCache.TryGet((id, level), out effect)) {
            return true;
        }

        lock (Context) {
            // Double-checked locking
            if (EffectCache.TryGet((id, level), out effect)) {
                return true;
            }

            effect = Context.AdditionalEffectMetadata.Find(id, level);

            if (effect == null) {
                return false;
            }

            EffectCache.AddReplace((id, level), effect);
        }

        return true;
    }

    public List<StoredSkillMetadata> Search(string name) {
        lock (Context) {
            return Context.SkillMetadata
                .Where(skill => EF.Functions.Like(skill.Name!, $"%{name}%"))
                .ToList();
        }
    }
}
