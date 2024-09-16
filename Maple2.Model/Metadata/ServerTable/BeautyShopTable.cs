using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record BeautyShopTable(IReadOnlyDictionary<int, BeautyShopMetadata> Entries) : ServerTable;

public record BeautyShopMetadata(
    int Id,
    BeautyShopCategory Category,
    int SubType,
    BeautyShopCostMetadata StyleCostMetadata,
    BeautyShopCostMetadata ColorCostMetadata,
    bool IsRandom,
    bool IsByItem,
    int ReturnCouponId,
    int CouponId,
    ItemTag CouponTag,
    BeautyShopItem[] Items,
    BeautyShopItemGroup[] ItemGroups);

public record BeautyShopCostMetadata(
    ShopCurrencyType CurrencyType,
    int Price,
    string Icon,
    int PaymentItemId);

public record BeautyShopItemGroup(
    long StartTime,
    BeautyShopItem[] Items);

public record BeautyShopItem(
    int Id,
    BeautyShopCostMetadata Cost,
    int Weight,
    int AchievementId,
    short AchievementRank,
    short RequiredLevel,
    ShopItemLabel SaleTag);
