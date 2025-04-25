using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;

namespace Maple2.Server.Core.Packets;

public static class RequestPacket {
    public static ByteWriter Login() {
        return Packet.Of(SendOp.RequestLogin);
    }

    /// <summary>
    /// Creates a packet for requesting a session key from the server.
    /// </summary>
    /// <returns>A <see cref="ByteWriter"/> containing the key request packet.</returns>
    public static ByteWriter Key() {
        return Packet.Of(SendOp.RequestKey);
    }

    /// <summary>
    /// Creates a heartbeat request packet containing the current system tick count.
    /// </summary>
    /// <returns>A ByteWriter representing the heartbeat request packet.</returns>
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
