using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Maple2.Model.Game;
using Maple2.Model.Game.Field;
using Maple2.Model.Metadata;
using Maple2.Model.Metadata.FieldEntities;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;
using Maple2.Tools;
using Serilog;

namespace Maple2.Server.Game.Model;

/// <summary>
/// SpawnPoint on field with random NPCs.
/// </summary>
public class FieldMobSpawn : FieldEntity<MapMetadataSpawn> {
    private const int FORCE_SPAWN_MULTIPLIER = 2;
    private const int SPAWN_DISTANCE = 250;
    private const int PET_SPAWN_RATE_TOTAL = 10000;

    private readonly WeightedSet<NpcMetadata> npcs;
    private readonly WeightedSet<ItemMetadata> pets;
    private readonly List<int> spawnedMobs;
    private readonly List<int> spawnedPets;
    private readonly List<Vector3> validSpawns;
    private long spawnTick;
    private int spawnId;

    public FieldMobSpawn(FieldManager field, int objectId, MapMetadataSpawn metadata, WeightedSet<NpcMetadata> npcs, WeightedSet<ItemMetadata> pets) : base(field, objectId, metadata) {
        this.npcs = npcs;
        this.pets = pets;
        spawnedMobs = new List<int>(metadata.Population);
        spawnedPets = new List<int>(metadata.PetPopulation);
        spawnId = metadata.Id;
        if (Value.Cooldown <= 0) {
            Log.Logger.Information("No respawn for mapId:{MapId} spawnId:{SpawnId}", Field.MapId, Value.Id);
        }

        validSpawns = new();
    }

    public void Despawn(int objectId) {
        // Pets do not impact spawn cycle.
        spawnedPets.Remove(objectId);

        if (!spawnedMobs.Remove(objectId)) {
            return;
        }

        // No respawn without a non-zero cooldown.
        if (Value.Cooldown <= 0) {
            return;
        }

#if DEBUG
        // 2x faster respawns.
        const int spawnRate = 500;
#else
        const int spawnRate = 1000;
#endif

        if (spawnedMobs.Count == 0) {
            spawnTick = Math.Min(spawnTick, Environment.TickCount64 + Value.Cooldown * spawnRate);
        } else if (spawnTick == long.MaxValue) {
            spawnTick = Environment.TickCount64 + Value.Cooldown * spawnRate * FORCE_SPAWN_MULTIPLIER;
        }
    }

    private List<Vector3> GetRandomSpawns(int count) {
        List<Vector3> spawnsPicked = new();
        Vector3[] spawnsRemaining = new Vector3[validSpawns.Count];

        validSpawns.CopyTo(spawnsRemaining);

        int selectSpawns = int.Min(count, validSpawns.Count);
        int remainder = count - selectSpawns;

        for (int i = 0; i < selectSpawns; ++i) {
            int picked = Random.Shared.Next(0, spawnsRemaining.Length - i);

            spawnsPicked.Add(spawnsRemaining[picked]);
            spawnsRemaining[picked] = spawnsRemaining[selectSpawns - i - 1]; // remove picked from list by replacing with last in list
        }

        if (remainder > 0) {
            Log.Logger.Error("Ran out of valid spawns to pick for spawn {SpawnId} in map {MapId}; valid spawns: {SpawnCount}; picking: {Picking}", spawnId, Field.MapId, validSpawns.Count, count);
        }

        // ran out of spawns to pick from so we are picking any duplicate now
        for (int i = 0; i < remainder; ++i) {
            int picked = Random.Shared.Next(0, validSpawns.Count);

            spawnsPicked.Add(validSpawns[picked]);
        }

        return spawnsPicked;
    }

    private void InitializeSpawns() {
        if (validSpawns.Count > 0) {
            return;
        }

        if (!Field.MapMetadata.TryGet(Field.MapId, out MapMetadata? map)) {
            Log.Logger.Error("Failed to get map xblock name for map {MapId}", Field.MapId);

            validSpawns.Add(Position);

            return;
        }

        if (!Field.MapData.TryGet(map.XBlock, out FieldAccelerationStructure? mapData)) {
            Log.Logger.Error("Failed to get map xblock name for map {MapId}", Field.MapId);

            validSpawns.Add(Position);

            return;
        }

        foreach (FieldEntity entity in mapData.QuerySpawnsList(Position, SPAWN_DISTANCE)) {
            validSpawns.Add(entity.Position);
        }

        if (validSpawns.Count == 0) {
            Log.Logger.Error("Failed to find spawns for spawn {SpawnId} in map {MapId}", spawnId, Field.MapId);

            validSpawns.Add(Position);
        }
    }

    public override void Update(long tickCount) {
        if (tickCount < spawnTick) {
            return;
        }

        InitializeSpawns();

        spawnTick = long.MaxValue;

        int spawnMobCount = Value.Population - spawnedMobs.Count;
        bool doSpawnPet = Random.Shared.Next(PET_SPAWN_RATE_TOTAL) < Value.PetSpawnRate;

        if (doSpawnPet) {
            ++spawnMobCount;
        }

        List<Vector3> pickedSpawns = GetRandomSpawns(spawnMobCount);
        int spawnIndex = 0;
        for (int i = spawnedMobs.Count; i < Value.Population; i++) {
            FieldNpc? fieldNpc = Field.SpawnNpc(npcs.Get(), pickedSpawns[spawnIndex++], Rotation, owner: this);
            if (fieldNpc == null) {
                continue;
            }

            spawnedMobs.Add(fieldNpc.ObjectId);

            Field.Broadcast(FieldPacket.AddNpc(fieldNpc));
            Field.Broadcast(ProxyObjectPacket.AddNpc(fieldNpc));
        }

        if (Value.PetSpawnRate <= 0 || pets.Count <= 0 || spawnedPets.Count >= Value.PetPopulation) {
            return;
        }

        if (doSpawnPet) {
            // Any stats are computed after pet is captured since that's when rarity is determined.
            var pet = new Item(pets.Get());
            FieldPet? fieldPet = Field.SpawnPet(pet, pickedSpawns.Last(), Rotation, owner: this);
            if (fieldPet == null) {
                return;
            }

            spawnedPets.Add(fieldPet.ObjectId);

            Field.Broadcast(FieldPacket.AddPet(fieldPet));
            Field.Broadcast(ProxyObjectPacket.AddPet(fieldPet));
        }
    }

    private Vector3 GetRandomSpawn() {
        int spawnX = Random.Shared.Next((int) Position.X - SPAWN_DISTANCE, (int) Position.X + SPAWN_DISTANCE);
        int spawnY = Random.Shared.Next((int) Position.Y - SPAWN_DISTANCE, (int) Position.Y + SPAWN_DISTANCE);
        return new Vector3(spawnX, spawnY, Position.Z);
    }
}
