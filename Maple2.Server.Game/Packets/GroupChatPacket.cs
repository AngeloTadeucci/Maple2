using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.GroupChat;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class GroupChatPacket {
    private enum Command : byte {
        Load = 0,
        Create = 1,
        Invite = 2,
        Join = 3,
        Leave = 4,
        AddMember = 6,
        RemoveMember = 7,
        LoginNotice = 8,
        LogoutNotice = 9,
        Chat = 10,
        Error = 13,
    }

    public static ByteWriter Load(GroupChat groupChat) {
        var pWriter = Packet.Of(SendOp.GroupChat);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(groupChat.Id);
        pWriter.WriteByte((byte) groupChat.Members.Count);
        foreach ((long characterId, GroupChatMember member) in groupChat.Members) {
            pWriter.WriteBool(true); // is member?
            pWriter.WriteClass<PlayerInfo>(member.Info);
        }

        return pWriter;
    }

    public static ByteWriter Create(int groupChatId) {
        var pWriter = Packet.Of(SendOp.GroupChat);
        pWriter.Write<Command>(Command.Create);
        pWriter.WriteInt(groupChatId);

        return pWriter;
    }

    public static ByteWriter Invite(string memberName, string targetName, int groupChatId) {
        var pWriter = Packet.Of(SendOp.GroupChat);
        pWriter.Write<Command>(Command.Invite);
        pWriter.WriteUnicodeString(memberName);
        pWriter.WriteUnicodeString(targetName);
        pWriter.WriteInt(groupChatId);

        return pWriter;
    }

    public static ByteWriter Join(string senderMemberName, string receiverTargetName, int groupChatId) {
        var pWriter = Packet.Of(SendOp.GroupChat);
        pWriter.Write<Command>(Command.Join);
        pWriter.WriteUnicodeString(senderMemberName);
        pWriter.WriteUnicodeString(receiverTargetName);
        pWriter.WriteInt(groupChatId);

        return pWriter;
    }

    public static ByteWriter Leave(int groupChatId) {
        var pWriter = Packet.Of(SendOp.GroupChat);
        pWriter.Write<Command>(Command.Leave);
        pWriter.WriteInt(groupChatId);

        return pWriter;
    }

    public static ByteWriter AddMember(PlayerInfo targetCharacter, string inviterName, int groupChatId) {
        var pWriter = Packet.Of(SendOp.GroupChat);
        pWriter.Write<Command>(Command.AddMember);
        pWriter.WriteInt(groupChatId);
        pWriter.WriteUnicodeString(inviterName);
        pWriter.WriteBool(true); // is member?
        pWriter.WriteClass<PlayerInfo>(targetCharacter);

        return pWriter;
    }

    public static ByteWriter RemoveMember(string memberName, int groupChatId) {
        var pWriter = Packet.Of(SendOp.GroupChat);
        pWriter.Write<Command>(Command.RemoveMember);
        pWriter.WriteInt(groupChatId);
        pWriter.WriteBool(false); // is not member?
        pWriter.WriteUnicodeString(memberName);

        return pWriter;
    }

    public static ByteWriter LoginNotice(string memberName, int groupChatId) {
        var pWriter = Packet.Of(SendOp.GroupChat);
        pWriter.Write<Command>(Command.LoginNotice);
        pWriter.WriteInt(groupChatId);
        pWriter.WriteUnicodeString(memberName);

        return pWriter;
    }

    public static ByteWriter LogoutNotice(string memberName, int groupChatId) {
        var pWriter = Packet.Of(SendOp.GroupChat);
        pWriter.Write<Command>(Command.LogoutNotice);
        pWriter.WriteInt(groupChatId);
        pWriter.WriteUnicodeString(memberName);

        return pWriter;
    }

    public static ByteWriter Chat(string memberName, int groupChatId, string message) {
        var pWriter = Packet.Of(SendOp.GroupChat);
        pWriter.Write<Command>(Command.Chat);
        pWriter.WriteInt(groupChatId);
        pWriter.WriteUnicodeString(memberName);
        pWriter.WriteUnicodeString(message);

        return pWriter;
    }

    public static ByteWriter Error(GroupChatError error, string memberName, string targetName) {
        var pWriter = Packet.Of(SendOp.GroupChat);
        pWriter.Write<Command>(Command.Error);
        pWriter.WriteByte(2); // Unknown
        pWriter.Write<GroupChatError>(error);
        pWriter.WriteUnicodeString(memberName);
        pWriter.WriteUnicodeString(targetName);

        return pWriter;
    }
}
