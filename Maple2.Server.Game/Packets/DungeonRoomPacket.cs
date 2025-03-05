using Maple2.Model.Error;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class DungeonRoomPacket {
    private enum Command : byte {
        Error = 18,
    }

    public static ByteWriter Error(DungeonRoomError error, int arg = 0) {
        var pWriter = Packet.Of(SendOp.RoomDungeon);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<DungeonRoomError>(error);
        pWriter.WriteInt(arg);

        return pWriter;
    }
}
