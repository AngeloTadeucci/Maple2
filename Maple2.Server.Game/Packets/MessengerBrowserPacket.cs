using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class MessengerBrowserPacket {
    public static ByteWriter Link(params Item[] items) {
        var pWriter = Packet.Of(SendOp.MessengerBrowser);
        pWriter.WriteInt(items.Length);

        foreach (Item item in items) {
            pWriter.WriteLong(item.Uid);
            pWriter.WriteInt(item.Id);
            pWriter.WriteInt(item.Rarity);
            pWriter.WriteClass<Item>(item);
        }

        return pWriter;
    }
}
