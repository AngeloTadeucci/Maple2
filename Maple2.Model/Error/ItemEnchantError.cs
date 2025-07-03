// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum ItemEnchantError : short {
    [Description("System Error: Item. code = {0}")]
    s_itemenchant_unknown_err = 0,
    [Description("This item cannot be enchanted.")]
    s_itemenchant_invalid_item = 1,
    [Description("The item is unstable.")]
    s_itemenchant_damaged_item = 2,
    [Description("Not enough materials.")]
    s_itemenchant_lack_ingredient = 3,
    not_enough_fodder = 4,
    excess_fodder = 5,
    max_fodder = 6,
    over_100_success_rate = 7,
}
