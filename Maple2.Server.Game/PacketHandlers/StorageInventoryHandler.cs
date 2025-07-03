using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Manager.Items;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class StorageInventoryHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.RequestItemStorage;

    private enum Command : byte {
        Deposit = 0,
        Withdraw = 1,
        Move = 2,
        Mesos = 3,
        Expand = 6,
        Sort = 8,
        Delete = 10,
        Load = 12,
        Close = 15,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Deposit:
                HandleDeposit(session, packet);
                return;
            case Command.Withdraw:
                HandleWithdraw(session, packet);
                return;
            case Command.Move:
                HandleMove(session, packet);
                return;
            case Command.Mesos:
                HandleMesos(session, packet);
                return;
            case Command.Expand:
                HandleExpand(session, packet);
                return;
            case Command.Sort:
                HandleSort(session, packet);
                return;
            case Command.Delete:
                HandleDelete(session, packet);
                return;
            case Command.Load:
                HandleLoad(session);
                return;
            case Command.Close:
                HandleClose(session);
                return;
        }
    }

    private static void HandleDeposit(GameSession session, IByteReader packet) {
        packet.ReadLong(); // 0
        long uid = packet.ReadLong();
        short slot = packet.ReadShort();
        int amount = packet.ReadInt();

        session.Storage?.Deposit(uid, slot, amount);
    }

    private static void HandleWithdraw(GameSession session, IByteReader packet) {
        packet.ReadLong(); // 0
        long uid = packet.ReadLong();
        short slot = packet.ReadShort();
        int amount = packet.ReadInt();

        session.Storage?.Withdraw(uid, slot, amount);
    }

    private static void HandleMove(GameSession session, IByteReader packet) {
        packet.ReadLong(); // 0
        long uid = packet.ReadLong();
        short dstSlot = packet.ReadShort();

        session.Storage?.Move(uid, dstSlot);
    }

    private static void HandleMesos(GameSession session, IByteReader packet) {
        packet.ReadLong(); // 0
        bool deposit = packet.ReadBool();
        long amount = packet.ReadLong();

        if (deposit) {
            session.Storage?.DepositMesos(amount);
        } else {
            session.Storage?.WithdrawMesos(amount);
        }
    }

    private static void HandleExpand(GameSession session, IByteReader packet) {
        packet.ReadLong(); // 0

        session.Storage?.Expand();
    }

    private static void HandleSort(GameSession session, IByteReader packet) {
        packet.ReadLong(); // 0

        session.Storage?.Sort();
    }

    private static void HandleDelete(GameSession session, IByteReader packet) {
        long uid = packet.ReadLong();

        session.Storage?.Delete(uid);
    }

    private static void HandleLoad(GameSession session) {
        if (session.Storage != null) {
            return;
        }

        session.Storage = new StorageManager(session);
        session.Storage.Load();
    }

    private static void HandleClose(GameSession session) {
        session.Storage?.Dispose();
    }
}
