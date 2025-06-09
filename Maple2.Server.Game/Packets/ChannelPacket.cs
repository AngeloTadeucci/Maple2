using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class ChannelPacket {
    private enum Command : byte {
        Load = 0,
        Update = 1,
    }

    public static ByteWriter Load(ICollection<int> _) {
        var pWriter = Packet.Of(SendOp.DynamicChannel);
        pWriter.Write<Command>(Command.Load);
        // Unsure as to why these values seem to work properly for Update packet to update.
        pWriter.WriteShort(10);
        pWriter.WriteShort(100);
        pWriter.WriteShort(100);
        pWriter.WriteShort(100);
        pWriter.WriteShort(100);
        pWriter.WriteShort(10);
        pWriter.WriteShort(10);
        pWriter.WriteShort(10);

        return pWriter;
    }

    public static ByteWriter Update(ICollection<short> channels) {
        var pWriter = Packet.Of(SendOp.DynamicChannel);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteShort((short) channels.Count);
        foreach (short channel in channels) {
            pWriter.WriteShort(channel);
        }

        return pWriter;
    }
}
