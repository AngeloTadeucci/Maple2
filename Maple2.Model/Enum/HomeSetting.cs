namespace Maple2.Model.Enum;

public enum HomeBackground : byte {
    Basic = 0,
    GreenMeadow = 1,
    MoonlitForest = 2,
    RainbowSnowfield = 3,
    Skyscraper = 4,
    BlueSea = 5,
    BarrenMountains = 6,
    Space = 7,
    Ludibrium = 8,
    FutureCity = 9,
    TwilightDesert = 10,
    LavaCave = 11,
    SweetVista = 12,
    VastSky = 13,
}

public enum HomeLighting : byte {
    Basic = 0,
    Natural = 1,
    Warm = 2,
    Cool = 3,
    Dark = 4,
    Soft = 5,
    Dusk = 6,
    Dawn = 7,
    Winter = 8,
}

public enum HomeCamera : byte {
    QuarterView = 0,
    SideView = 1,
    TopView = 2,
    AreaView = 3,
}

public enum PlotMode : byte {
    Normal = 0,
    DecorPlanner = 1,
    ModelHome = 2,
    BlueprintPlanner = 3,
}
