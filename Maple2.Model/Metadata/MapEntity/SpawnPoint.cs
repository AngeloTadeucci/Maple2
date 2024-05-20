using System;
using System.Collections.Generic;
using System.Numerics;

namespace Maple2.Model.Metadata;

public abstract record SpawnPoint(int Id, Vector3 Position, Vector3 Rotation, bool Visible) : MapBlock;

public record SpawnPointPC(
    int Id,
    Vector3 Position,
    Vector3 Rotation,
    bool Visible,
    bool Enable
) : SpawnPoint(Id, Position, Rotation, Visible);

public record SpawnPointNPCListEntry(int NpcId, int Count);

public record SpawnPointNPC(
    int Id,
    Vector3 Position,
    Vector3 Rotation,
    bool Visible,
    bool SpawnOnFieldCreate,
    float SpawnRadius,
    IList<SpawnPointNPCListEntry> NpcList,
    int RegenCheckTime,
    string? PatrolData
) : SpawnPoint(Id, Position, Rotation, Visible);

public record EventSpawnPointNPC(
    int Id,
    Vector3 Position,
    Vector3 Rotation,
    bool Visible,
    bool SpawnOnFieldCreate,
    float SpawnRadius,
    IList<SpawnPointNPCListEntry> NpcList,
    int RegenCheckTime,
    int LifeTime,
    string SpawnAnimation
) : SpawnPointNPC(Id, Position, Rotation, Visible, SpawnOnFieldCreate, SpawnRadius, NpcList, RegenCheckTime, String.Empty);

public record EventSpawnPointItem(
    int Id,
    Vector3 Position,
    Vector3 Rotation,
    float Lifetime,
    int IndividualDropBoxId,
    int GlobalDropBoxId,
    int GlobalDropLevel,
    bool Visible
) : SpawnPoint(Id, Position, Rotation, Visible);
