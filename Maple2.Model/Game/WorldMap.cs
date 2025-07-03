using System.Runtime.InteropServices;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 19)]
public struct MapWorldBoss {
    public int NpcId;
    public int MapId;
    public short Channel;
    public long SpawnTime;
    private readonly byte Unknown = 1;

    public MapWorldBoss(int npcId, int mapId, short channel, long spawnTime) {
        NpcId = npcId;
        MapId = mapId;
        Channel = channel;
        SpawnTime = spawnTime;
    }

}

[StructLayout(LayoutKind.Sequential, Pack = 2, Size = 10)]
public struct MapPopulation {
    public int MapId;
    public int Population; // (1=3 icon, 2=2 icon, 3=1 icon)
    public short Channel;
}
