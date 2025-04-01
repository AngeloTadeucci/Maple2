using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record DungeonConfigTable(
    IReadOnlyDictionary<int, int> UnitedWeeklyRewards, // Key: RewardCount, Value: RewardId
    IReadOnlyDictionary<int, DungeonMissionRankMetadata> MissionRanks) : Table;

public record DungeonMissionRankMetadata(
    int Id,
    string Description,
    int MaxScore,
    DungeonMissionRankMetadata.Score[] Scores) {
    public record Score(
        DungeonMissionRank Grade,
        int Value);
}
