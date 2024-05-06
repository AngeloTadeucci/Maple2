using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game.Shop;

public class ShopItem(int id) : IByteSerializable {
    public readonly int Id = id;
    public int ItemId { get; init; }
    public ShopCost Cost { get; init; } = ShopCost.Zero;
    public byte Rarity { get; init; }
    public int StockCount { get; init; }
    public int StockPurchased { get; set; }
    public string Category { get; init; }
    public int RequireGuildTrophy { get; init; }
    public int RequireAchievementId { get; init; }
    public int RequireAchievementRank { get; init; }
    public byte RequireChampionshipGrade { get; init; }
    public short RequireChampionshipJoinCount { get; init; }
    public byte RequireGuildMerchantType { get; init; }
    public short RequireGuildMerchantLevel { get; init; }
    public short Quantity { get; init; }
    public ShopItemLabel Label { get; init; }
    public string IconCode { get; init; }
    public short RequireQuestAllianceId { get; init; }
    public int RequireFameGrade { get; init; }
    public bool AutoPreviewEquip { get; init; }
    public RestrictedBuyData? RestrictedBuyData { get; init; }
    public Item Item;

    public ShopItem Clone() {
        return new ShopItem(Id) {
            AutoPreviewEquip = AutoPreviewEquip,
            Category = Category,
            Cost = Cost,
            IconCode = IconCode,
            Item = Item.Clone(),
            ItemId = ItemId,
            Label = Label,
            Quantity = Quantity,
            Rarity = Rarity,
            RequireAchievementId = RequireAchievementId,
            RequireAchievementRank = RequireAchievementRank,
            RequireChampionshipGrade = RequireChampionshipGrade,
            RequireChampionshipJoinCount = RequireChampionshipJoinCount,
            RequireFameGrade = RequireFameGrade,
            RequireGuildMerchantLevel = RequireGuildMerchantLevel,
            RequireGuildMerchantType = RequireGuildMerchantType,
            RequireGuildTrophy = RequireGuildTrophy,
            RequireQuestAllianceId = RequireQuestAllianceId,
            StockCount = StockCount,
            StockPurchased = StockPurchased,
            RestrictedBuyData = RestrictedBuyData?.Clone(),
        };
    }

    public virtual void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteInt(ItemId);
        writer.WriteClass<ShopCost>(Cost);
        writer.WriteByte(Rarity);
        writer.WriteInt();
        writer.WriteInt(StockCount);
        writer.WriteInt(StockPurchased * Quantity);
        writer.WriteInt(RequireGuildTrophy);
        writer.WriteString(Category);
        writer.WriteInt(RequireAchievementId);
        writer.WriteInt(RequireAchievementRank);
        writer.WriteByte(RequireChampionshipGrade);
        writer.WriteShort(RequireChampionshipJoinCount);
        writer.WriteByte(RequireGuildMerchantType);
        writer.WriteShort(RequireGuildMerchantLevel);
        writer.WriteBool(false);
        writer.WriteShort(Quantity);
        writer.WriteByte();
        writer.Write<ShopItemLabel>(Label);
        writer.WriteString(IconCode);
        writer.WriteShort(RequireQuestAllianceId);
        writer.WriteInt(RequireFameGrade);
        writer.WriteBool(AutoPreviewEquip);
        writer.WriteBool(RestrictedBuyData != null);
        if (RestrictedBuyData != null) {
            writer.WriteClass<Game.Shop.RestrictedBuyData>(RestrictedBuyData);
        }

        writer.WriteClass<Item>(Item);
    }
}
