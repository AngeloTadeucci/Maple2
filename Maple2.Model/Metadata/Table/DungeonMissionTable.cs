using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record DungeonMissionTable(IReadOnlyDictionary<int, DungeonMissionMetadata> Missions) : Table;

public record DungeonMissionMetadata(
    int Id,
    DungeonMissionType Type,
    long[] Value1, // Technically this is a float in the xml but the difference is negligible/impossible.
    long Value2,
    short MaxScore,
    short ApplyCount,
    bool IsPenaltyType);


