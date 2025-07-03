using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record MeretMarketTable(IReadOnlyDictionary<int, MeretMarketItemMetadata> Entries) : ServerTable;

public record MeretMarketItemMetadata(
    int Id,
    int ParentId,
    int TabId,
    string Banner,
    MeretMarketBannerTag BannerTag,
    int ItemId,
    byte Rarity,
    int Quantity,
    int BonusQuantity,
    int DurationInDays,
    MeretMarketItemSaleTag SaleTag,
    MeretMarketCurrencyType CurrencyType,
    long Price,
    long SalePrice,
    long SaleStartTime,
    long SaleEndTime,
    JobFilterFlag JobRequirement,
    bool RestockUnavailable,
    short RequireMinLevel,
    short RequireMaxLevel,
    int RequireAchievementId,
    int RequireAchievementRank,
    bool PcCafe,
    bool Giftable,
    bool ShowSaleTime,
    string PromoName,
    long PromoStartTime,
    long PromoEndTime);
