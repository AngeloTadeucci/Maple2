using Maple2.Model.Error;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;

namespace Maple2.Server.Game.Packets;

public static class MyInfoPacket {
    public static ByteWriter UpdateMotto(FieldPlayer fieldPlayer) {
        var pWriter = Packet.Of(SendOp.MyInfo);
        pWriter.WriteInt(fieldPlayer.ObjectId);
        pWriter.WriteUnicodeString(fieldPlayer.Value.Character.Motto);
        pWriter.Write<MyInfoError>(MyInfoError.none);
        pWriter.WriteUnicodeString();

        return pWriter;
    }

    public static ByteWriter BanMotto(string motto) {
        var pWriter = Packet.Of(SendOp.MyInfo);
        pWriter.WriteInt();
        pWriter.WriteUnicodeString(motto);
        pWriter.Write<MyInfoError>(MyInfoError.s_ban_check_err_any_word);
        pWriter.WriteUnicodeString();

        return pWriter;
    }

    public static ByteWriter Error(string message) {
        var pWriter = Packet.Of(SendOp.MyInfo);
        pWriter.WriteInt();
        pWriter.WriteUnicodeString();
        pWriter.Write<MyInfoError>(MyInfoError.custom_message);
        pWriter.WriteUnicodeString(message);

        return pWriter;
    }
}
