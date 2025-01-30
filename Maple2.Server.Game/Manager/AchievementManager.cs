using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Game.Manager;

public sealed class AchievementManager {
    private const int BATCH_SIZE = 60;
    private readonly GameSession session;

    private readonly IDictionary<int, Achievement> accountValues;
    private readonly IDictionary<int, Achievement> characterValues;

    private readonly ILogger logger = Log.Logger.ForContext<AchievementManager>();

    public AchievementManager(GameSession session) {
        this.session = session;

        using GameStorage.Request db = session.GameStorage.Context();
        accountValues = new ConcurrentDictionary<int, Achievement>(db.GetAchievements(session.AccountId));
        characterValues = new ConcurrentDictionary<int, Achievement>(db.GetAchievements(session.CharacterId));
    }

    public void Load() {
        session.Send(AchievementPacket.Initialize());
        foreach (ImmutableList<Achievement> batch in accountValues.Values.Batch(BATCH_SIZE)) {
            session.Send(AchievementPacket.Load(batch));
        }
        foreach (ImmutableList<Achievement> batch in characterValues.Values.Batch(BATCH_SIZE)) {
            session.Send(AchievementPacket.Load(batch));
        }
    }

    public bool TryGetAchievement(int achievementId, [NotNullWhen(true)] out Achievement? achievement) {
        return accountValues.TryGetValue(achievementId, out achievement) || characterValues.TryGetValue(achievementId, out achievement);
    }

    /// <summary>
    /// Checks for any possible trophies under stated ConditionType. If there is a trophy that can have any progress updated to it, update or add to player if it hasn't existed yet.
    /// </summary>
    /// <param name="conditionType">TrophyConditionType to search metadata</param>
    /// <param name="count">Condition value to update progress of a trophy. Default is 1.</param>
    /// <param name="targetString">Trophy grade condition target parameter in string.</param>
    /// <param name="targetLong">Trophy grade condition target parameter in long.</param>
    /// <param name="codeString">Trophy grade condition code parameter in string.</param>
    /// <param name="codeLong">Trophy grade condition code parameter in long.</param>
    public void Update(ConditionType conditionType, long count = 1, string targetString = "", long targetLong = 0, string codeString = "", long codeLong = 0) {
        foreach (AchievementMetadata metadata in session.AchievementMetadata.GetType(conditionType)) {
            IDictionary<int, Achievement> achievements = metadata.AccountWide ? accountValues : characterValues;
            if (!achievements.TryGetValue(metadata.Id, out Achievement? achievement) || !metadata.Grades.TryGetValue(achievement.CurrentGrade, out AchievementMetadataGrade? grade)) {
                int lowestgradeValue = metadata.Grades.Keys.Min();
                grade = metadata.Grades[lowestgradeValue];
            }

            if (!grade.Condition.Check(session, targetString, targetLong, codeString, codeLong)) {
                continue;
            }

            if (achievement == null) {
                achievement = new Achievement(metadata) {
                    CurrentGrade = grade.Grade,
                    RewardGrade = grade.Grade,
                };
                GameStorage.Request db = session.GameStorage.Context();
                achievement = db.CreateAchievement(metadata.AccountWide ? session.AccountId : session.CharacterId, achievement);
                if (achievement == null) {
                    throw new InvalidOperationException($"Failed to create achievement: {metadata.Id}");
                }
                achievements.Add(metadata.Id, achievement);
            }

            if (!RankUp(achievement, count)) {
                session.Send(AchievementPacket.Update(achievement));
            }
        }
    }

