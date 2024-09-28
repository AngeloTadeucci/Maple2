using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class WeddingHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.Wedding;

    private enum Command : byte {
        Propose = 3,
        ProposalReply = 4,
        CancelProposal = 5,
        DivorceReply = 8,
        ReverseHall = 13,
        ReverseHallReply = 14,
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
        }
    }

    private static void HandlePropose(GameSession session, IByteReader packet) {
        string otherPlayerName = packet.ReadUnicodeString();

        // other partner must be in the same field and a buddy.
        if (!session.Field.TryGetPlayer(otherPlayerName, out FieldPlayer? otherPlayer)) {
            return;
        }

        if (!session.Buddy.IsBuddy(otherPlayer.Value.Character.Id)) {
            return;
        }

        otherPlayer.Session.Send(MarriagePacket.Proposal(session.PlayerName, session.CharacterId));
    }

    private static void HandleProposalReply(GameSession session, IByteReader packet) {
        short replyCode = packet.ReadShort();
        long playerId = packet.ReadLong();

        if (!session.PlayerInfo.GetOrFetch(playerId, out PlayerInfo? playerInfo) ||
            !session.Field.TryGetPlayer(playerInfo.Name, out FieldPlayer? otherPlayer)) {
            return;
        }

        if (!session.Buddy.IsBuddy(otherPlayer.Value.Character.Id)) {
            return;
        }

        if (replyCode != 1) {
            return;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        Marriage? marriage = db.CreateMarriage(session.CharacterId, playerId);
        if (marriage == null) {
            return;
        }

    }
}
