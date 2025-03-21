using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class DungeonWaitingPacket {
    public static ByteWriter Set(int dungeonId, int size) {
        var pWriter = Packet.Of(SendOp.DungeonWaiting);
        pWriter.WriteInt(dungeonId);
        pWriter.WriteInt(size);

        return pWriter;
    }
}
