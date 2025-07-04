using Maple2.Model.Error;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class AttendancePacket {
    private enum Command : byte {
        Unknown5 = 5,
        Unknown6 = 6,
        Unknown7 = 7,
        Error = 9,
    }

    public static ByteWriter Unknown5() {
        var pWriter = Packet.Of(SendOp.Attendance);
        pWriter.Write<Command>(Command.Unknown5);

        return pWriter;
    }

    public static ByteWriter Unknown6() {
        var pWriter = Packet.Of(SendOp.Attendance);
        pWriter.Write<Command>(Command.Unknown6);
        pWriter.WriteInt(); // count

        // foreach (int i in count) {
        //     pWriter.WriteInt(Id);
        //     pWriter.WriteLong(Timestamp);
        // }

        return pWriter;
    }

    public static ByteWriter Unknown7() {
        var pWriter = Packet.Of(SendOp.Attendance);
        pWriter.Write<Command>(Command.Unknown7);
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter Error(AttendanceError error) {
        var pWriter = Packet.Of(SendOp.Attendance);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<AttendanceError>(error);

        return pWriter;
    }
}
