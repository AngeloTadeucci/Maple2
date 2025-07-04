// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum DungeonMatchError : byte {
    ok = 0,
    [Description("You or your party are not eligible to enter any dungeons at this time.\nPlease check again after increasing your Levels and Gear Scores.")]
    s_dungeonMatch_error_notFoundDungeon = 1,
    [Description("Up to {0} people are allowed.")]
    s_dungeonMatch_error_overMaxRegisterUser = 2,
    [Description("You do not meet the dungeon entry requirements.")]
    s_dungeonMatch_error_lackRequire = 3,
    [Description("One of the selected dungeons cannot be entered. Please adjust your selection.")]
    s_dungeonMatch_error_containCouldNotEnter = 4,
    [Description("A party member is still in the dungeon.\nPlease try again after all party members have exited the dungeon.")]
    s_dungeonMatch_error_insideDungeonUser = 5,
    [Description("Only the party leader can send the request.")]
    s_dungeonMatch_error_isNotChief = 6,
    [Description("A party member has disconnected.")]
    s_dungeonMatch_error_hasOfflineUser = 7,
    [Description("You cannot search for a new recommended normal adventure dungeon party while [Comradery] is active.")]
    s_dungeonMatch_error_hasDungeonMatchCooldown = 8,
    [Description("[Comradery] will expire in {0}. You may not search for a new recommended normal adventure dungeon party during this time.")]
    s_dungeonMatch_error_hasDungeonMatchCooldownParty = 9,
}
