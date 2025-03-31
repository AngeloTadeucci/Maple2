using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class RoomStageDungeonPacket {
    private enum Command : byte {
        Set = 0,
        Update = 1,
    }

    public static ByteWriter Set(int dungeonId) {
        var pWriter = Packet.Of(SendOp.RoomStageDungeon);
        pWriter.Write<Command>(Command.Set);
        pWriter.WriteInt(dungeonId);
        pWriter.WriteInt();
        pWriter.WriteShort();

        return pWriter;
    }

    public static ByteWriter Update(int value) {
        var pWriter = Packet.Of(SendOp.RoomStageDungeon);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteInt(value);

        return pWriter;
    }
}
