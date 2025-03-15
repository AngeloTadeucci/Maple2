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
    Unknown = 0,
    None = 1,
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
