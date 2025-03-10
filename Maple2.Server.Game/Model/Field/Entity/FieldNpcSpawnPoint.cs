using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;
using Serilog;

namespace Maple2.Server.Game.Model;

public class FieldSpawnPointNpc : FieldEntity<SpawnPointNPC> {
    private readonly List<int> spawnedMobs;
    private int SpawnId => Value.Id;

    private bool active;

    public FieldSpawnPointNpc(FieldManager field, int objectId, SpawnPointNPC metadata) : base(field, objectId, metadata) {
        spawnedMobs = [];
        if (metadata.RegenCheckTime > 0) {
            active = true;
        }
    }

    public void SpawnOnCreate() {
        if (Value.SpawnOnFieldCreate) {
            TriggerSpawn();
        }
    }

    public void TriggerSpawn() {
        foreach (SpawnPointNPCListEntry spawn in Value.NpcList) {
            if (!Field.NpcMetadata.TryGet(spawn.NpcId, out NpcMetadata? npcMetadata)) {
                Log.Logger.Warning("Npc {NpcId} failed to load for map {MapId}", spawn.NpcId, Field.MapId);
                continue;
            }

            for (int i = 0; i < spawn.Count; i++) {
                FieldNpc? npc = Field.SpawnNpc(npcMetadata, Value);
                if (npc == null) {
                    continue;
                }
                npc.SpawnPointId = 0;

                Field.Broadcast(FieldPacket.AddNpc(npc));
                Field.Broadcast(ProxyObjectPacket.AddNpc(npc));
                spawnedMobs.Add(npc.ObjectId);
            }
        }
    }
}
