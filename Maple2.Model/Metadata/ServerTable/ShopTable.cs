using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record ShopTable(IReadOnlyDictionary<int, ShopMetadata> Entries) : ServerTable;

public record ShopMetadata(
    int Id,
    int CategoryId,
    string Name,
    ShopFrameType FrameType,
    bool DisplayOnlyUsable,
    bool HideStats,
    bool DisplayProbability,
    bool IsOnlySell,
    bool OpenWallet,
    bool DisplayNew,
    bool DisableDisplayOrderSort,
    long RestockTime,
    bool EnableReset,
    ShopRestockData RestockData);

public record ShopRestockData(
    ResetType ResetType,
    ShopCurrencyType CurrencyType,
    ShopCurrencyType ExcessCurrencyType,
    int MinItemCount,
    int MaxItemCount,
    int Price,
    bool EnablePriceMultiplier,
    bool DisableInstantRestock,
    bool AccountWide);
