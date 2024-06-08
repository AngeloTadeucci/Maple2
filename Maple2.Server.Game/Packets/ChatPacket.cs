using System.Web;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class ChatPacket {
    public static ByteWriter Message(Player player, ChatType type, string message) {
        var pWriter = Packet.Of(SendOp.UserChat);
        pWriter.WriteLong(player.Account.Id);
        pWriter.WriteLong(player.Character.Id);
        pWriter.WriteUnicodeString(player.Character.Name);
        pWriter.WriteBool(false);
        pWriter.WriteUnicodeString(message);
        pWriter.Write<ChatType>(type);
        pWriter.WriteBool(false);
        pWriter.WriteInt();

        switch (type) {
            case ChatType.WhisperFrom:
                pWriter.WriteUnicodeString();
                break;
            case ChatType.Super:
                pWriter.WriteInt();
                break;
            case ChatType.Club:
                pWriter.WriteLong();
                break;
        }

        pWriter.WriteBool(false);

        return pWriter;
    }

    public static ByteWriter Message(long accountId, long characterId, string characterName, ChatType type, string message, int superChatId = 0, long clubId = 0) {
        var pWriter = Packet.Of(SendOp.UserChat);
        pWriter.WriteLong(accountId);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(characterName);
        pWriter.WriteBool(false);
        pWriter.WriteUnicodeString(message);
        pWriter.Write<ChatType>(type);
        pWriter.WriteBool(false);
        pWriter.WriteInt();

        switch (type) {
            case ChatType.WhisperFrom:
                pWriter.WriteUnicodeString();
                break;
            case ChatType.Super:
                pWriter.WriteInt(superChatId);
                break;
            case ChatType.Club:
                pWriter.WriteLong(clubId);
                break;
        }

        pWriter.WriteBool(false);

        return pWriter;
    }

    public static ByteWriter Whisper(long accountId, long characterId, string name, string message, string? unknown = null) {
        ChatType type = unknown == null ? ChatType.WhisperTo : ChatType.WhisperFrom;

        var pWriter = Packet.Of(SendOp.UserChat);
        pWriter.WriteLong(accountId);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(name);
        pWriter.WriteBool(false);
        pWriter.WriteUnicodeString(message);
        pWriter.Write<ChatType>(type);
        pWriter.WriteBool(false);
        pWriter.WriteInt();

        if (type == ChatType.WhisperFrom) {
            pWriter.WriteUnicodeString(unknown ?? string.Empty);
        }

        pWriter.WriteBool(false);

        return pWriter;
    }

    public static ByteWriter WhisperReject(string name) {
        var pWriter = Packet.Of(SendOp.UserChat);
        pWriter.WriteLong();
        pWriter.WriteLong();
        pWriter.WriteUnicodeString(name);
        pWriter.WriteBool(false);
        pWriter.WriteUnicodeString(".");
        pWriter.Write<ChatType>(ChatType.WhisperReject);
        pWriter.WriteBool(false);
        pWriter.WriteInt();
        pWriter.WriteBool(false);

        return pWriter;
    }

    public static ByteWriter System(string type, string message) {
        var pWriter = Packet.Of(SendOp.UserChat);
        pWriter.WriteLong();
        pWriter.WriteLong();
        pWriter.WriteUnicodeString(type);
        pWriter.WriteBool(false);
        pWriter.WriteUnicodeString(message);
        pWriter.Write<ChatType>(ChatType.System);
        pWriter.WriteBool(false);
        pWriter.WriteInt();
        pWriter.WriteBool(false);

        return pWriter;
    }

    public static ByteWriter Alert(StringCode code) {
        var pWriter = Packet.Of(SendOp.UserChat);
        pWriter.WriteLong();
        pWriter.WriteLong();
        pWriter.WriteUnicodeString();
        pWriter.WriteBool(true);
        pWriter.Write<StringCode>(code);
        pWriter.Write<ChatType>(ChatType.NoticeAlert);
        pWriter.WriteBool(false);
        pWriter.WriteInt();
        pWriter.WriteBool(false);

        return pWriter;
    }

    public static ByteWriter Alert(string message, bool htmlEncoded = false) {
        var pWriter = Packet.Of(SendOp.UserChat);
        pWriter.WriteLong();
        pWriter.WriteLong();
        pWriter.WriteUnicodeString();
        pWriter.WriteBool(false);
        pWriter.WriteUnicodeString(htmlEncoded ? message : HttpUtility.HtmlEncode(message));
        pWriter.Write<ChatType>(ChatType.NoticeAlert);
        pWriter.WriteBool(false);
        pWriter.WriteInt();
        pWriter.WriteBool(false);

        return pWriter;
    }
}
