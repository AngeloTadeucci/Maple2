using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record PrestigeLevelRewardTable(IReadOnlyDictionary<int, PrestigeLevelRewardMetadata> Entries) : Table;

public record PrestigeLevelRewardMetadata(
    int Level,
    PrestigeAwardType Type,
    int Id,
    short Rarity,
    int Value);
