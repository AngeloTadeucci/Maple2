// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum ItemMergeError : short {
    ok = 0,
    [Description("Not enough materials.")]
    s_item_merge_option_error_lack_material = 1,
    [Description("Not enough mesos.")]
    s_item_merge_option_error_lack_meso = 2,
    [Description("This empowerment item is no longer valid.")]
    s_item_merge_option_error_invalid_mergeitem = 3,
    [Description("Invalid material item.")]
    s_item_merge_option_error_invalid_material = 4,
    [Description("That empowerment crystal is no longer valid.")]
    s_item_merge_option_error_invalid_merge_scroll = 5,
    [Description("You can only remove the attributes of an item that is the same tier as the empowerment crystal.")]
    s_item_merge_option_error_invalid_revert_rank = 6,
    [Description("No attributes to remove.")]
    s_item_merge_option_error_invalid_revert_option = 7,
}
