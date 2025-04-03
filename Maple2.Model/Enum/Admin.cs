namespace Maple2.Model.Enum;

public enum ReportCategory : byte {
    Player = 0,
    Chat = 1,
    Poster = 2,
    ItemDesign = 3,
    Home = 4,
    Pet = 7,
}

[Flags]
public enum PlayerReportFlag {
    None = 0,
    CharacterPortrait = 1,
    Clothes = 2,
    Hacking = 4,
    Behavior = 8,
    Misc = 128,
}

[Flags]
public enum HomeReportFlag {
    None = 0,
    ItemDesign = 1,
    ItemPlacement = 2,
    HomeName = 4,
    Copyright = 8,
}

[Flags]
public enum PosterReportFlag {
    None = 0,
    Copyright = 1,
    Swearing = 2,
    Derogatory = 4,
    CommercialAdvertisement = 8,
}

[Flags]
public enum DesignItemReportFlag {
    None = 0,
    Design = 1,
    Name = 2,
    Description = 4,
    Copyright = 8,
    Copying = 16,
}

[Flags]
public enum ChatReportFlag {
    None = 0,
    Swearing = 1,
    Derogatory = 2,
    RMT = 4,
    Spam = 8,
}

[Flags]
public enum PetReportFlag {
    None = 0,
    Name = 1,
}


[Flags]
public enum AdminPermissions {
    Alert,
    Ban,
    SpawnItem,
    SpawnNpc,
    Debug,
    Quest,
    Warp,
    Find,
    PlayerCommands,

    Spawn = SpawnNpc | SpawnItem,
    Admin = int.MaxValue,
}
