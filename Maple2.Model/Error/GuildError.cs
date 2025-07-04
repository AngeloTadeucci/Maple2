// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum GuildError : byte {
    none = 0,
    [Description("Unknown Guild Error")]
    s_guild_err_unknown = 1,
    [Description("Guild not found.")]
    s_guild_err_null_guild = 3,
    [Description("This character is already a member of a guild.")]
    s_guild_err_already_exist = 4,
    [Description("Unable to send invite.")]
    s_guild_err_wait_inviting = 5,
    [Description("Guild invitation failed.")]
    s_guild_err_blocked = 6,
    [Description("They have already joined another guild.")]
    s_guild_err_has_guild = 7,
    [Description("The guild is no longer valid.")]
    s_guild_err_invalid_guild = 8,
    [Description("Unable to invite the player to your guild.")]
    s_guild_err_null_user = 10,
    [Description("A guild with the same name already exists.")]
    s_guild_err_name_exist = 11,
    [Description("Contains a forbidden word.")]
    s_guild_err_name_value = 12,
    [Description("Guild member not found.")]
    s_guild_err_null_member = 13,
    [Description("The guild cannot be disbanded if there are any guild members.")]
    s_guild_err_exist_member = 14,
    [Description("You have reached the maximum number of guild members.")]
    s_guild_err_full_member = 15,
    [Description("This guild member has not joined.")]
    s_guild_err_not_join_member = 16,
    [Description("The guild leader cannot leave the guild.")]
    s_guild_err_cannot_leave_master = 17,
    [Description("You cannot kick the guild leader.")]
    s_guild_err_expel_target_master = 18,
    [Description("To create a guild, you must be above level {0}.")]
    s_guild_err_not_enough_level = 19,
    [Description("Not enough mesos.")]
    s_guild_err_no_money = 20,
    [Description("You don't have permission to do that.")]
    s_guild_err_no_authority = 21,
    [Description("Only the guild leader can do that.")]
    s_guild_err_no_master = 22,
    [Description("This rank cannot be used.")]
    s_guild_err_invalid_grade_range = 23,
    [Description("You cannot change the maximum amount of guild members to this value.")]
    s_guild_err_invalid_capacity_range = 24,
    [Description("This rank cannot be used.")]
    s_guild_err_invalid_grade_data = 25,
    [Description("Incorrect rank.")]
    s_guild_err_invalid_grade_index = 26,
    [Description("This rank cannot be granted.")]
    s_guild_err_exist_empty_grade_index = 27,
    [Description("Rank setting failed.")]
    s_guild_err_set_grade_failed = 28,
    [Description("Guild member invitation failed.")]
    s_guild_err_fail_addmember = 30,
    [Description("This character was not invited.")]
    s_guild_err_null_invite_member = 32,
    [Description("Cannot be done during a guild battle.")]
    s_guild_err_cant_during_pvp = 33,
    [Description("The guild member maximum cannot be changed.")]
    s_guild_extend_capacity_err_cannot = 34,
    [Description("A problem occurred while changing the maximum amount of guild members.")]
    s_guild_extend_capacity_err_current = 35,
    [Description("Preferences have remained the same.")]
    s_guild_search_same_propensity = 36,
    [Description("You can submit up to 10.")]
    s_guild_search_max_join_request = 37,
    [Description("Please wait a moment.")]
    s_guild_search_last_request = 38,
    [Description("Application not found.")]
    s_guild_search_null_join_guild_request = 39,
    [Description("The target is in a location where they cannot be invited.")]
    s_guild_err_fail_this_field = 41,
    [Description("The guild level is not high enough.")]
    s_guild_err_not_enough_guild_level = 42,
    [Description("Not enough guild funds.")]
    s_guild_err_not_enough_guild_fund = 43,
    [Description("You cannot use the guild skill right now.")]
    s_guild_err_cannot_use_skill = 44,
    [Description("Please try again later.")]
    s_guild_err_too_fast_to_search_by_name = 45,
    [Description("You have already arrived at the Glorious Arena.")]
    s_guild_pvp_already_pvp_field = 46,
    [Description("Applications are not currently being accepted.")]
    s_guild_err_isNotVsGameTime = 47,
    [Description("You need at least {0} players online.")]
    s_guild_err_requireOnlineUserCount = 48,

    [Description("Undefined Guild Error")]
    s_guild_err_none = byte.MaxValue,
}
