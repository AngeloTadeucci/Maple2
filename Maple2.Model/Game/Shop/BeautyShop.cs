using System.Collections.Generic;
using Maple2.PacketLib.Tools;
using Maple2.Tools.Extensions;

namespace Maple2.Model.Game.Shop;

public sealed class BeautyShop(int id) : BeautyShopData(id) {
    public byte Unknown1 { get; init; }
    public byte Unknown2 { get; init; }
    public IList<BeautyShopEntry> Entries;

    public override void WriteTo(IByteWriter writer) {
        base.WriteTo(writer);
        writer.WriteByte(Unknown1);
        writer.WriteByte(Unknown2);
        writer.WriteShort((short) Entries.Count);
        foreach (BeautyShopEntry entry in Entries) {
            writer.WriteClass<BeautyShopEntry>(entry);
        }
    }
}
