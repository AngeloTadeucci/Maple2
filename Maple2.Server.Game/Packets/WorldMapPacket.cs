using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class WorldMapPacket {
    private enum Command : byte {
        Load = 0,
        Population = 1,
    }

    public static ByteWriter Load(IList<ICollection<MapWorldBoss>> bossGroups, ICollection<MapPopulation> populations,
            IList<ICollection<MapWorldBoss>>? otherBossGroups = null) {
        var pWriter = Packet.Of(SendOp.Worldmap);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteWorldBosses(bossGroups);

        pWriter.WriteBool(otherBossGroups != null);
        if (otherBossGroups != null) {
            pWriter.WriteWorldBosses(otherBossGroups);
        }

        pWriter.WriteByte(3); // Const
        pWriter.WriteInt(populations.Count);
        foreach (MapPopulation population in populations) {
            pWriter.Write<MapPopulation>(population);
        }

        return pWriter;
    }

    public static ByteWriter Population(ICollection<MapPopulation> populations) {
        var pWriter = Packet.Of(SendOp.Worldmap);
        pWriter.Write<Command>(Command.Population);
        pWriter.WriteByte(3); // Const
        pWriter.WriteInt(populations.Count);
        foreach (MapPopulation population in populations) {
            pWriter.Write<MapPopulation>(population);
        }

        return pWriter;
    }

    private static void WriteWorldBosses(this IByteWriter pWriter, IList<ICollection<MapWorldBoss>>? bossGroups) {
        pWriter.WriteBool(bossGroups != null);
        if (bossGroups == null) {
            return;
        }

        pWriter.WriteInt(bossGroups.Count);
        foreach (ICollection<MapWorldBoss> group in bossGroups) {
            pWriter.WriteInt(group.Count);
            foreach (MapWorldBoss boss in group) {
                pWriter.Write<MapWorldBoss>(boss);
            }
        }
    }
}
