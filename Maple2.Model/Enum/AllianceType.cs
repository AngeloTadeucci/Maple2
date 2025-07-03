using System.ComponentModel;

namespace Maple2.Model.Enum;

public enum AllianceType : short {
    [Description("none")]
    AnyFaction = 0,
    [Description("darkwind")]
    DarkWind = 1,
    [Description("mapleunion")]
    MapleAlliance = 2,
    [Description("lumiknight")]
    Lumiknights = 3,
    [Description("triaroyalguard")]
    RoyalGuard = 4,
    [Description("greenhood")]
    GreenHoods = 5,
    [Description("mapleunion_kritiasexped")]
    KritiasMapleAlliance = 6,
    [Description("lumiknight_kritiasexped")]
    KritiasLumiknights = 7,
    [Description("greenhood_kritiasexped")]
    KritiasGreenHoods = 8,
    // [Description("georg")]
    // georg = 9,
}
