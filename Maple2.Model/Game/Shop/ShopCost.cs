using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Shop;

public class ShopCost : IByteSerializable {
    public static readonly ShopCost Zero = new() {
        Type = ShopCurrencyType.Meso,
        ItemId = 0,
        Amount = 0,
        SaleAmount = 0,
    };

    public ShopCurrencyType Type { get; init; }
    public int ItemId { get; init; } = 0;
    public int Amount { get; init; }
    public int SaleAmount { get; init; } = 0;

    public void WriteTo(IByteWriter writer) {
        writer.Write<ShopCurrencyType>(Type);
        writer.WriteInt(ItemId);
        writer.WriteInt();
        writer.WriteInt(Amount);
        writer.WriteInt(SaleAmount);
    }
}
