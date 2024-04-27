using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Shop;

public class BeautyShopCost : IByteSerializable {
    public static BeautyShopCost Zero = new BeautyShopCost(ShopCurrencyType.Meso, 0, 0);

    public ShopCurrencyType Type { get; init; }
    public int ItemId { get; init; }
    public int Amount { get; init; }
    public string ItemIdIconCode { get; init; }

    public BeautyShopCost(ShopCurrencyType type, int itemId, int amount) {
        Type = type;
        ItemId = itemId;
        Amount = amount;

        // TODO: This is a hack to get the icon code for the item ID. This should be replaced with a proper lookup.
        ItemIdIconCode = ItemId is >= 20500001 and < 20600001 ? "20500001" : string.Empty;
    }

    public void WriteTo(IByteWriter writer) {
        writer.Write<ShopCurrencyType>(Type);
        writer.WriteInt(ItemId);
        writer.WriteInt(Amount);
        writer.WriteString(ItemIdIconCode);
    }
}
