using System.Numerics;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class BuddyEmotePacket {
    private enum Command : byte {
        Invite = 0,
        InviteConfirm = 1,
        Error = 2,
        Accept = 3,
        Decline = 4,
        Start = 5,
        Cancel = 6,
    }

    public static ByteWriter Invite(int emoteId, Player sender) {
        var pWriter = Packet.Of(SendOp.BuddyEmote);
        pWriter.Write<Command>(Command.Invite);
        pWriter.WriteInt(emoteId);
        pWriter.WriteLong(sender.Character.Id);
        pWriter.WriteUnicodeString(sender.Character.Name);

        return pWriter;
    }

    // You sent {0} an invitation to participate in a buddy emote.
    public static ByteWriter InviteConfirm(long receiverId) {
        var pWriter = Packet.Of(SendOp.BuddyEmote);
        pWriter.Write<Command>(Command.InviteConfirm);
        pWriter.WriteLong(receiverId);

        return pWriter;
    }

    public static ByteWriter Error(BuddyEmoteError error) {
        var pWriter = Packet.Of(SendOp.BuddyEmote);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<BuddyEmoteError>(error);

        return pWriter;
    }

    public static ByteWriter Accept(int emoteId, long receiverId) {
        var pWriter = Packet.Of(SendOp.BuddyEmote);
        pWriter.Write<Command>(Command.Accept);
        pWriter.WriteInt(emoteId);
        pWriter.WriteLong(receiverId);

        return pWriter;
    }

    public static ByteWriter Decline(int emoteId, long receiverId) {
        var pWriter = Packet.Of(SendOp.BuddyEmote);
        pWriter.Write<Command>(Command.Decline);
        pWriter.WriteInt(emoteId);
        pWriter.WriteLong(receiverId);

        return pWriter;
    }

    public static ByteWriter Start(int emoteId, long senderId, long receiverId, in Vector3 senderPosition, in Vector3 senderRotation) {
        var pWriter = Packet.Of(SendOp.BuddyEmote);
        pWriter.Write<Command>(Command.Start);
        pWriter.WriteInt(emoteId);
        pWriter.WriteLong(senderId);
        pWriter.WriteLong(receiverId);
        pWriter.Write<Vector3>(senderPosition);
        pWriter.Write<Vector3>(senderRotation);
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter Cancel(int emoteId, long characterId) {
        var pWriter = Packet.Of(SendOp.BuddyEmote);
        pWriter.Write<Command>(Command.Cancel);
        pWriter.WriteInt(emoteId);
        pWriter.WriteLong(characterId);

        return pWriter;
    }
}
