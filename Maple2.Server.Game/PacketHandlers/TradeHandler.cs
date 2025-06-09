using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using static Maple2.Model.Error.TradeError;

namespace Maple2.Server.Game.PacketHandlers;

public class TradeHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Trade;

    private enum Command : byte {
        Request = 0,
        Acknowledge = 2,
        Accept = 3,
        Decline = 4,
        Cancel = 7,
        AddItem = 8,
        RemoveItem = 9,
        SetMesos = 10,
        Finalize = 11,
        Complete = 13,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();

        switch (command) {
            case Command.Request:
                HandleRequest(session, packet);
                return;
            case Command.Acknowledge:
                HandleAcknowledge(session, packet);
                return;
            case Command.Accept:
                HandleAccept(session, packet);
                return;
            case Command.Decline:
                HandleDecline(session, packet);
                return;
            case Command.Cancel:
                HandleCancel(session);
                return;
            case Command.AddItem:
                HandleAddItem(session, packet);
                return;
            case Command.RemoveItem:
                HandleRemoveItem(session, packet);
                return;
            case Command.SetMesos:
                HandleSetMesos(session, packet);
                return;
            case Command.Finalize:
                HandleFinalize(session);
                return;
            case Command.Complete:
                HandleComplete(session);
                return;
        }
    }

    private static void HandleRequest(GameSession session, IByteReader packet) {
        if (session.Field == null) {
            return;
        }

        if (session.Trade != null) {
            session.Send(TradePacket.Error(s_trade_error_trading_now));
            return;
        }

        long characterId = packet.ReadLong();
        if (!session.Field.TryGetPlayerById(characterId, out FieldPlayer? target)) {
            session.Send(TradePacket.Error(s_trade_error_decline));
            return;
        }

        if (target.Session.Trade != null) {
            session.Send(TradePacket.Error(s_trade_error_already_request, name: target.Value.Character.Name));
            return;
        }

        session.Trade = new TradeManager(session, target.Session);
    }

    private static void HandleAcknowledge(GameSession session, IByteReader packet) {
        if (session.Field == null) {
            return;
        }

        if (session.Trade != null) {
            session.Send(TradePacket.Error(s_trade_error_trading_now));
            return;
        }

        long characterId = packet.ReadLong();
        if (!session.Field.TryGetPlayerById(characterId, out FieldPlayer? target) || target.Session.Trade == null) {
            session.Send(TradePacket.Error(s_trade_error_decline));
            return;
        }

        session.Trade = target.Session.Trade;
        session.Trade.Acknowledge(session);
    }

    private static void HandleAccept(GameSession session, IByteReader packet) {
        long characterId = packet.ReadLong();

        session.Trade?.Accept(session);
    }

    private static void HandleDecline(GameSession session, IByteReader packet) {
        long characterId = packet.ReadLong();

        session.Trade?.Decline(session);
    }

    private static void HandleCancel(GameSession session) {
        session.Trade?.Dispose();
    }

    private static void HandleAddItem(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        int amount = packet.ReadInt();
        int tradeSlot = packet.ReadInt();

        session.Trade?.AddItem(session, itemUid, amount, tradeSlot);
    }

    private static void HandleRemoveItem(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        int tradeSlot = packet.ReadInt();

        session.Trade?.RemoveItem(session, itemUid, tradeSlot);
    }

    private static void HandleSetMesos(GameSession session, IByteReader packet) {
        long amount = packet.ReadLong();

        session.Trade?.SetMesos(session, amount);
    }

    private static void HandleFinalize(GameSession session) {
        session.Trade?.Finalize(session);
    }

    private static void HandleComplete(GameSession session) {
        session.Trade?.Complete(session);
    }
}
