using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game;

public class UgcMarketItem(ItemMetadata metadata) : MarketItem(metadata) {
    public long Id { get; init; }
    public required UgcMarketListingStatus Status { get; set; }
    public required long ListingEndTime { get; set; }
    public required long PromotionEndTime { get; set; }
    public required long SellerAccountId { get; init; }
    public required long SellerCharacterId { get; init; }
    public required string SellerCharacterName { get; init; }
    public required string Description { get; set; }
    public required string[] Tags { get; set; } = [];
    public required UgcItemLook Look { get; init; }
    public required ItemBlueprint Blueprint { get; init; }
    public UgcItemMusicScore MusicScore = new(); // TODO: Implement UgcItemMusicScore
    public UgcMarketHomeCategory Category = UgcMarketHomeCategory.None;

    public override void WriteTo(IByteWriter writer) {
        writer.WriteInt();
        writer.WriteLong(Id);
        writer.WriteInt();
        writer.WriteLong(Id);
        writer.Write<UgcMarketListingStatus>(Status);
        writer.WriteInt(ItemMetadata.Id);
        writer.WriteInt(TabId);
        writer.Write<GenderFilterFlag>(ItemMetadata.Limit.Gender.FilterFlag());
        writer.WriteInt();
        writer.WriteLong(Price);
        writer.WriteInt();
        writer.WriteInt();
        writer.WriteInt(SalesCount);
        writer.WriteInt();
        writer.WriteLong(CreationTime);
        writer.WriteLong(CreationTime);
        writer.WriteLong(ListingEndTime);
        writer.WriteInt();
        writer.WriteLong(PromotionEndTime);
        writer.WriteLong();
        writer.WriteLong(CreationTime);
        writer.WriteInt();
        writer.WriteLong(SellerAccountId);
        writer.WriteLong(SellerCharacterId);
        writer.WriteUnicodeString();
        writer.WriteUnicodeString(SellerCharacterName);
        writer.WriteUnicodeString(string.Join(",", string.Join(",", Tags) + ", " + Look.Name));
        writer.WriteUnicodeString(Description);
        writer.WriteUnicodeString(SellerCharacterName);
        writer.WriteClass<UgcItemLook>(Look);
        writer.WriteClass<UgcItemMusicScore>(MusicScore);
        writer.WriteClass<ItemBlueprint>(Blueprint);
        writer.Write<UgcMarketHomeCategory>(Category);
    }
}
