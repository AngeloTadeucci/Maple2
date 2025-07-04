using Maple2.Model.Game.Dungeon;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class DungeonMissionPacket {
    private enum Command : byte {
        Load = 0,
        Update = 1,
        SetAbandon = 4,
    }

    public static ByteWriter Load(IDictionary<int, DungeonMission> missions) {
        var pWriter = Packet.Of(SendOp.DungeonMission);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(missions.Count);
        foreach ((int id, DungeonMission mission) in missions) {
            pWriter.WriteClass<DungeonMission>(mission);
        }

        return pWriter;
    }

    public static ByteWriter Update(params DungeonMission[] missions) {
        var pWriter = Packet.Of(SendOp.DungeonMission);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteInt(missions.Length);
        foreach (DungeonMission mission in missions) {
            pWriter.WriteClass<DungeonMission>(mission);
        }

        return pWriter;
    }

    public static ByteWriter SetAbandon(bool enable) {
        var pWriter = Packet.Of(SendOp.DungeonMission);
        pWriter.Write<Command>(Command.SetAbandon);
        pWriter.WriteBool(enable);

        return pWriter;
    }
}
