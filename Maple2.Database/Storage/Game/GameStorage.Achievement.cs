using System.Collections.Generic;
using System.Linq;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Z.EntityFramework.Plus;
using Achievement = Maple2.Model.Game.Achievement;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    public partial class Request {
        public Achievement? CreateAchievement(long ownerId, Achievement achievement) {
            Model.Achievement model = achievement;
            model.OwnerId = ownerId;
            Context.Achievement.Add(model);

            return Context.TrySaveChanges() ? ToAchievement(model) : null;
        }

        public IDictionary<int, Achievement> GetAchievements(long ownerId) {
            return Context.Achievement.Where(achievement => achievement.OwnerId == ownerId)
                .AsEnumerable()
                .Select(ToAchievement)
                .Where(achievement => achievement != null)
                .ToDictionary(achievement => achievement!.Id, achievement => achievement!);
        }

        public AchievementInfo GetAchievementInfo(long accountId, long characterId) {
            Dictionary<AchievementCategory, int> achievementCounts = Context.Achievement
                .Where(achievement => achievement.OwnerId == accountId || achievement.OwnerId == characterId)
                .GroupBy(achievement => achievement.Category)
                .Select(group => new {
                    Category = group.Key,
                    Count = group.Sum(g => g.CompletedCount),
                })
                .ToDictionary(entry => entry.Category, entry => entry.Count);

            return new AchievementInfo {
                Combat = achievementCounts.GetValueOrDefault(AchievementCategory.Combat, 0),
                Adventure = achievementCounts.GetValueOrDefault(AchievementCategory.Adventure, 0),
                Lifestyle = achievementCounts.GetValueOrDefault(AchievementCategory.Life, 0)
                            + achievementCounts.GetValueOrDefault(AchievementCategory.None, 0),
            };
        }

        public bool SaveAchievements(long ownerId, ICollection<Achievement> achievements) {
            foreach (Achievement achievement in achievements) {
                Model.Achievement model = achievement;
                model.OwnerId = ownerId;

                Context.Achievement.Update(model);
            }

            return Context.TrySaveChanges();
        }

        // Converts model to item if possible, otherwise returns null.
        private Achievement? ToAchievement(Model.Achievement? model) {
            if (model == null) {
                return null;
            }

            return game.achievementMetadata.TryGet(model.Id, out AchievementMetadata? metadata) ? model.Convert(metadata) : null;
        }
    }
}
