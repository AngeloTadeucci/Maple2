using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game.Shop;

public class BeautyShop : IByteSerializable {
    public int Id => Metadata.Id;
    public readonly BeautyShopMetadata Metadata;
    public BeautyShopType Type { get; init; }
    public BeautyShopItem[] Items { get; init; }

    public BeautyShop(BeautyShopMetadata metadata, BeautyShopItem[] items) {
        Metadata = metadata;
        Items = items;
        Type = BeautyShopType.Default;
    }

    public virtual void WriteTo(IByteWriter writer) {
        writer.Write<BeautyShopType>(Type);
        writer.WriteInt(Id);
        writer.Write<BeautyShopCategory>(Metadata.Category);
        writer.WriteInt(Metadata.CouponId);
        writer.WriteByte(); // Related to random hair tickets
        writer.WriteInt(Metadata.ReturnCouponId);
        writer.WriteInt(Metadata.SubType);
        writer.WriteByte();
        writer.WriteClass<BeautyShopCost>(Metadata.StyleCostMetadata);
        writer.WriteClass<BeautyShopCost>(Metadata.ColorCostMetadata);
    }
}
