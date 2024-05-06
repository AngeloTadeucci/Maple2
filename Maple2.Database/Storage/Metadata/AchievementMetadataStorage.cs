using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Maple2.Database.Context;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Database.Storage;

public class AchievementMetadataStorage(MetadataContext context) : MetadataStorage<int, AchievementMetadata>(context, CACHE_SIZE), ISearchable<AchievementMetadata> {
    private const int CACHE_SIZE = 2500; // ~2.2k total trophies

    public bool TryGet(int id, [NotNullWhen(true)] out AchievementMetadata? achievement) {
        if (Cache.TryGet(id, out achievement)) {
            return true;
        }

        lock (Context) {
            // Double-checked locking
            if (Cache.TryGet(id, out achievement)) {
                return true;
            }

            achievement = Context.AchievementMetadata.Find(id);

            if (achievement == null) {
                return false;
            }

            Cache.AddReplace(id, achievement);
        }

        return true;
    }

    public ICollection<AchievementMetadata> GetType(ConditionType type) {
        lock (Context) {
            return Context.AchievementMetadata
                .AsEnumerable()
                .Where(achievement => achievement.Grades.Values.Any(grade => grade.Condition.Type == type))
                .ToList();
        }
    }

    public List<AchievementMetadata> Search(string name) {
        lock (Context) {
            return Context.AchievementMetadata
                .Where(achievement => EF.Functions.Like(achievement.Name!, $"%{name}%"))
                .ToList();
        }
    }
}
