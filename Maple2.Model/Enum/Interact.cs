using System.ComponentModel;

namespace Maple2.Model.Enum;

public enum InteractType : byte {
    Mesh = 1,
    Telescope = 2,
    Ui = 3,
    Web = 4,
    DisplayImage = 5,
    Gathering = 6,
    GuildPoster = 7,
    BillBoard = 8, // AdBalloon
    WatchTower = 9,
}

public enum InteractState : byte {
    Normal = 0,
    Reactable = 1,
    Hidden = 2,
}

// ReSharper disable InconsistentNaming, IdentifierTypo
public enum InteractResult : byte {
    none = 0,
    [Description("You get a good look at the area.")]
    s_interact_find_new_telescope = 0,
    [Description("System Error: interact. {0}")]
    s_interact_result_unknown = 1,
    [Description("You are not in the middle of a quest.")]
    s_interact_result_quest = 2,
    [Description("Only the party leader has the power to do that.")]
    s_interact_result_party = 3,
    [Description("That cannot be done on this map.")]
    s_tutorial_dialog_limit = 4,
    [Description("You don't have permission to do that.")]
    s_interact_result_privilege = 5,
    [Description("You don't have permission to do that.")]
    s_interact_result_auth = 7,
    // 12
    [Description("Requires rank {1} {0}.")] // {0} Life Skill Type, {1} Life Skill Rank
    s_interact_result_mastery = 13,
}

public enum GatherResult : short {
    Success = 0,
    Fail = 1,
}

public enum InteractCubeState {
    None = 0,
    InUse = 1,
    Available = 2,
}

public enum InteractCubeControlType {
    None,
    Farming,
    Breeding,
    Switch,
    Skill,
    Nurturing,
    Ride,
    SpawnNPC,
    OpenWeb,
    Sensor,
    FunctionUI,
    Notice,
    InstallNPC,
    Portal,
    SpawnPoint,
    PVP,
}
