// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum ShopError {
    [Description("Not enough supplies.")]
    s_err_lack_shopitem = 2,
    [Description("Item not found.")]
    s_err_invalid_item = 4,
    [Description("Cannot be sold.")]
    s_msg_cant_sell = 9,
    [Description("Not enough mesos.")]
    s_err_lack_meso = 10,
    [Description("Not enough merets.")]
    s_err_lack_merat = 11,
    [Description("Your inventory is full.")]
    s_err_inventory = 14,
    [Description("Not enough guild trophies.")]
    s_err_lack_guild_trophy = 15,
    [Description("Insufficient items. (Missing Item: $item:{0}$)")]
    s_err_lack_payment_item = 17,
    [Description("You can buy after being in the guild for 7 days.")]
    s_err_lack_guild_require_date = 19,
    [Description("The game will not run due to fatigue time.")]
    s_anti_addiction_cannot_receive = 22,
    [Description("You can't sell items to this shop.")]
    s_msg_cant_sell_to_only_sell_shop = 23,
    [Description("A restriction will be placed on Trade for 60 seconds after entering the game.")]
    s_system_property_protection_time = 24,
    [Description("Can only be purchased by guild leaders.")]
    s_guild_err_buy_no_master = 25,
    [Description("Not enough guild funds.")]
    s_guild_err_not_enough_guild_fund = 26,
    [Description("You cannot purchase this at this time.")]
    s_err_invalid_item_cannot_buy_by_period = 27,
    [Description("Can only be used during the event period.")]
    s_shop_no_star_point_event = 29,
    [Description("This item cannot be purchased in the country from which you are connecting.")]
    s_meratmarket_error_country_limit = 31,
    [Description("You cannot sell a pet while it is summoned. Please unsummon your pet and try again.")]
    s_err_cannot_sell_petitem_summon = 32,
}
