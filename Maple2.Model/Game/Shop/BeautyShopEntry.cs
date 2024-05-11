using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game.Shop;

public class BeautyShopEntry : IByteSerializable {
    public int ItemId { get; init; }
    public BeautyShopCost Cost { get; init; }

    public ShopItemLabel Label { get; init; }
    public short RequireLevel { get; init; }
    public int RequireAchievementId { get; init; }
    public byte RequireAchievementRank { get; init; }

    public BeautyShopEntry(int itemId, BeautyShopCost cost) {
        ItemId = itemId;
        Cost = cost;
    }

    public BeautyShopEntry() {
        Cost = BeautyShopCost.Zero;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(ItemId);
        writer.Write<ShopItemLabel>(Label);
        writer.WriteShort(RequireLevel);
        writer.WriteInt(RequireAchievementId);
        writer.WriteByte(RequireAchievementRank);
        writer.WriteClass<BeautyShopCost>(Cost);
    }
}
