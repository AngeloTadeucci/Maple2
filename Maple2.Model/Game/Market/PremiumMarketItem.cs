using System.Collections.Generic;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;

namespace Maple2.Model.Game;

public class PremiumMarketItem(ItemMetadata metadata) : MarketItem(metadata) {
    public required int Id { get; init; }
    public required int ParentId { get; init; }
    public required MeretMarketItemLabel Label { get; init; }
    public required MeretMarketCurrencyType CurrencyType { get; init; }
    public required long SalePrice { get; init; }
    public required long SellBeginTime { get; init; }
    public required long SellEndTime { get; init; }
    public required JobFilterFlag JobRequirement { get; init; }
    public required bool RestockUnavailable { get; init; }
    public required short RequireMinLevel { get; init; }
    public required short RequireMaxLevel { get; init; }
    public required byte Rarity { get; init; }
    public required int Quantity { get; init; }
    public required int ItemDuration { get; init; } // in days
    public required int BonusQuantity { get; init; }
    public required MeretMarketBannerLabel BannerLabel { get; init; }
    public required string BannerName { get; init; }
    public required int RequireAchievementId { get; init; }
    public required int RequireAchievementRank { get; init; }
    public required bool PcCafe { get; init; }
    public required bool Giftable { get; init; }
    public required PremiumMarketPromoData? PromoData { get; init; }
    public required bool ShowSaleTime { get; init; }
    public IList<PremiumMarketItem> AdditionalQuantities { get; set; } = new List<PremiumMarketItem>();

    public override void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteByte();
        writer.WriteUnicodeString(Name);
        writer.WriteBool(true);
        writer.WriteInt(ParentId);
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteByte();
        writer.Write<MeretMarketItemLabel>(Label);
        writer.Write<MeretMarketCurrencyType>(CurrencyType);
        writer.WriteLong(Price);
        writer.WriteLong(SalePrice);
        writer.WriteBool(Giftable);
        writer.WriteLong(SellBeginTime);
        writer.WriteLong(SellEndTime);
        writer.WriteInt(); // Another flag
        writer.WriteInt();
        writer.WriteBool(RestockUnavailable);
        writer.WriteInt();
        writer.WriteByte();
        writer.WriteShort(RequireMinLevel);
        writer.WriteShort(RequireMaxLevel);
        writer.Write<JobFilterFlag>(JobRequirement);
        writer.WriteInt(ItemMetadata.Id);
        writer.WriteByte(Rarity);
        writer.WriteInt(Quantity);
        writer.WriteInt(ItemDuration);
        writer.WriteInt(BonusQuantity);
        writer.WriteInt(TabId);
        writer.WriteInt();
        writer.WriteByte();
        writer.Write<MeretMarketBannerLabel>(BannerLabel);
        writer.WriteString(BannerName);
        writer.WriteString();
        writer.WriteByte();
        writer.WriteByte();
        writer.WriteInt();
        writer.WriteByte();
        writer.WriteInt(RequireAchievementId);
        writer.WriteInt(RequireAchievementRank);
        writer.WriteInt();
        writer.WriteBool(PcCafe);
        writer.WriteByte();
        writer.WriteInt();

    }
}
