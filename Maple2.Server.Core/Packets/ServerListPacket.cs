using System.Net;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;

namespace Maple2.Server.Core.Packets;

public static class ServerListPacket {
    private enum Command : byte {
        Error = 0,
        Load = 1,
    }

    public static ByteWriter Error() {
        var pWriter = Packet.Of(SendOp.ServerList);
        pWriter.Write<Command>(Command.Error);

        return pWriter;
    }

    public static ByteWriter Load(string serverName, IList<IPEndPoint> serverIps, ICollection<int> channels) {
        var pWriter = Packet.Of(SendOp.ServerList);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(1); // Unknown
        pWriter.WriteUnicodeString(serverName);
        pWriter.WriteByte(4); // IPv4?
        pWriter.Write<ushort>((ushort) serverIps.Count);
        foreach (IPEndPoint endpoint in serverIps) {
            pWriter.WriteUnicodeString(endpoint.Address.ToString());
            pWriter.Write<ushort>((ushort) endpoint.Port);
        }
        pWriter.WriteInt(100); // Const

        // Channels
        pWriter.Write<ushort>((ushort) channels.Count);
        // I think this should be sorted by population
        foreach (int channel in channels) {
            pWriter.WriteShort((short) channel);
        }

        return pWriter;
    }
}
