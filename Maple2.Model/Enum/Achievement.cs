namespace Maple2.Model.Enum;

public enum AchievementStatus : byte {
    InProgress = 2,
    Completed = 3,
}

public enum AchievementCategory {
    None = 0,
    Combat = 1,
    Adventure = 2,
    Life = 3,
}

public enum AchievementRewardType {
    none = 0,
    item = 1,
    title = 2,
    statpoint = 3,
    skillpoint = 4,
    shop_weapon = 5,
    shop_build = 6,
    shop_ride = 7,
    itemcoloring = 8,
    beauty_makeup = 9,
    beauty_skin = 10,
    beauty_hair = 11,
    dynamicaction = 12,
    etc = 13,
}
