using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class RevivalPacket {
    public static ByteWriter UpdatePenalty(int objectId, int endTick, int count) {
        var pWriter = Packet.Of(SendOp.RevivalConfirm);
        pWriter.WriteInt(objectId);
        pWriter.WriteInt(endTick);
        pWriter.WriteInt(count);

        return pWriter;
    }

    public static ByteWriter RevivalCount(int count) {
        var pWriter = Packet.Of(SendOp.RevivalCount);
        pWriter.WriteInt(count);

        return pWriter;
    }

    public static ByteWriter Revive(int objectId) {
        var pWriter = Packet.Of(SendOp.Revival);
        pWriter.WriteInt(objectId);
        pWriter.WriteByte();

        return pWriter;
    }

    public static ByteWriter Tombstone(Tombstone tombstone) {
        var pWriter = Packet.Of(SendOp.Tombstone);
        pWriter.WriteClass<Tombstone>(tombstone);

        return pWriter;
    }
}
