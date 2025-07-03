// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum TradeError : byte {
    [Description("Trade failed.")]
    s_trade_error_system = 0,
    [Description("The trade request failed because the target is too far away.")]
    s_trade_error_distance = 1,
    [Description("{0} has already received a trade request.")]
    s_trade_error_already_request = 2,
    [Description("Trade in progress.")]
    s_trade_error_trading_now = 3,
    [Description("Your offer has already been finalized.")]
    s_trade_error_latched = 5,
    [Description("Trading is not possible with this target.")]
    s_trade_error_decline = 6,
    [Description("All slots in the trade window are full. Remove an item, or make deliver the item through a separate trade.")]
    s_trade_error_itemcount = 7,
    [Description("The other player's inventory is full.")]
    s_trade_error_slotcount = 8,
    [Description("The trade has been canceled because there are no items to trade.")]
    s_trade_error_itemnone = 9,
    [Description("You cannot trade during PvP.")]
    s_trade_error_pvp = 10,
    [Description("This can't be done here.")]
    s_trade_error_mapLimit = 11,
    [Description("{0} failed to respond to your trade request.")]
    s_trade_error_timeout = 12,
    [Description("Confirm the trade amount.")]
    s_trade_error_invalid_meso = 13,
    [Description("Not enough mesos.")]
    s_trade_error_meso = 14,
    [Description("{0} is in protected mode.")]
    s_trade_error_target_property_protection_time = 15,
    [Description("Trading is not possible due to {0}'s fatigue.")]
    s_trade_error_target_fatigue_penalty = 16,
    [Description("")]
    unknown = 17,
    [Description("You cannot trade if the highest level character in your account is less than {0}.")]
    s_trade_error_restricted_userlevel_max = 18,
    [Description("You cannot trade with this player because their highest level character is less than {0}.")]
    s_trade_error_restricted_target_userlevel_max = 19,
    [Description("You must have a character that&apos;s Lv. {1} or higher to trade {0}.")]
    s_trade_error_send_restricted_by_user_level_ex = 20,
    [Description("The person you are trading with must have a character that&apos;s Lv. {1} or higher to trade {0}.")]
    s_trade_error_recv_restricted_by_user_level_ex = 21,

    [Description("System Error: Community")]
    s_trade_error = byte.MaxValue,
}
