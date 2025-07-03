
// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum PartySearchError : int {
    none = 0,
    [Description("Party Finder system error.\nPlease try again later.\nIf the error continues to occur, please contact customer support.")]
    s_partysearch_err_server_db = 2,
    [Description("You are already registered in the party finder.")]
    s_partysearch_err_server_already_register = 13, // or 14 or 15 or 105
    [Description("Processing the previous request. Please wait.")]
    s_partysearch_err_server_lastaction = 99,
    [Description("Only the party leader can do this.")]
    s_partysearch_err_server_not_chief = 101,
    [Description("Registration conditions do not match.\nPlease check again.")]
    s_partysearch_err_server_invalid_type = 102,
    [Description("Your listing is being posted.\nPlease wait.")]
    s_partysearch_err_server_registring = 103, // Only works with category 1
    [Description("The party has already been made.")]
    s_partysearch_err_server_in_party = 104,
    [Description("You already removed the recruitment listing.")]
    s_partysearch_err_server_not_find_recruit = 106, // Only works with category 1
    [Description("A prohibited word is in the title.\nPlease enter again.")]
    s_partysearch_err_server_banword_title = 108,
    [Description("A prohibited word is in the search text.\nPlease enter again.")]
    s_partysearch_err_server_banword_findword = 108, // Only works with category 2
    [Description("You cannot access the Party Finder at this time.")]
    s_partysearch_err_server_blocked = 109,
    [Description("Cannot apply for party recruitment because\nthe max number of party members has been reached.")]
    s_partysearch_err_server_max_member = 110,
    [Description("You cannot post a recruitment listing until you have dealt with the pending party invitation.")]
    s_partysearch_err_server_party_invited = 111,
}
