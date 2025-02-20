using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class EnterUgcMapPacket {
    private enum Command : byte {
        // NoOtherHousesToVisit = 0,
        // WaitingToBeSold = 2,
        RequestPassword = 3,
        WrongPassword = 4,
        // EmptyMessageBox = 5,
        CannotVisitThatCharacterHome = 6,
        // CannotVisitAHomeRightNow = 7,
        // CannotVisitAHomeFromHere = 8,
        // CannotVisitAHomeWhileTombstoned = 9,
        // YouCannotMoveToTheHouse = 10,
    }

    public static ByteWriter RequestPassword(long accountId) {
        var pWriter = Packet.Of(SendOp.EnterUgcMap);
        pWriter.Write(Command.RequestPassword);
        pWriter.WriteInt();
        pWriter.WriteLong();
        pWriter.WriteLong(accountId);
        pWriter.WriteInt();
        pWriter.WriteByte(1); // When submitting the password:
        // if its 0 it'll request for MoveFieldHandler Mode 0 (Portal)
        // if its 1 it'll request for MoveFieldHandler Mode 2 (VisitHome)

        return pWriter;
    }

    public static ByteWriter WrongPassword(long accountId) {
        var pWriter = Packet.Of(SendOp.EnterUgcMap);
        pWriter.Write(Command.WrongPassword);
        pWriter.WriteInt();
        pWriter.WriteLong();
        pWriter.WriteLong(accountId);

        return pWriter;
    }

    public static ByteWriter CannotVisitThatCharacterHome() {
        var pWriter = Packet.Of(SendOp.EnterUgcMap);
        pWriter.Write(Command.CannotVisitThatCharacterHome);

        return pWriter;
    }
}
