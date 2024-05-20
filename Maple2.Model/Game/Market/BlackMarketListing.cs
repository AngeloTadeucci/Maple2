using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game;

public class BlackMarketListing : IByteSerializable {
    public long Id { get; init; }
    public Item Item { get; init; }
    public long CreationTime { get; init; }
    public long ExpiryTime { get; init; }
    public long Price { get; init; }
    public int Quantity { get; init; }
    public long AccountId { get; init; }
    public long CharacterId { get; init; }
    public long Deposit { get; init; }

    public BlackMarketListing(Item item) {
        Item = item;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteLong(Id);
        writer.WriteLong(CreationTime);
        writer.WriteLong(CreationTime);
        writer.WriteLong(ExpiryTime);
        writer.WriteInt(Item.Amount);
        writer.WriteInt();
        writer.WriteLong(Price);
        writer.WriteBool(false);
        writer.WriteLong(Item.Uid);
        writer.WriteInt(Item.Id);
        writer.WriteByte((byte) Item.Rarity);
        writer.WriteLong(AccountId);
        writer.WriteClass<Item>(Item);
    }
}
