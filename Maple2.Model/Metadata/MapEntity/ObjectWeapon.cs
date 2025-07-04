using System.Numerics;

namespace Maple2.Model.Metadata;

public record ObjectWeapon(
    int[] ItemIds,
    int RespawnTick,
    float ActiveDistance,
    Vector3 Position,
    Vector3 Rotation,
    int SpawnNpcId = 0,
    int SpawnNpcCount = 0,
    float SpawnNpcRate = 0,
    int SpawnNpcLifeTick = 0)
: MapBlock;
