using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Tools;
using Serilog;

namespace Maple2.Server.Game.Model;

public class FieldSpawnGroup : FieldEntity<SpawnGroupMetadata> {

    private readonly WeightedSet<SpawnNpcMetadata> npcs;
    private readonly WeightedSet<SpawnInteractObjectMetadata> interactObjects;
    private readonly List<int> spawnIds = [];
    private long resetTick;

    private bool active;

    public FieldSpawnGroup(FieldManager field, int objectId, SpawnGroupMetadata metadata) : base(field, objectId, metadata) {
        resetTick = metadata.ResetTick + field.FieldTick;
        npcs = new WeightedSet<SpawnNpcMetadata>();
        interactObjects = new WeightedSet<SpawnInteractObjectMetadata>();
        Init();
    }

    private void Init() {
        switch (Value.Type) {
            case CombineSpawnGroupType.npc:
                if (!Field.ServerTableMetadata.CombineSpawnTable.Npcs.TryGetValue(Value.GroupId, out Dictionary<int, SpawnNpcMetadata>? npcDict)) {
                    return;
                }

                foreach (SpawnNpcMetadata npc in npcDict.Values) {
                    npcs.Add(npc, npc.Weight);
                }
                break;
            case CombineSpawnGroupType.interactObject:
                if (!Field.ServerTableMetadata.CombineSpawnTable.InteractObjects.TryGetValue(Value.GroupId, out Dictionary<int, SpawnInteractObjectMetadata>? interactDict)) {
                    return;
                }

                foreach (SpawnInteractObjectMetadata interact in interactDict.Values) {
                    interactObjects.Add(interact, interact.Weight);
                }
                break;
            default:
                Log.Logger.Error("Invalid spawn group type {Type} for spawn group {GroupId}", Value.Type, Value.GroupId);
                break;
        }
    }

    public void ToggleActive(bool active) {
        this.active = active;

        if (!active) {
            spawnIds.Clear();
        } else {
            InitializeSpawns();
        }
    }

    private void InitializeSpawns() {
        if (Value.TotalCount <= 0) {
            return;
        }

        switch (Value.Type) {
            case CombineSpawnGroupType.npc:
                SpawnNpcs();
                return;
            case CombineSpawnGroupType.interactObject:
                SpawnInteractObjects();
                return;
        }
    }

    private void SpawnNpcs() {
        var removedSpawnIds = new List<int>();
        foreach (int spawnId in spawnIds) {
            if (!Field.GetActorsBySpawnId(spawnId).Any()) {
                removedSpawnIds.Add(spawnId);
            }
        }

        foreach (int spawnId in removedSpawnIds) {
            spawnIds.Remove(spawnId);
        }

        do {
            SpawnNpcMetadata npc = npcs.Get();
            if (spawnIds.Contains(npc.SpawnId)) {
                continue;
            }
            Field.ToggleNpcSpawnPoint(npc.SpawnId);
            spawnIds.Add(npc.SpawnId);
        } while (Value.TotalCount > spawnIds.Count);
    }

    private void SpawnInteractObjects() {
        var removedSpawnIds = new List<int>();
        foreach (int spawnId in spawnIds) {
            if (!Field.GetInteractObjectsBySpawnId(spawnId).Any()) {
                removedSpawnIds.Add(spawnId);
            }
        }

        foreach (int spawnId in removedSpawnIds) {
            spawnIds.Remove(spawnId);
        }

        do {
            SpawnInteractObjectMetadata interactObject = interactObjects.Get();
            if (spawnIds.Contains(interactObject.RegionSpawnId)) {
                continue;
            }
            Field.SpawnInteractObject(interactObject);
            spawnIds.Add(interactObject.RegionSpawnId);
        } while (Value.TotalCount > spawnIds.Count);
    }

    public override void Update(long tickCount) {
        if (!active) {
            return;
        }

        if (tickCount < resetTick) {
            return;
        }

        InitializeSpawns();
        resetTick = Field.FieldTick + Value.ResetTick;
    }
}
