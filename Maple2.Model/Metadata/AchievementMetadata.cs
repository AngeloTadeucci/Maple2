using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record AchievementMetadata(
    int Id,
    string? Name,
    bool AccountWide,
    AchievementCategory Category,
    string[] CategoryTags,
    IReadOnlyDictionary<int, AchievementMetadataGrade> Grades) : ISearchResult;

public record AchievementMetadataGrade(
    int Grade,
    ConditionMetadata Condition,
    AchievementMetadataReward? Reward);

public record AchievementMetadataReward(
    AchievementRewardType Type,
    int Code,
    int Value,
    int Rank);
