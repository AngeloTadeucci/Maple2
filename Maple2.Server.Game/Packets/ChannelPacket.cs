using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class ChannelPacket {
    private enum Command : byte {
        Load = 0,
        Update = 1,
    }

    public static ByteWriter Load(ICollection<int> channels) {
        short count = (short) channels.Count;

        var pWriter = Packet.Of(SendOp.DynamicChannel);
        pWriter.Write<Command>(Command.Load);
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
        foreach (int channel in channels) {
            pWriter.WriteShort((short) channel);
        }

        return pWriter;
    }
}
