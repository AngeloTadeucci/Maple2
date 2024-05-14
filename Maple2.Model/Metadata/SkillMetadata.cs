using System.Collections.Generic;
using System.Numerics;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

// Note: This class only exists to store metadata in db.
public record StoredSkillMetadata(
    int Id,
    string? Name,
    SkillMetadataProperty Property,
    SkillMetadataState State,
    IReadOnlyDictionary<short, SkillMetadataLevel> Levels) : ISearchResult;

public record SkillMetadata(
    int Id,
    short Level,
    string? Name,
    SkillMetadataProperty Property,
    SkillMetadataState State,
    SkillMetadataLevel Data);

public record SkillMetadataProperty(
    SkillType Type,
    SkillSubType SubType,
    RangeType RangeType,
    AttackType AttackType,
    Element Element,
    bool ContinueSkill,
    bool SpRecoverySkill,
    bool ImmediateActive,
    //bool WeaponDependency, // 1,10300179,10300184
    bool UnrideOnHit,
    bool UnrideOnUse,
    bool ReleaseObjectWeapon,
    //bool DisableWater, // 10500061,10500065,10500190 (Sharp Eyes)
    //bool HoldAttack, // 10700171,10700252
    int SkillGroup,
    short MaxLevel);

public record SkillMetadataState;

public record SkillMetadataLevel(
    BeginCondition Condition,
    SkillMetadataChange? Change,
    SkillMetadataConsume Consume,
    SkillMetadataRecovery Recovery,
    SkillEffectMetadata[] Skills,
    SkillMetadataMotion[] Motions);

public record SkillMetadataChange(
    SkillMetadataChange.Skill Origin,
    SkillMetadataChange.Effect[] Effects,
    SkillMetadataChange.Skill[] Skills) {

    public record Skill(int Id, int Level);

    public record Effect(int Id, int Level, int OverlapCount);
}

public record SkillMetadataConsume(
    long Meso,
    bool UseItem,
    float HpRate,
    Dictionary<BasicAttribute, long> Stat);

public record SkillMetadataRecovery(
    long SpValue,
    float SpRate);

public record SkillMetadataMotion(
    SkillMetadataMotionProperty MotionProperty,
    SkillMetadataAttack[] Attacks);

public record SkillMetadataMotionProperty(
    string SequenceName,
    float SequenceSpeed);

public record SkillMetadataAttack(
    string Point,
    int PointGroup,
    int TargetCount,
    long MagicPathId,
    long CubeMagicPathId,
    CompulsionType[] CompulsionTypes,
    SkillMetadataPet? Pet,
    SkillMetadataRange Range,
    SkillMetadataArrow Arrow,
    SkillMetadataDamage Damage,
    SkillEffectMetadata[] Skills);

public record SkillMetadataPet(
    int TamingGroup,
    int TrapLevel,
    int TamingPoint,
    bool ForcedTaming);

public record SkillMetadataRange(
    SkillRegion Type,
    float Distance,
    float Height,
    float Width,
    float EndWidth,
    float RotateZDegree,
    Vector3 RangeAdd,
    Vector3 RangeOffset,
    SkillEntity IncludeCaster, // 0,1,2
    SkillEntity ApplyTarget, // 0,1,2,3,5,6,7,8
    SkillEntity CastTarget); // 0,1,2,3,4,5,7

public record SkillMetadataArrow(
    bool Overlap,
    bool Explosion,
    SkillEntity NonTarget,
    int BounceType,
    int BounceCount,
    float BounceRadius,
    bool BounceOverlap,
    Vector3 Collision,
    Vector3 CollisionAdd);

public record SkillMetadataDamage(
    int Count,
    float Rate,
    float HitSpeed,
    float HitDelay,
    bool IsConstDamage, // Use 'Value' to determine damage
    long Value,
    float DamageByTargetMaxHp,
    SkillMetadataPush? Push);

public record SkillMetadataPush(
    int Type,
    float Distance,
    float Duration,
    float Probability);
