using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;

namespace Maple2.Model.Game;

public class PremiumMarketItem : MarketItem {
    public int Id => Metadata.Id;
    public readonly MeretMarketItemMetadata Metadata;
    public readonly PremiumMarketPromoData? PromoData;
    public IList<PremiumMarketItem> AdditionalQuantities { get; set; }

    public PremiumMarketItem(MeretMarketItemMetadata marketItemMetadata, ItemMetadata metadata) : base(metadata) {
        AdditionalQuantities = new List<PremiumMarketItem>();
        Metadata = marketItemMetadata;
        PromoData = new PremiumMarketPromoData {
            Name = Metadata.PromoName,
            StartTime = Metadata.PromoStartTime,
            EndTime = Metadata.PromoEndTime,
        };
        TabId = Metadata.TabId;
        Price = Metadata.Price;
    }

    public override void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteByte();
        writer.WriteUnicodeString(Name);
        writer.WriteBool(true);
        writer.WriteInt(Metadata.ParentId);
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteByte();
        writer.Write<MeretMarketItemSaleTag>(Metadata.SaleTag);
        writer.Write<MeretMarketCurrencyType>(Metadata.CurrencyType);
        writer.WriteLong(Price);
        writer.WriteLong(Metadata.SalePrice);
        writer.WriteBool(Metadata.Giftable);
        writer.WriteLong(Metadata.SaleStartTime);
        writer.WriteLong(Metadata.SaleEndTime);
        writer.WriteInt(); // Another flag
        writer.WriteInt();
        writer.WriteBool(Metadata.RestockUnavailable);
        writer.WriteInt();
        writer.WriteByte();
        writer.WriteShort(Metadata.RequireMinLevel);
        writer.WriteShort(Metadata.RequireMaxLevel);
        writer.Write<JobFilterFlag>(Metadata.JobRequirement);
        writer.WriteInt(ItemMetadata.Id);
        writer.WriteByte(Metadata.Rarity);
        writer.WriteInt(Metadata.Quantity);
        writer.WriteInt(Metadata.DurationInDays);
        writer.WriteInt(Metadata.BonusQuantity);
        writer.WriteInt(TabId);
        writer.WriteInt();
        writer.WriteByte();
        writer.Write<MeretMarketBannerTag>(Metadata.BannerTag);
        writer.WriteString(Metadata.Banner);
        writer.WriteString();
        writer.WriteByte();
        writer.WriteByte();
        writer.WriteInt();
        writer.WriteByte();
        writer.WriteInt(Metadata.RequireAchievementId);
        writer.WriteInt(Metadata.RequireAchievementRank);
        writer.WriteInt();
        writer.WriteBool(Metadata.PcCafe);
        writer.WriteByte();
        writer.WriteInt();

    }
}
