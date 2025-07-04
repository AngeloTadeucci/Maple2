using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class SoundPacket {
    public static ByteWriter Npc(int objectId, string sound) {
        var pWriter = Packet.Of(SendOp.PlayNpcSound);
        pWriter.WriteInt(objectId);
        pWriter.WriteUnicodeString(sound);

        return pWriter;
    }

    public static ByteWriter System(string sound) {
        var pWriter = Packet.Of(SendOp.PlaySystemSound);
        pWriter.WriteUnicodeString(sound);

        return pWriter;
    }
}
