using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class InsigniaPacket {

    public static ByteWriter Update(int objectId, short insigniaId, bool display) {
        var pWriter = Packet.Of(SendOp.Insignia);
        pWriter.WriteInt(objectId);
        pWriter.WriteShort(insigniaId);
        pWriter.WriteBool(display);

        return pWriter;
    }
}
