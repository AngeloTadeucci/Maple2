// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum ItemExchangeScrollError : short {
    [Description("Fusion successful.")]
    s_itemslot_exchange_ok = 0,
    [Description("The selected items are no longer valid.")]
    s_itemslot_exchange_scroll_invalid = 1,
    [Description("This item cannot be fused.")]
    s_itemslot_exchange_upgrade_invalid = 2,
    [Description("Not enough mesos.")]
    s_itemslot_exchange_money_invalid = 3,
    [Description("You do not have enough items.")]
    s_itemslot_exchange_count_invalid = 4,
    [Description("The enchant level for this equipment is too high to fuse.")]
    s_itemslot_exchange_grade_invalid = 5,
    [Description("This item is locked.")]
    s_itemslot_exchange_lockstate_item = 6,
    [Description("Please double-check the number of fusions.")]
    s_itemslot_exchange_count_check = 7,
    [Description("System error.")]
    s_itemslot_exchange_unknown = 8,
}
