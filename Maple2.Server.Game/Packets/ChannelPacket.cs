using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class ChannelPacket {
    public static ByteWriter Dynamic(ICollection<int> channels) {
        short count = (short) channels.Count;

        var pWriter = Packet.Of(SendOp.DynamicChannel);
        pWriter.WriteByte();
        pWriter.WriteShort(10);
        pWriter.WriteShort(count);
        pWriter.WriteShort(count);
        pWriter.WriteShort(count);
        pWriter.WriteShort(count);
        pWriter.WriteShort(10);
        pWriter.WriteShort(10);
        pWriter.WriteShort(10);

        return pWriter;
    }
}
