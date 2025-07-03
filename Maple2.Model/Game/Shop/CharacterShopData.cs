using Maple2.Model.Enum;

namespace Maple2.Model.Game.Shop;

public class CharacterShopData {
    public required int ShopId { get; init; }
    public long RestockTime { get; set; }
    public int RestockCount { get; set; }
    public ResetType Interval { get; init; }
}
