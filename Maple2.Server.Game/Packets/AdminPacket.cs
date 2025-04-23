using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class AdminPacket {

    public static ByteWriter Enable() {
        ByteWriter pWriter = Packet.Of(SendOp.Admin);
        pWriter.WriteByte();
        pWriter.WriteByte(255);

        return pWriter;
    }
}
