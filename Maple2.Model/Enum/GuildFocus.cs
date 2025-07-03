namespace Maple2.Model.Enum;

[Flags]
public enum GuildFocus {
    // Guild Focus
    Social = 1,
    HuntingParties = 2,
    TrophyCollection = 4,
    Dungeons = 8,
    HomeDesign = 16,
    Pvp = 32,
    WorkshopTemplates = 64,
    GuildArcade = 128,
    // Active Times
    Weekdays = 256,
    Mornings = 512,
    Weekends = 1024,
    Evenings = 2048,
    // Member Ages
    Teens = 4096,
    Thirties = 8192,
    Twenties = 16384,
    Other = 32768,
}
