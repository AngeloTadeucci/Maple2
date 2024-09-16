using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record FurnishingShopTable(IReadOnlyDictionary<int, FurnishingShopTable.Entry> Entries) : Table {
    public record Entry(
        int ItemId,
        bool Buyable,
        FurnishingMoneyType FurnishingTokenType,
        int Price);
}
