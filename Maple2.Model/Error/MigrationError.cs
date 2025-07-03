// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum MigrationError : byte {
    ok = 0,
    [Description("The server is full. Try again later.")]
    s_move_err_field_limit = 7,
    [Description("Server not found.")]
    s_move_err_no_server = 8,
    [Description("Reached the player limit.")]
    s_move_err_over_user = 9, // also 21
    [Description("You cannot enter the channel because it is full.\nPlease use another channel.")]
    s_move_err_member_limit = 10, // also 22
    [Description("Cannot enter because the time limit has run out.")]
    s_move_err_time_out = 12,
    [Description("The dungeon cannot be found.")]
    s_move_err_dungeon_not_exist = 17,
    [Description("You are not the party leader.")]
    s_party_err_not_chief = 27,
    [Description("Only a party can enter.")]
    s_move_err_NotFoundParty = 28,
    // s_move_err_dungeon_not_exist message, but also sets v16 = 1?
    [Description("The dungeon cannot be found.")]
    s_move_err_dungeon_not_exist_2 = 29,
    [Description("Failed to create a dungeon.")]
    s_move_err_FailCreateDungeon = 30,
    [Description("Cannot enter while looking for a dungeon.")]
    s_move_err_DungeonMatch = 31,
    [Description("A party member is still in the dungeon.\nPlease try again after all party members have exited the dungeon.")]
    s_move_err_InsideDungeonUser = 32,
    [Description("The entry time has passed. You can no longer enter.")]
    s_move_err_ExpireEnterTime = 33,
    [Description("A party member is still in Mushking Royale. Please try again after all party members have exited.")]
    s_move_err_InsideSurvivalSquad = 34,
    [Description("Cannot enter because the wedding has completed.")]
    s_move_err_wedding_complete = 35,

    [Description("An unknown error has occurred while moving the server. Code={0}")]
    s_move_err_default = byte.MaxValue,
}
