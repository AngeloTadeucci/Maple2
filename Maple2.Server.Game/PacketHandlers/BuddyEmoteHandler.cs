using System.Numerics;
using Maple2.Model.Error;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using static Maple2.Model.Error.BuddyEmoteError;

namespace Maple2.Server.Game.PacketHandlers;

public class BuddyEmoteHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.BuddyEmote;

    private enum Command : byte {
        Invite = 0,
        InviteConfirm = 1,
        Error = 2,
        Accept = 3,
        Decline = 4,
        Cancel = 6,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Invite:
                HandleInvite(session, packet);
                return;
            case Command.InviteConfirm:
                HandleInviteConfirm(session, packet);
                return;
            case Command.Error:
                HandleError(session, packet);
                return;
            case Command.Accept:
                HandleAccept(session, packet);
                return;
            case Command.Decline:
                HandleDecline(session, packet);
                return;
            case Command.Cancel:
                HandleCancel(session, packet);
                return;
        }
    }

    private void HandleInvite(GameSession session, IByteReader packet) {
        int emoteId = packet.ReadInt();
        long receiverId = packet.ReadLong();

        if (!session.Player.Value.Unlock.Emotes.Contains(emoteId)) {
            session.Send(BuddyEmotePacket.Error(s_couple_emotion_failed_request_not_exist_skill));
            return;
        }
        if (session.Field?.TryGetPlayerById(receiverId, out FieldPlayer? receiver) is null or false) {
            session.Send(BuddyEmotePacket.Error(s_couple_emotion_target_user_wrong_position));
            return;
        }

        receiver.Session.Send(BuddyEmotePacket.Invite(emoteId, session.Player));
    }

    private void HandleInviteConfirm(GameSession session, IByteReader packet) {
        long senderId = packet.ReadLong();

        if (session.Field?.TryGetPlayerById(senderId, out FieldPlayer? sender) is null or false) {
            session.Send(BuddyEmotePacket.Error(s_couple_emotion_failed_accept_cannot_find_request_user));
            return;
        }

        sender.Session.Send(BuddyEmotePacket.InviteConfirm(session.CharacterId));
    }

    private void HandleError(GameSession session, IByteReader packet) {
        long characterId = packet.ReadLong();
        var error = packet.Read<BuddyEmoteError>();
        if (session.Field?.TryGetPlayerById(characterId, out FieldPlayer? player) is null or false) {
            return;
        }

        player.Session.Send(BuddyEmotePacket.Error(error));
    }

    private void HandleAccept(GameSession session, IByteReader packet) {
        int emoteId = packet.ReadInt();
        long senderId = packet.ReadLong();
        var senderPosition = packet.Read<Vector3>();
        var receiverPosition = packet.Read<Vector3>();
        float rotationZ = packet.ReadFloat();

        if (session.Field?.TryGetPlayerById(senderId, out FieldPlayer? sender) is null or false) {
            session.Send(BuddyEmotePacket.Error(s_couple_emotion_failed_accept_cannot_find_request_user));
            return;
        }

        sender.Session.Send(BuddyEmotePacket.Accept(emoteId, session.CharacterId));
        ByteWriter start = BuddyEmotePacket.Start(emoteId, senderId, session.CharacterId, senderPosition, new Vector3(0, 0, rotationZ));
        session.Send(start);
        sender.Session.Send(start);
    }

    private void HandleDecline(GameSession session, IByteReader packet) {
        int emoteId = packet.ReadInt();
        long senderId = packet.ReadLong();

        if (session.Field?.TryGetPlayerById(senderId, out FieldPlayer? sender) is null or false) {
            return;
        }

        sender.Session.Send(BuddyEmotePacket.Decline(emoteId, session.CharacterId));
    }

    private void HandleCancel(GameSession session, IByteReader packet) {
        int emoteId = packet.ReadInt();
        long characterId = packet.ReadLong();

        if (session.Field?.TryGetPlayerById(characterId, out FieldPlayer? player) is null or false) {
            return;
        }

        player.Session.Send(BuddyEmotePacket.Cancel(emoteId, characterId));
    }
}
