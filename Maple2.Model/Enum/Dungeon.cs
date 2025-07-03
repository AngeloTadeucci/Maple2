using System.ComponentModel;

namespace Maple2.Model.Enum;

public enum DungeonPlayType {
    none = 0,
    limitReward = 1,
    limitEnter = 2,
}

public enum DungeonGroupType {
    none = 0,
    normal = 1,
    raid = 2,
    chaosRaid = 3,
    reverseRaid = 4,
    lapenta = 5,
    guildRaid = 6,
    darkStream = 7,
    worldBossDungeon = 8,
    item = 9,
    vip = 10,
    @event = 11,
    fameChallenge = 12,
    colosseum = 13,
    turka = 14,
}

public enum DungeonCooldownType {
    none = 0,
    dayOfWeeks = 1,
    nextDay = 2,
}

public enum DungeonTimerType {
    none = 0,
    clock = 1,
    gauge = 2,
}

public enum DungeonBossRankingType {
    None = 0,
    Kill = 1,
    Damage = 2,
    KillRumble = 3,
    MultiKill = 4,
    Colosseum = 5,
}

public enum DungeonRequireRole {
    None = 0,
    Support = 1,
    Tank = 2,
}

public enum DungeonEnterLimit : byte {
    Rookie = 0,
    Veteran = 1,
    MinLevel = 12,
    Achievement = 13,
    Vip = 14,
    Gearscore = 15,
    DungeonClear = 16,
    Buff = 17,
    RecommendedWeapon = 18,
}

public enum DungeonRoomModify : byte {
    [Description("s_room_dungeon_give_reward - You got your dungeon rewards.")]
    GiveReward = 1,
    [Description("s_room_dungeon_give_dungeonHelperReward - You got a Dungeon Helper reward.")]
    GiveDungeonHelperReward = 2,
    [Description("s_room_dungeon_reward_addExtraCount - The number of rewards has increased.")]
    AddExtraCount = 3,
    [Description("s_room_dungeon_record_notify_change_expert - You're now a veteran in {0}. Collect Dungeon Helper rewards by clearing the dungeon with rookies!")]
    ChangeToExpert = 4,
}

public enum DungeonAccumulationRecordType {
    [Description("s_dungeon_record_accum_damage - Total Damage: {0}")]
    TotalDamage = 0,
    [Description("s_dungeon_record_accum_heal - Total Healing: {0}")]
    TotalHealing = 1,
    [Description("s_dungeon_record_accum_hit_count - Total Hit Count: {0}")]
    TotalHitCount = 2,
    [Description("s_dungeon_record_boss_last_hit - Boss Final Blows: {0}")]
    BossFinalBlows = 3,
    [Description("s_dungeon_record_accum_move_distance - Total Move Distance: {0}")]
    TotalMoveDistance = 4,
    [Description("s_dungeon_record_accum_critical_damage - Total Critical Damage: {0}")]
    TotalCriticalDamage = 5,
    [Description("s_dungeon_record_max_critial_damage - Maximum Critical Damage: {0}")]
    MaximumCriticalDamage = 6,
    [Description("s_dungeon_record_accum_monster_kill - Defeated Monsters: {0}")]
    DefeatedMonsters = 7,
    [Description("s_dungeon_record_accum_be_hit_count - Incoming Damage: {0}")]
    IncomingDamage = 8,
    [Description("s_dungeon_record_accum_default_skill_damage - Basic Attack Damage: {0}")]
    BasicAttackDamage = 9,
}

public enum DungeonState : byte {
    None = 0,
    [Description("s_room_dungeon_clear - Dungeon Cleared.")]
    Clear = 1,
    [Description("s_room_dungeon_fail - Dungeon Failed.")]
    Fail = 2,
}

public enum DungeonMissionRank {
    None = -1,
    F = 0,
    C = 1,
    B = 2,
    A = 3,
    S = 4,
    SPlus = 5,
}

public enum DungeonRewardType : byte {
    Meso = 1,
    Exp = 2,
    Prestige = 3,
}

[Flags]
public enum DungeonBonusFlag {
    None = 1,
    [Description("s_dungeon_reward_dungeon_reward_count - Dungeon Clears: {0}")]
    Clear = 2,
    [Description("s_dungeon_reward_bonus_event - Event Bonus")]
    Event = 4,
    [Description("s_dungeon_reward_mission_rank - Rank Bonus")]
    MissionRank = 8,
    [Description("s_dungeon_reward_dungeon_helper - Dungeon Helper Bonus")]
    Helper = 16,
    [Description("s_dungeon_reward_dungeon_helper_event - Mutual Help Event")]
    MutualHelp = 32,
    [Description("s_dungeon_reward_mentor - Mentor Bonus")]
    Mentor = 64,
    [Description("s_dungeon_reward_mentee - Returning Player Bonus")]
    Mentee = 128,
    [Description("s_dungeon_reward_mentee_party_gift - Mentee Party Gift")]
    MenteePartyGift = 256,
    [Description("s_dungeon_reward_united_weekly - Weekly Bonus")]
    UnitedWeekly = 512,
}

public enum DungeonMissionType {
    LastHitNpc,
    PlayTime,
    DamageBySkill,
    DeathCount,
    GainBuff,
    LimitUserCount,
    Trigger,
    DamageToNpc,
}

[Flags]
public enum DungeonRecordFlag : byte {
    None = 0,
    Veteran = 1,
    Favorite = 2,
}
