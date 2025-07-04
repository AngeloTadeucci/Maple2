namespace Maple2.Model.Enum;

public enum MapRegion {
    None = 0,
    Tutorial = 1, // Includes Maple Island
    LithHarbor = 2,
    Tria = 3,
    Kerning = 4,
    Henesys = 5,
    Perion = 6,
    Ellinia = 7,
    Taliskar = 8,
    Ludibrium = 9,
    Ludari = 11,
    KarkarDesert = 12,
    Nazkar = 13,
    Calibre = 14,
    Kritias = 15,
    ShadowWorld = 100,
}

public enum MapType {
    None = 0,
    Quest = 1,
    Life = 2, // land plots and harvestable objects
    Home = 3,
    Telescope = 4,
    Dungeon = 5,
    PocketRealm = 7,
    Event = 8,
    Pvp = 9,
    Alikar = 10,
    Shadow = 11,
    Shelter = 12,
    Arcade = 13,
    Event2 = 14,
    SurvivalSolo = 15,
    SurvivalTeam = 16,
    FieldWar = 17,
}

public enum Continent {
    MapleIsland = 101,
    VictoriaIsland = 102,
    KarkarIsland = 103,
    Calibre = 104,
    Kritias = 105,
    ShadowWorld = 202,
}

public enum AutoReviveType {
    None = 0,
    Trigger = 1,
    Countdown = 2, // Only map 65000003 - Treasure Island. Uses autoRevivalTime
}
