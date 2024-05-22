// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum BlackMarketError : int {
    none = 0,
    [Description("Failed to list item.\nThe item and deposit will be returned by mail.")]
    s_blackmarket_error_fail_register = 5,
    [Description("The item could not be sold.")]
    s_err_null_product = 13,
    [Description("You can't list items that are not in your inventory.\nSelect another item.")]
    s_blackmarket_error_register_not_exist_in_inven = 14,
    [Description("You have reached the listing limit.\nYou can list up to {0} items.")]
    s_blackmarket_error_max_register_item = 21,
    [Description("Not enough mesos.")]
    s_err_lack_meso = 22,
    [Description("This item is sold out.")]
    s_err_lack_itemcount = 23,
    [Description("Please enter the amount you wish to sell once more.")]
    s_blackmarket_error_invalid_sale_count = 25,
    [Description("Please enter the amount you wish to sell once more.")]
    s_blackmarket_error_lack_sale_count = 26,
    [Description("The item has expired.")]
    s_blackmarket_error_buy_expired = 27,
    [Description("The Black Market can't be used right now.")]
    s_blackmarket_error_not_useable = 29,
    [Description("This item cannot be listed due to the initial listing.\nPlease try listing the item again.")]
    s_blackmarket_error_already_add = 30,
    [Description("This item can't be listed on the Black Market.")]
    s_blackmarket_error_disable_registitem = 31,
    [Description("This word cannot be used.")]
    s_ban_check_err_all_word = 36,
    [Description("A restriction will be placed on Black Market for 60 seconds after entering the game.")]
    s_system_property_protection_time = 37,
    [Description("The game will not run due to fatigue time.")]
    s_anti_addiction_cannot_receive = 38,
    [Description("The Black Market can't be used right now.")]
    s_blackmarket_error_close = 39,
    [Description("Not enough quantity available for purchase.")]
    s_blackmarket_error_purchase_count = 41,
    [Description("You cannot purchase an item listed by a character on your account.")]
    s_blackmarket_error_cannot_buy_ownProduct = 42,
    [Description("You cannot sell in the Black Market if the highest level character in your account is less than {0}.")]
    s_blackmarket_error_sell_restricted_by_user_level = 43,
    [Description("You cannot purchase from the Black Market if the highest level character in your account is less than {0}.")]
    s_blackmarket_error_buy_restricted_by_user_level = 44,

    // Unknown values for the following:
    /*[Description("This item includes untradeable gemstones and cannot be listed on the Black Market.")]
    s_blackmarket_error_disable_registitem_by_gemstone,
    [Description("The item has already been sold.")]
    s_blackmarket_error_already_remove,
    [Description("Please enter the sale price once more.")]
    s_blackmarket_error_invalid_sale_price,
    [Description("The total sale price must be between {0} and {1}.")]
    s_blackmarket_error_invalid_sale_price_range,
    [Description("The Black Market can't be used right now.")]
    s_blackmarket_error_not_useable_by_dead,
    [Description("You cannot afford the listing deposit.")]
    s_blackmarket_error_register_lack_deposit,
    [Description("You must set a name or classification to use advanced search.")]
    s_blackmarket_error_cannot_searchex_without_condition,
    [Description("Your character must be at least {0} hours old to sell on the Black Market.")]
    s_blackmarket_error_sell_restricted_by_char_cdate,
    [Description("Your character must be at least {0} hours old to buy from the Black Market.")]
    s_blackmarket_error_buy_restricted_by_char_cdate,
    [Description("You must have a character that's Lv. {1} or higher to list {0} on the Black Market.")]
    s_blackmarket_error_sell_restricted_by_user_level_ex,
    [Description("You must have a character that's Lv. {1} or higher to buy {0} on the Black Market.")]
    s_blackmarket_error_buy_restricted_by_user_level_ex,*/
}
