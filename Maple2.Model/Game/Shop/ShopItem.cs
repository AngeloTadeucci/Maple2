using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game.Shop;

public class ShopItem : IByteSerializable {
    public readonly ShopItemMetadata Metadata;
    public int Id => Metadata.Id;
    public int StockPurchased { get; set; }
    public int StockCount { get; set; }
    public required Item Item { get; init; }

    public ShopItem(ShopItemMetadata metadata) {
        Metadata = metadata;
        StockCount = metadata.SellCount;
    }

    public ShopItem Clone() {
        return new ShopItem(Metadata) {
            Item = Item.Clone(),
            StockCount = StockCount,
            StockPurchased = StockPurchased,
        };
    }

    public virtual void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteInt(Metadata.ItemId);
        writer.WriteClass<ShopCost>(Metadata.Cost);
        writer.WriteByte(Metadata.Rarity);
        writer.WriteInt(500);
        writer.WriteInt(StockCount);
        writer.WriteInt(StockPurchased * Metadata.SellUnit);
        writer.WriteInt(Metadata.Requirements.GuildTrophy);
        writer.WriteString(Metadata.Category);
        writer.WriteInt(Metadata.Requirements.Achievement.Id);
        writer.WriteInt(Metadata.Requirements.Achievement.Rank);
        writer.WriteByte(Metadata.Requirements.Championship.Rank);
        writer.WriteShort(Metadata.Requirements.Championship.JoinCount);
        writer.WriteByte((byte) Metadata.Requirements.GuildNpc.Type);
        writer.WriteShort(Metadata.Requirements.GuildNpc.Level);
        writer.WriteBool(false);
        writer.WriteShort(Metadata.SellUnit);
        writer.WriteByte();
        writer.Write<ShopItemLabel>(Metadata.Label);
        writer.WriteString(Metadata.IconTag);
        writer.Write<ReputationType>(Metadata.Requirements.QuestAlliance.Type);
        writer.WriteInt(Metadata.Requirements.QuestAlliance.Grade);
        writer.WriteBool(Metadata.WearForPreview);
        writer.WriteBool(Metadata.RestrictedBuyData != null);
        if (Metadata.RestrictedBuyData != null) {
            writer.WriteClass<Game.Shop.RestrictedBuyData>(Metadata.RestrictedBuyData);
        }

        writer.WriteClass<Item>(Item);
    }
}
