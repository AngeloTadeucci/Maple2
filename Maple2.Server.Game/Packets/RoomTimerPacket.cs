using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;

namespace Maple2.Server.Game.Packets;

public static class RoomTimerPacket {
    private enum Command : byte {
        Start = 0,
        Modify = 1,
    }

    public static ByteWriter Start(RoomTimer timer) {
        var pWriter = Packet.Of(SendOp.RoomTimer);
        pWriter.Write<Command>(Command.Start);
        pWriter.Write<RoomTimerType>(timer.Type);
        pWriter.WriteInt(timer.StartTick);
        pWriter.WriteInt(timer.Duration);
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter Modify(RoomTimer timer, int delta) {
        var pWriter = Packet.Of(SendOp.RoomTimer);
        pWriter.Write<Command>(Command.Modify);
        pWriter.WriteInt();
        pWriter.WriteInt();
        pWriter.WriteInt((int) timer.Type);
        pWriter.WriteInt(delta);

        return pWriter;
    }
}
