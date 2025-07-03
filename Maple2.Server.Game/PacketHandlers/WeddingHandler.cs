using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.PacketHandlers;

public class WeddingHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Wedding;

    private enum Command : byte {
        Propose = 3,
        ProposalReply = 4,
        CancelProposal = 5,
        DivorceReply = 8,
        ReserveHall = 13,
        ReserveHallReply = 14,
        CancelReservation = 16,
        CancelNoRefundReply = 17,
        CancelReservationReply = 18,
        ChangeReservation = 20,
        ChangeReservationReply = 21,
        EnterCeremony = 26,
        InvitationFindUser = 27,
        SendInvitation = 28,
        StartCeremony = 30,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();

        switch (command) {
            case Command.Propose:
                HandlePropose(session, packet);
                return;
            case Command.ProposalReply:
                HandleProposalReply(session, packet);
                return;
            case Command.ReserveHall:
                HandleReserveHall(session, packet);
                return;
            case Command.ReserveHallReply:
                HandleReserveHallReply(session, packet);
                return;
            case Command.CancelReservation:
                HandleCancelReservation(session);
                return;
            case Command.CancelNoRefundReply:
                CancelNoRefundReply(session, packet);
                return;
            case Command.CancelReservationReply:
                HandleCancelReservationReply(session, packet);
                return;
            case Command.ChangeReservation:
                HandleChangeReservation(session, packet);
                return;
            case Command.ChangeReservationReply:
                HandleChangeReservationReply(session, packet);
                return;
            case Command.EnterCeremony:
                HandleEnterCeremony(session, packet);
                return;
            case Command.InvitationFindUser:
                HandleInvitationFindUser(session, packet);
                return;
            case Command.SendInvitation:
                HandleSendInvitation(session, packet);
                return;
        }
    }

    private static void HandlePropose(GameSession session, IByteReader packet) {
        if (session.Field is null) return;
        string otherPlayerName = packet.ReadUnicodeString();

        if (!session.Field.TryGetPlayer(otherPlayerName, out FieldPlayer? otherPlayer)) {
            session.Send(WeddingPacket.Error(WeddingError.s_wedding_result_err_same_field));
            return;
        }

        WeddingError error = session.Marriage.Propose(otherPlayer);

        session.Send(WeddingPacket.Error(error));
        if (error == WeddingError.s_wedding_msg_box_propose_to) {
            otherPlayer.Session.Send(WeddingPacket.Proposal(session.PlayerName, session.CharacterId));
            session.ConditionUpdate(ConditionType.wedding_propose);
        }
    }

    private static void HandleProposalReply(GameSession session, IByteReader packet) {
        if (session.Field is null) return;
        var response = packet.Read<WeddingError>();
        long playerId = packet.ReadLong();

        if (!session.PlayerInfo.GetOrFetch(playerId, out PlayerInfo? playerInfo) ||
            !session.Field.TryGetPlayer(playerInfo.Name, out FieldPlayer? otherPlayer)) {
            return;
        }

        if (response != WeddingError.s_wedding_propose_ok) {
            session.ConditionUpdate(ConditionType.wedding_propose_decline);
            otherPlayer.Session.ConditionUpdate(ConditionType.wedding_propose_declined);
            otherPlayer.Session.Send(WeddingPacket.Error(response));
            return;
        }

        WeddingError error = session.Marriage.AcceptProposal(otherPlayer);
        if (error != WeddingError.none) {
            session.Send(WeddingPacket.Error(error));
            otherPlayer.Session.Send(WeddingPacket.Error(WeddingError.s_wedding_result_decline_propose));
        }
    }

    private static void HandleReserveHall(GameSession session, IByteReader packet) {
        if (session.Marriage.Marriage.Status != MaritalStatus.Engaged) {
            session.Send(WeddingPacket.Error(WeddingError.s_wedding_result_err_only_promise_user));
            return;
        }

        if (session.Marriage.WeddingHall.Id != 0) {
            session.Send(WeddingPacket.Error(WeddingError.s_wedding_result_already_reservation));
            return;
        }

        var hall = packet.ReadClass<WeddingHall>();
        bool useCoupon = packet.ReadBool();

        WeddingError error = session.Marriage.ReserveHall(hall, useCoupon);
        session.Send(WeddingPacket.Error(error));
    }

    private static void HandleReserveHallReply(GameSession session, IByteReader packet) {
        var response = packet.Read<WeddingError>();

        if (response != WeddingError.confirm_reservation) {
            session.Send(WeddingPacket.Error(response));
            return;
        }

        WeddingError error = session.Marriage.ConfirmReservation();
        if (error != WeddingError.s_wedding_propose_ok) {
            session.Send(WeddingPacket.Error(error));
        }
    }

    private static void HandleCancelReservation(GameSession session) {
        if (session.Marriage.WeddingHall.Id == 0) {
            session.Send(WeddingPacket.Error(WeddingError.s_wedding_result_not_find_weddinghall));
            return;
        }

        session.Marriage.CancelReservationRequest();
    }

    private static void CancelNoRefundReply(GameSession session, IByteReader packet) {
        var response = packet.Read<WeddingError>();

        if (response == WeddingError.decline_reservation_cancel) {
            session.Send(WeddingPacket.DeclineCancelReservation());
            return;
        }

        WeddingError error = session.Marriage.CancelNoRefund();
        if (error != WeddingError.none) {
            session.Send(WeddingPacket.Error(error));
        }
    }

    private static void HandleCancelReservationReply(GameSession session, IByteReader packet) {
        var response = packet.Read<WeddingError>();

        WeddingError error = session.Marriage.CancelReservationReply(response);
        if (error != WeddingError.none) {
            session.Send(WeddingPacket.Error(error));
        }
    }

    private static void HandleChangeReservation(GameSession session, IByteReader packet) {
        var hall = packet.ReadClass<WeddingHall>();

        WeddingError error = session.Marriage.ChangeReservationRequest(hall);
        if (error != WeddingError.none) {
            session.Send(WeddingPacket.Error(error));
        }
    }

    private static void HandleChangeReservationReply(GameSession session, IByteReader packet) {
        var response = packet.Read<WeddingError>();

        WeddingError error = session.Marriage.ChangeReservationReply(response);
        if (error != WeddingError.none) {
            session.Send(WeddingPacket.Error(error));
        }
    }

    private static void HandleEnterCeremony(GameSession session, IByteReader packet) {
        long marriageId = packet.ReadLong();
        using GameStorage.Request db = session.GameStorage.Context();
        WeddingHall? hall = db.GetWeddingHall(marriageId: marriageId);
        if (hall == null) {
            session.Send(WeddingPacket.Error(WeddingError.s_wedding_result_err_invalid_wedding_hall));
            return;
        }

        DateTime ceremonyTime = hall.CeremonyTime.FromEpochSeconds();

        // Can enter only within 5 minutes before the ceremony
        if (DateTime.Now >= ceremonyTime.AddMinutes(-5)) {
            session.Send(WeddingPacket.Error(WeddingError.s_wedding_visit_failed_invalid_time));
            return;
        }

        // Cannot enter after 30 minutes from the ceremony scheduled time
        if (DateTime.Now > ceremonyTime.AddMinutes(30)) {
            session.Send(WeddingPacket.Error(WeddingError.s_wedding_visit_failed_common));
            return;
        }
    }

    private static void HandleInvitationFindUser(GameSession session, IByteReader packet) {
        string inviteeName = packet.ReadUnicodeString();
    }

    private static void HandleSendInvitation(GameSession session, IByteReader packet) {
        int inviteCount = packet.ReadInt();
    }
}
