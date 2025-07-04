using System.Numerics;

namespace Maple2.Model.Metadata;

public record MagicPathTable(IReadOnlyDictionary<long, IReadOnlyList<MagicPath>> Entries) : Table;

public record MagicPath(
    bool Align,
    int AlignHeight,
    bool Rotate,
    bool IgnoreAdjust,
    bool ExplosionByDestroy,
    bool CatmullRom,
    bool IgnorePhysXTestInitPosition,
    bool IgnoreCancelAtSpawnTime,
    Vector3 Direction,
    Vector3 FireOffset,
    Vector3 FireFixed,
    Vector3 ControlValue0,
    Vector3 ControlValue1,
    Vector3 ControlEndOffsetValue,
    bool TraceTargetOffset,
    float TraceTargetDuration,
    float Velocity,
    float Distance,
    float RotateZDegree,
    float LifeTime,
    float DelayTime,
    float SpawnTime,
    float DestroyTime,
    float ControlRate,
    int LookAtType,
    int PiercingAttackInterval,
    int PiercingAttackMaxTargetCount,
    float NonTargetMoveDistance,
    int MoveEndHoldDuration);
