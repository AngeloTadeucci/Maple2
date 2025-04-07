using System.Collections.Generic;
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
    bool OnlyShadowWorld,
    bool OnlyFlyableMap,
    bool AllowDead,
    IReadOnlyDictionary<BasicAttribute, long> Stat,
    BeginConditionWeapon[]? Weapon,
    BeginConditionTarget? Target,
    BeginConditionTarget? Owner,
    BeginConditionTarget? Caster);

public record BeginConditionWeapon(
    ItemType LeftHand,
    ItemType RightHand);

public record BeginConditionTarget(
    BeginConditionTarget.HasBuff[] Buff,
    BeginConditionTarget.EventCondition? Event
) {
    public record HasBuff(int Id, short Level, bool Owned, int Count, CompareType Compare);

    // All: 1,2,4,5,6,7,10,11,13,14,16,17,18,19,20,102,103
    // IgnoreOwner => 1,2,4,5,6,7,10,16,17,18
    // SkillIds => 4,6,7,14,20
    // BuffIds => 16,17,102
    public record EventCondition(EventConditionType Type, bool IgnoreOwner, int[] SkillIds, int[] BuffIds);
}
