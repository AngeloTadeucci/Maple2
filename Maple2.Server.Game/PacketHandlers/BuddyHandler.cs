using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class BuddyHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Buddy;

    private enum Command : byte {
        Request = 2,
        Accept = 3,
        Decline = 4,
        Block = 5,
        Unblock = 6,
        Delete = 7,
        UpdateBlock = 10,
        Cancel = 17,
        Unknown = 20,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Request:
                Invite(session, packet);
                return;
            case Command.Accept:
                Accept(session, packet);
                break;
            case Command.Decline:
                Decline(session, packet);
                break;
            case Command.Block:
                Block(session, packet);
                break;
            case Command.Unblock:
                Unblock(session, packet);
                break;
            case Command.Delete:
                Delete(session, packet);
                break;
            case Command.UpdateBlock:
                UpdateBlock(session, packet);
                break;
            case Command.Cancel:
                Cancel(session, packet);
                break;
            case Command.Unknown:
                Logger.Information("BUDDY(20) = {Value}", packet.ReadInt());
                break;
        }
    }

    private static void Invite(GameSession session, IByteReader packet) {
        string name = packet.ReadUnicodeString();
        string message = packet.ReadUnicodeString();

        session.Buddy.SendInvite(name, message);
    }

    private static void Accept(GameSession session, IByteReader packet) {
        long entryId = packet.ReadLong();

        session.Buddy.SendAccept(entryId);
    }

    private static void Decline(GameSession session, IByteReader packet) {
        long entryId = packet.ReadLong();

        session.Buddy.SendDecline(entryId);
    }

    private static void Block(GameSession session, IByteReader packet) {
        long entryId = packet.ReadLong();
        string name = packet.ReadUnicodeString();
        string message = packet.ReadUnicodeString();

        session.Buddy.SendBlock(entryId, name, message);
    }

    private static void Unblock(GameSession session, IByteReader packet) {
        long entryId = packet.ReadLong();

        session.Buddy.Unblock(entryId);
    }

    private static void Delete(GameSession session, IByteReader packet) {
        long entryId = packet.ReadLong();

        session.Buddy.SendDelete(entryId);
    }

    private static void UpdateBlock(GameSession session, IByteReader packet) {
        long entryId = packet.ReadLong();
        string name = packet.ReadUnicodeString();
        string message = packet.ReadUnicodeString();

        session.Buddy.UpdateBlock(entryId, name, message);
    }

    private static void Cancel(GameSession session, IByteReader packet) {
        long entryId = packet.ReadLong();

        session.Buddy.SendCancel(entryId);
    }
}
