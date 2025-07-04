using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;

namespace Maple2.Server.Core.Packets;

public static class RequestPacket {
    public static ByteWriter Login() {
        return Packet.Of(SendOp.RequestLogin);
    }

    public static ByteWriter Key() {
        return Packet.Of(SendOp.RequestKey);
    }

    public static ByteWriter Heartbeat() {
        var pWriter = Packet.Of(SendOp.RequestHeartbeat);
        pWriter.WriteInt(Environment.TickCount);

        return pWriter;
    }

    public static ByteWriter TickSync(int serverTick) {
        var pWriter = Packet.Of(SendOp.RequestClientTickSync);
        pWriter.WriteInt(serverTick);

        return pWriter;
    }
}