    /// <summary>
    /// Checks if trophy has reached a new grade. Provides rewards only on certain reward types.
    /// </summary>
    /// <param name="achievement">Trophy entry from player</param>
    /// <param name="count">Count amount to increment on for the trophy.</param>
    /// <returns>False if there is no rank up possible or condition value has not been met.</returns>
    private bool RankUp(Achievement achievement, long count = 1) {
        achievement.Counter += count;

        if (!achievement.Metadata.Grades.TryGetValue(achievement.CurrentGrade, out AchievementMetadataGrade? grade)) {
            return false;
        }

        int newGradesCount = 0;
        while (achievement.Counter >= grade.Condition.Value) {
            if (achievement.Completed) {
                break;
            }
            achievement.Grades.Add(achievement.CurrentGrade, DateTime.Now.ToEpochSeconds());
            newGradesCount++;

            if (achievement.Grades.Count < achievement.Metadata.Grades.Count) {
                achievement.CurrentGrade++;
            }

            // Update count on player
            switch (achievement.Category) {
                case AchievementCategory.Combat:
                    session.Player.Value.Character.AchievementInfo.Combat++;
                    break;
                case AchievementCategory.Adventure:
                    session.Player.Value.Character.AchievementInfo.Adventure++;
                    break;
                case AchievementCategory.Life:
                    session.Player.Value.Character.AchievementInfo.Lifestyle++;
                    break;
                default:
                    continue; // Invalid category, just skip.
            }

            GiveReward(achievement);

            session.Send(AchievementPacket.Update(achievement));
            if (!achievement.Metadata.Grades.TryGetValue(achievement.CurrentGrade, out grade)) {
                break;
            }
        }

        if (newGradesCount <= 0) {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gives rewards (if applicable) from awarded trophy grades. If there is no reward, it'll increment up on the reward grade. Will not increment if there is a item, title, skill point, or attribute point to give.
    /// </summary>
    /// <param name="achievement">Achievement entry from user</param>
    /// <param name="manualClaim">If true, assumes player requested the reward. it will give the player rewards for items, titles, skill points, and attribute points.
    /// These are never given automatically upon newly awarded trophy grade.</param>
    private void GiveReward(Achievement achievement, bool manualClaim = false) {
        if (achievement.CurrentGrade < achievement.RewardGrade) {
            return;
        }

        if (!achievement.Metadata.Grades.TryGetValue(achievement.RewardGrade, out AchievementMetadataGrade? grade)) {
            return;
        }

        if (grade.Reward == null) {
            achievement.RewardGrade++;
            return;
        }

        switch (grade.Reward.Type) {
            case AchievementRewardType.item:
                if (!manualClaim) {
                    return;
                }
                Item? item = session.Field.ItemDrop.CreateItem(grade.Reward.Code, grade.Reward.Rank, grade.Reward.Value);
                if (item == null) {
                    return;
                }
                if (!session.Item.Inventory.Add(item, true)) {
                    session.Item.MailItem(item);
                }
                break;
            case AchievementRewardType.title:
                if (!manualClaim) {
                    return;
                }
                if (session.Player.Value.Unlock.Titles.Contains(grade.Reward.Code)) {
                    break;
                }
                session.Send(UserEnvPacket.AddTitle(grade.Reward.Code));
                session.Player.Value.Unlock.Titles.Add(grade.Reward.Code);
                break;
            case AchievementRewardType.dynamicaction:
                if (session.Player.Value.Unlock.Emotes.Contains(grade.Reward.Code)) {
                    break;
                }
                session.Player.Value.Unlock.Emotes.Add(grade.Reward.Code);
                session.Send(EmotePacket.Learn(new Emote(grade.Reward.Code)));
                break;
            case AchievementRewardType.skillpoint:
                session.Config.AddSkillPoint(SkillPointSource.Trophy, grade.Reward.Value, (short) (grade.Reward.Rank - 1));
                break;
            case AchievementRewardType.statpoint:
                session.Config.AddStatPoint(AttributePointSource.Trophy, grade.Reward.Value);
                break;
            case AchievementRewardType.beauty_hair:
            case AchievementRewardType.beauty_makeup:
            case AchievementRewardType.beauty_skin:
            case AchievementRewardType.itemcoloring:
            case AchievementRewardType.shop_build:
            case AchievementRewardType.shop_ride:
            case AchievementRewardType.shop_weapon:
            case AchievementRewardType.etc: // currently used as quest unlocks
                // I don't think anything is supposed to happen here. Just client sided visuals?
                break;
            default:
                logger.Error("Unimplemented trophy reward type {RewardType}", grade.Reward.Type);
                break;
        }
        achievement.RewardGrade++;
    }

    /// <summary>
    /// Claims rewards from trophy grades. If there is no reward, it'll increment up on the reward grade. Loops until it catches up to the current grade + 1.
    /// </summary>
    /// <param name="achievementId"></param>
    public void ClaimReward(int achievementId) {
        if (!TryGetAchievement(achievementId, out Achievement? achievement)) {
            return;
        }

        for (int startGrade = achievement.RewardGrade; startGrade <= achievement.CurrentGrade; startGrade++) {
            GiveReward(achievement, true);
            session.Send(AchievementPacket.Update(achievement));
        }
    }

    public bool HasAchievement(int achievementId, int grade = -1) {
        if (!TryGetAchievement(achievementId, out Achievement? achievement)) {
            return false;
        }

        if (grade <= 0) {
            return achievement.Grades.Count > 0;
        }

        return achievement.Grades.ContainsKey(grade);
    }

    public void Save(GameStorage.Request db) {
        db.SaveAchievements(session.AccountId, accountValues.Values.ToList());
        db.SaveAchievements(session.CharacterId, characterValues.Values.ToList());
    }
}