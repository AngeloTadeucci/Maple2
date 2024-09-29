using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record WeddingRewardTable(IReadOnlyDictionary<MarriageExpType, WeddingReward> Entries) : Table;

public record WeddingReward(
    MarriageExpType Type,
    int Amount,
    MarriageExpLimit Limit);
