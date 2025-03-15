// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum DungeonRoomError {
    none = 0,
    [Description("The party leader must enter first.")]
    s_room_party_err_not_chief = 1,
    [Description("Entry limit reached.")]
    s_room_party_err_full_room = 2,
    [Description("You do not have permission to enter.")]
    s_room_dungeon_error_invalidPartyOID = 3,
    [Description("The number of rewards for this dungeon cannot be increased any further.")]
    s_room_dungeon_reward_CantUseExtraReward = 5,
    [Description("You do not have the {0} item.\nDo you want to purchase the item from the Meret Market?")]
    s_dungeonRoom_lack_extra_ticket = 6,
    [Description("You can no longer increase the number of rewards.")]
    s_room_dungeon_max_reward_count = 7,
    [Description("This dungeon has expired. Entry is no longer possible.")]
    s_room_dungeon_expired = 8,
    [Description("Preparing.")]
    s_room_dungeon_commingSoon = 9,
    [Description("You cannot enter a dungeon while searching for a dungeon.")]
    s_room_dungeon_canEnterOnDungeonMatch = 10,
    [Description("You cannot enter a dungeon during the tutorial.")]
    s_room_dungeon_canEnterTutorialField = 11,
    [Description("No entry allowed today.\\nPlease check the entry conditions in the dungeon information.")]
    s_room_dungeon_canEnterDayOfWeeks = 12,
    [Description("You are already in the hall.")]
    s_room_dungeon_AlreadyChaosHall = 13,
    [Description("You are already $map:63000063$.")]
    s_room_dungeon_AlreadyLapentaHall = 14,
    [Description("You are already in the Queen's Parlor.")]
    s_room_dungeon_AlreadyColosseumHall = 15,
    [Description("You cannot move while inside a dungeon.")]
    s_room_dungeon_CantEnterInDungeon = 16,
    [Description("Up to {0} people are allowed.")]
    s_room_dungeon_OverMaxUserCount = 17,
    [Description("Requires at least {0} party members.")]
    s_room_dungeon_UnderMinUserCount = 18,
    [Description("Rewards can only be taken on days when entry is allowed.")]
    s_room_dungeon_noRewardDayOfWeeks = 19,
    [Description("You do not have the item needed for entry.")]
    s_room_dungeon_HasNotLimitItem = 20,
    [Description("Entry is not allowed at this time.\\nPlease check the entry conditions in the dungeon information.")]
    s_room_dungeon_NotAllowTime = 21,
    [Description("That cannot be used while inside a dungeon.")]
    s_room_dungeon_CantUseAtDungeonRoom = 22,
    [Description("You cannot enter at this time.")]
    s_room_dungeon_notOpenTimeDungeon = 23,
    [Description("It is too early to abandon the dungeon.")]
    s_room_dungeon_cannot_giveup_yet = 24,
    [Description("In order to enter, all party members must be from the same guild.")]
    s_room_dungeon_require_guild_partry = 25,
    [Description("New guild members cannot participate until the Guild Raid Score is reset.")]
    s_room_dungeon_require_guild_join_date = 26,
    [Description("This function is not currently available.")]
    s_room_dungeon_shutdown_find_dungeon_helper = 27,
    [Description("This dungeon is not available yet. \\nCheck the requirements in the Dungeon Info menu.")]
    s_room_dungeon_is_not_open_period_date = 28,
    [Description("This dungeon is not available yet. \\nCheck the requirements in the Dungeon Info menu.")]
    s_room_dungeon_is_not_open_period = 29,
    [Description("One or more party members can't enter.")]
    s_room_dungeon_cant_enter_in_partymember = 30,
    [Description("You've already reset the weekly clear count for a character on your account this week.\\nTry again after midnight on Friday.")]
    s_room_dungeon_error_used_reset_united_reward = 31,
    [Description("You have not yet reached the weekly clear count limit.")]
    s_room_dungeon_error_still_have_united_reawrd = 32,
    [Description("The weekly clear count reset feature is still in its beta phase and has been temporarily disabled.")]
    s_room_dungeon_error_shutdown_united_reawrd_reset = 33,
}
