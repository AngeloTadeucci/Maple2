using System.Numerics;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
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
    private long spawnTick;

    public FieldMobSpawn(FieldManager field, int objectId, MapMetadataSpawn metadata, WeightedSet<NpcMetadata> npcs, WeightedSet<ItemMetadata> pets) : base(field, objectId, metadata) {
        this.npcs = npcs;
        this.pets = pets;
        spawnedMobs = new List<int>(metadata.Population);
        spawnedPets = new List<int>(metadata.PetPopulation);
        if (Value.Cooldown <= 0) {
            Log.Logger.Information("No respawn for mapId:{MapId} spawnId:{SpawnId}", Field.MapId, Value.Id);
        }
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

    public override void Update(long tickCount) {
        if (tickCount < spawnTick) {
            return;
        }

        spawnTick = long.MaxValue;
        for (int i = spawnedMobs.Count; i < Value.Population; i++) {
            FieldNpc? fieldNpc = Field.SpawnNpc(npcs.Get(), GetRandomSpawn(), Rotation, owner: this);
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

        if (Random.Shared.Next(PET_SPAWN_RATE_TOTAL) < Value.PetSpawnRate) {
            // Any stats are computed after pet is captured since that's when rarity is determined.
            var pet = new Item(pets.Get());
            FieldPet? fieldPet = Field.SpawnPet(pet, GetRandomSpawn(), Rotation, owner: this);
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
