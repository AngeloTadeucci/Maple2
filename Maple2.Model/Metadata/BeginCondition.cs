using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record BeginCondition(
    short Level,
    long Mesos,
    Gender Gender,
    JobCode[] JobCode,
    float Probability,
    float CooldownTime,
    int DurationWithoutDamage,
    int DurationWithoutMoving,
    bool OnlyShadowWorld,
    bool OnlyFlyableMap,
    bool AllowDead,
    bool AllowOnBattleMount,
    bool OnlyOnBattleMount,
    bool AllowOnSurvival,
    DungeonGroupType[] DungeonGroupType,
    IReadOnlyDictionary<BasicAttribute, long> Stat,
    int[] Maps,
    MapType[] MapTypes,
    Continent[] Continents,
    int[] ActiveSkill,
    BeginConditionWeapon[]? Weapon,
    BeginConditionTarget? Target,
    BeginConditionTarget? Owner,
    BeginConditionTarget? Caster);

public record BeginConditionWeapon(
    ItemType LeftHand,
    ItemType RightHand);

public record BeginConditionTarget(
    BeginConditionTarget.HasBuff[] Buff,
    BeginConditionTarget.EventCondition Event,
    BeginConditionTarget.BeginConditionStat[] Stat,
    ActorState[] States,
    ActorSubState[] SubStates,
    IReadOnlyDictionary<MasteryType, int> Masteries,
    //string[] NpcTags // not used?
    int[] NpcIds,
    int[] HasNotBuffIds
) {
    public record HasBuff(int Id, short Level, bool Owned, int Count, CompareType Compare);

    public record EventCondition(EventConditionType Type, bool IgnoreOwner, int[] SkillIds, int[] BuffIds);
    public record BeginConditionStat(BasicAttribute Attribute, float Value, CompareType Compare, CompareStatValueType ValueType);

    public record TargetCheck(int RangeDistance, int RangeCount, int Friendly, CompareType Compare);
}


