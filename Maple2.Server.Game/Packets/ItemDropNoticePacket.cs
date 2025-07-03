using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class ItemDropNoticePacket {
    public static ByteWriter Notice(string playerName, Item item) {
        var pWriter = Packet.Of(SendOp.ItemDropNotice);
        pWriter.WriteUnicodeString(playerName);
        pWriter.WriteInt(item.Amount);
        pWriter.WriteInt(item.Rarity);
        pWriter.WriteLong(item.Uid);

        return pWriter;
    }
}
