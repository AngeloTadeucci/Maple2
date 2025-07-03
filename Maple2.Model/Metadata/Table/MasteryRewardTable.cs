using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record MasteryRewardTable(IReadOnlyDictionary<MasteryType, IReadOnlyDictionary<int, MasteryRewardTable.Entry>> Entries) : Table {
    public record Entry(int Value, int ItemId, int ItemRarity, int ItemAmount);
}
