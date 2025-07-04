using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class BuddyBadgePacket {
    private enum Command : byte {
        Start = 0,
        Stop = 1,
    }

    public static ByteWriter Start(long characterId) {
        var pWriter = Packet.Of(SendOp.BuddyBadge);
        pWriter.Write<Command>(Command.Start);
        pWriter.WriteLong(characterId);

        return pWriter;
    }

    public static ByteWriter Stop(long characterId) {
        var pWriter = Packet.Of(SendOp.BuddyBadge);
        pWriter.Write<Command>(Command.Stop);
        pWriter.WriteLong(characterId);

        return pWriter;
    }
}
