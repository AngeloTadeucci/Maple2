using Maple2.Model.Game.Dungeon;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class DungeonRewardPacket {
    private enum Command : byte {
        Dungeon = 0,
        MiniGame = 1,
        Unknown2 = 2,
        Unknown3 = 3,
    }

    public static ByteWriter Dungeon(DungeonUserRecord userRecord) {
        var pWriter = Packet.Of(SendOp.DungeonReward);
        pWriter.Write<Command>(Command.Dungeon);
        pWriter.WriteClass<DungeonUserRecord>(userRecord);

        return pWriter;
    }

    public static ByteWriter MiniGame(MiniGameUserRecord userRecord) {
        var pWriter = Packet.Of(SendOp.DungeonReward);
        pWriter.Write<Command>(Command.MiniGame);
        pWriter.WriteClass<MiniGameUserRecord>(userRecord);

        return pWriter;
    }
}
