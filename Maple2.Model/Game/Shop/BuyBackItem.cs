using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Tools;
using Maple2.Tools.Extensions;

public class BuyBackItem : IByteSerializable {
    public int Id { get; init; }
    public long AddedTime { get; init; }
    public long Price { get; init; }
    public required Item Item { get; init; }

    public virtual void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteInt(Item.Id);
        writer.WriteByte((byte) Item.Rarity);
        writer.WriteLong(Price);
        writer.WriteClass<Item>(Item);
    }
}
