namespace Maple2.Model.Enum;

// Setting Values
// 0 = None / PvpEnabled
// 1 = Self
// 2 = Party
public enum HomePermission : byte {
    Jump = 0,
    Climb = 1,
    Skill = 2,
    Music = 3,
    Potion = 4,
    GroundMount = 5,
    AirMount = 6,
    Pvp = 7,
    Unknown = 8,
}

public enum HomePermissionSetting : byte {
    None = 0,
    Self = 1,
    Party = 2,
}
