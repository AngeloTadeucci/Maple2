namespace Maple2.Model.Enum;

public enum FieldProperty : byte {
    Gravity = 1,
    MusicConcert = 2,
    HidePlayer = 3,
    LockPlayer = 4,
    UserTagSymbol = 5,
    SightRange = 6,
    Weather = 7,
    AmbientLight = 8,
    DirectionalLight = 9,
    LocalCamera = 10,
    PhotoStudio = 11,
}

public enum WeatherType : byte {
    None = 0,
    Snow = 1,
    HeavySnow = 2,
    Rain = 3,
    HeavyRain = 4,
    SandStorm = 5,
    CherryBlossom = 6,
    LeafFall = 7,
}
