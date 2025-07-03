using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Shop;

public class BeautyShopCost : IByteSerializable {
    public BeautyShopCostMetadata Metadata { get; init; }

    public BeautyShopCost(BeautyShopCostMetadata metadata) {
        Metadata = metadata;
    }

    public static implicit operator BeautyShopCost(BeautyShopCostMetadata other) {
        return new BeautyShopCost(other);
    }

    public void WriteTo(IByteWriter writer) {
        writer.Write<ShopCurrencyType>(Metadata.CurrencyType);
        writer.WriteInt(Metadata.PaymentItemId);
        writer.WriteInt(Metadata.Price);
        writer.WriteString(Metadata.Icon);
    }
}
