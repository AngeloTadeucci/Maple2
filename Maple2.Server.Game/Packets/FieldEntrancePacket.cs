using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class FieldEntrancePacket {
    private enum Command : byte {
        Load = 0,
        Error = 2,
    }

    public static ByteWriter Load(IDictionary<int, DungeonEnterLimit> enterLimits) {
        var pWriter = Packet.Of(SendOp.FieldEntrance);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(enterLimits.Count);

        foreach ((int dungeonId, DungeonEnterLimit limit) in enterLimits) {
            pWriter.Write(dungeonId);
            pWriter.Write<DungeonEnterLimit>(limit);
        }

        return pWriter;
    }

    public static ByteWriter Error(int dungeonId, DungeonEnterLimit limit) {
        var pWriter = Packet.Of(SendOp.FieldEntrance);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<DungeonEnterLimit>(limit);
        pWriter.Write(dungeonId);

        return pWriter;
    }
}
