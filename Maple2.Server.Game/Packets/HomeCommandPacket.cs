using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class HomeCommandPacket {
    private enum Command : byte {
        Load = 0,
        UpdateArchitectScore = 1,
    }

    public static ByteWriter LoadHome(long accountId) {
        var pWriter = Packet.Of(SendOp.HomeCommand);
        pWriter.Write(Command.Load);
        pWriter.WriteLong(accountId);
        pWriter.WriteLong(); // last time player nominated home

        return pWriter;
    }

    public static ByteWriter UpdateArchitectScore(int ownerObjectId, int architectScoreCurrent, int architectScoreTotal) {
        var pWriter = Packet.Of(SendOp.HomeCommand);
        pWriter.Write(Command.UpdateArchitectScore);
        pWriter.WriteInt(ownerObjectId);
        pWriter.WriteLong(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        pWriter.WriteInt(architectScoreCurrent);
        pWriter.WriteInt(architectScoreTotal);

        return pWriter;
    }
}
