// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum MesoMarketError {
    none = 0,
    [Description("An error occurred with the request. Please try again.")]
    s_mesoMarket_error_errorDB = 2,
    [Description("Meso trading is currently unavailable.")]
    s_mesoMarket_error_shutDown = 3,
    [Description("Another request is being processed. Please try again later.")]
    s_mesoMarket_error_alreadyDbWork = 4,
    [Description("You do not have enough Mesos to list.")]
    s_mesoMarket_error_hasNotMeso = 5,
    [Description("Not enough merets.\nDo you want to buy merets?")]
    s_err_lack_merat_ask = 6,
    [Description("You don't have enough Meso Tokens. Do you want to buy some?")]
    s_err_lack_meso_makret_token_ask = 7,
    [Description("You reached the listing limit.\nYou cannot list any more.")]
    s_mesoMarket_error_maxCountOwnProduct = 8,
    [Description("The item could not be sold.")]
    s_mesoMarket_error_notFoundProduct = 9,
    [Description("An error occurred in the search range.")]
    s_mesoMarket_error_invalidSearchParam = 10,
    [Description("You can't purchase your own mesos.")]
    s_mesoMarket_error_cannotBuyOwnProduct = 11,
    [Description("The item could not be sold.")]
    s_mesoMarket_error_cannotBuyExpireDate = 12,
    [Description("You reached the listing limit for the day.\nNo more mesos can be listed today.")]
    s_mesoMarket_error_maxCountRegister = 13,
    [Description("You've reached your meso purchase limit.\nThe limit is reset on the 1st of every month. You cannot purchase any more mesos this month.")]
    s_mesoMarket_error_maxCountBuy = 14,
    [Description("Invalid quantity of Mesos to sell.")]
    s_mesoMarket_error_invalidSaleMoney = 15,
    [Description("Your sale price must be within {0}% of the current average price, or between {1} and {2} Meso Tokens.")]
    s_mesoMarket_error_invalidBuyMerat = 16,
    [Description("This item is sold out.")]
    s_mesoMarket_error_alreadySoldProduct = 17,
    // Not sure exactly how these two work, but they have something to do with suspensions
    // 1097, 1098, 1102, 1103, 1104, 1106
    [Description("Prohibited because of your Penalty.")]
    s_admin_block_velma_msgbox_content = 18,
    [Description("Prohibited because of your Citation.")]
    s_admin_block_velma_ugc_msgbox_content = 19,

    [Description("System Error: Black Market Meso Trade code={0}")]
    s_mesoMarket_error_unknown = byte.MaxValue,
}
