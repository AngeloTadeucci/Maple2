using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class DeadUserPacket {
    public static ByteWriter Dead(int objectId, bool darkTombstone) {
        var pWriter = Packet.Of(SendOp.DeadUser);
        pWriter.WriteInt(objectId);
        pWriter.WriteBool(darkTombstone);

        return pWriter;
    }
}
