using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class InGameRankPacket {

    public static ByteWriter Load() {
        var pWriter = Packet.Of(SendOp.InGameRank);
        pWriter.WriteByte(31);
        pWriter.WriteInt(120);
        pWriter.WriteInt(60);

        return pWriter;
    }
}
