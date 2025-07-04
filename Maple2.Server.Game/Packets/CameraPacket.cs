using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class CameraPacket {
    public static ByteWriter Local(int triggerId, bool enable, int objectId = 0) {
        var pWriter = Packet.Of(SendOp.LocalCamera);
        pWriter.WriteInt(triggerId);
        pWriter.WriteBool(enable);
        pWriter.WriteInt(objectId);

        return pWriter;
    }

    public static ByteWriter Interpolate(float interpolationTime) {
        var pWriter = Packet.Of(SendOp.CameraInterpolation);
        pWriter.WriteFloat(interpolationTime);

        return pWriter;
    }
}
