using System.Collections.Generic;
using System.Numerics;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record NpcMetadata(
    int Id,
    string? Name,
    string Model,
    NpcMetadataStat Stat,
    NpcMetadataBasic Basic,
    NpcMetadataProperty Property,
    NpcMetadataDropInfo DropInfo,
    NpcMetadataAction Action,
    NpcMetadataDead Dead) : ISearchResult;

public record NpcMetadataStat(
    IReadOnlyDictionary<BasicAttribute, long> Stats,
    float[] ScaleStatRate,
    long[] ScaleBaseTap,
    long[] ScaleBaseDef,
    float[] ScaleBaseSpaRate);

public record NpcMetadataBasic(
    int Friendly,
    int AttackGroup,
    int DefenseGroup,
    int Kind,
    int ShopId,
    int HitImmune,
    int AbnormalImmune,
    short Level,
    int Class,
    bool RotationDisabled,
    int MaxSpawnCount,
    int GroupSpawnCount,
    int RareDegree,
    int Difficulty,
    string[] MainTags,
    string[] SubTags,
    long CustomExp);

public record NpcMetadataProperty(
    NpcMetadataSkill[] Skills,
    NpcMetadataBuff[] Buffs,
    NpcMetadataCapsule Capsule,
    NpcMetadataCollision? Collision);

public record NpcMetadataSkill(int Id, int Level, int Priority, int Probability);

public record NpcMetadataBuff(int Id, int Level);

public record NpcMetadataCapsule(float Radius, float Height);

public record NpcMetadataCollision(Vector3 Dimensions, Vector3 Offset);

public record NpcMetadataAction(
    float RotateSpeed,
    float WalkSpeed,
    float RunSpeed,
    NpcAction[] Actions,
    int MoveArea,
    string MaidExpired);

public record NpcAction(string Name, int Probability);

public record NpcMetadataDropInfo(
    float DropDistanceBase,
    int DropDistanceRandom,
    int[] GlobalDropBoxIds,
    int[] DeadGlobalDropBoxIds,
    int[] IndividualDropBoxIds,
    int[] GlobalHitDropBoxIds,
    int[] IndividualHitDropBoxIds);

public record NpcMetadataDead(
    float Time,
    int Revival,
    int Count,
    float LifeTime,
    int ExtendRoomTime);
