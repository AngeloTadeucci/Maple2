using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class FallDamagePacket {
    public static ByteWriter FallDamage(int objectId, int damage) {
        var pWriter = Packet.Of(SendOp.StateFallDamage);
        pWriter.WriteInt(objectId);
        pWriter.WriteInt(damage);

        return pWriter;
    }
}
