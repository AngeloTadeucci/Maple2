// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum ItemInventoryError : int {
    [Description("The item amounts do not match.")]
    s_item_err_invalid_count = 12,
    [Description("Your inventory is full.")]
    s_err_inventory = 13,
    [Description("You cannot discard that pet, as there are items in its bag.")]
    s_err_cannot_destroy_petitem_hasitem = 31,
    [Description("Not enough merets.")]
    s_cannot_charge_merat = 34,
    [Description("A character's item can only be worn by that character.")]
    s_item_err_puton_invalid_binding = 35,
    [Description("A character's item can only be used by that character.")]
    s_item_err_use_invalid_binding = 36,
    [Description("This slot cannot be used.")]
    s_item_err_Invalid_slot = 37, // This may not work
    [Description("The tab you have selected in is inactive.")]
    s_item_err_not_active_tab = 38,
    [Description("You must have a character that's Lv. {1} or higher to drop {0}.")]
    s_itemdrop_error_restricted_by_user_level_ex = 40,
    [Description("You must have a character that's Lv. {1} or higher to pick up {0}.")]
    s_itempickup_error_restricted_by_user_level_ex = 41,
    [Description("This item cannot be discarded.")]
    s_item_err_drop = 42,
}
