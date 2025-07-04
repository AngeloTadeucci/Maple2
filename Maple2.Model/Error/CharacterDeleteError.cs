// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum CharacterDeleteError : int {
    ok = 0,
    [Description("This character has already been deleted.")]
    s_char_err_already_destroy = 1,
    [Description("Because you own real estate, there must be at least 1 active character on the account.")]
    s_char_err_exist_ugc_map = 2,
    [Description("A guild leader cannot be deleted.")]
    s_char_err_guild_master = 3,
    [Description("You cannot delete a character while they are a member of a guild.")]
    s_char_err_guild = 4,
    [Description("The character cannot be deleted because they have an item listed in the Design Shop.")]
    s_char_err_ugc_market = 5,
    [Description("The character cannot be deleted because they have an item listed on the Black Market.")]
    s_char_err_black_market_count = 6,
    [Description("The character cannot be deleted because it has unread mail.")]
    s_char_err_unread_mail = 7,
    [Description("This character is not waiting to be deleted.")]
    s_char_err_no_destroy_wait = 8,
    [Description("The character cannot be deleted because they have mesos listed on the Black Market.")]
    s_char_err_meso_market_count = 9,
    // {0}: 2018-01-00 00:00:00 Time Format
    [Description("You cannot delete this character until {0}.")]
    s_char_err_next_delete_char_date = 10,
    [Description("The character cannot be deleted because they are engaged, married, or in process of divorce.")]
    s_char_err_wedding = 11,

    [Description("System Error: Can't delete character [code = {0}]")]
    s_char_err_destroy = byte.MaxValue,
}
