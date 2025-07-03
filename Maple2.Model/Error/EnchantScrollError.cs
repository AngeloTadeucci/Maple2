// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum EnchantScrollError : short {
    [Description("The enchantment was a success!")]
    s_enchantscroll_ok = 0,
    [Description("The selected items are no longer valid.")]
    s_enchantscroll_invalid_scroll = 1,
    [Description("That item is not eligible.")]
    s_enchantscroll_invalid_item = 2,
    [Description("Unstable items cannot be enchanted.")]
    s_enchantscroll_breaking_item = 3,
    [Description("The selected gear cannot be enchanted with this scroll due to its level.")]
    s_enchantscroll_invalid_level = 4,
    [Description("The selected gear cannot be enchanted with this scroll.")]
    s_enchantscroll_invalid_slot = 5,
    [Description("The selected gear cannot be enchanted with this scroll due to its grade.")]
    s_enchantscroll_invalid_rank = 6,
    [Description("Cannot be used because the enchantment grade of the selected gear is higher.")]
    s_enchantscroll_invalid_grade = 7,
    [Description("If the item is unstable, it cannot be restored.")]
    s_enchantscroll_not_breaking_item = 8,
}
