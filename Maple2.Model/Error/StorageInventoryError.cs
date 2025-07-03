// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum StorageInventoryError {
    [Description("The item amounts do not match.")]
    s_item_err_invalid_count = 10,
    [Description("This item cannot be stored here.")]
    s_item_err_invaild_store_type = 12,
    [Description("Storage is full.")]
    s_item_err_store_full = 13,
    [Description("This cannot be expanded any further.")]
    s_store_err_expand_max = 14,
    [Description("Not enough merets.")]
    s_cannot_charge_merat = 15,
    [Description("This item cannot be stored.")]
    s_item_err_binditem = 16,
    [Description("An item belonging to a specific character can only be retrieved by that character.")]
    s_item_err_binditem_store_out = 17,
    [Description("Meso storage is not possible with this safe.")]
    s_store_err_deposit_disable_type = 18,
    [Description("Confirm the amount entered.")]
    s_store_err_deposit_invalid_money = 19,
    [Description("Up to {0} mesos can be stored.")]
    s_store_err_deposit_max_money = 20,
    [Description("Not enough funds.")]
    s_cashshop_lack_balance = 21,
    [Description("This item can only be retrieved by the character that stored it.")]
    s_item_err_moveDisableitem_store_out = 22,

    [Description("Bank safe error code: {0}")]
    s_store_err_code = byte.MaxValue,
}
