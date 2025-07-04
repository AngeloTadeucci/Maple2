using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class CheckCharacterNamePacket {
    public static ByteWriter Result(bool nameIsUsed, string characterName, long itemUid) {
        var pWriter = Packet.Of(SendOp.CheckCharNameResult);
        pWriter.WriteBool(nameIsUsed);
        pWriter.WriteLong(itemUid);
        pWriter.WriteUnicodeString(characterName);

        return pWriter;
    }
}
