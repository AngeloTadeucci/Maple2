using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class PlayerKillNoticePacket {
    private enum Command : byte {
        Notice = 0,
        Unknown = 1, // Error?
    }

    public static ByteWriter Notice(string attacker, string victim) {
        var pWriter = Packet.Of(SendOp.ItemDropNotice);
        pWriter.Write<Command>(Command.Notice);
        pWriter.WriteUnicodeString(attacker);
        pWriter.WriteUnicodeString(victim);

        return pWriter;
    }
}
