// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum ChangeAttributesScrollError {
    none = 0,
    [Description("This item is not eligible.")]
    s_itemremake_scroll_error_invalid_target = 1,
    [Description("The selected items are no longer valid.")]
    s_itemremake_scroll_error_invalid_target_data = 2,
    [Description("The selected item cannot use this scroll.")]
    s_itemremake_scroll_error_invalid_target_stat = 3,
    [Description("The selected item cannot use this scroll.")]
    s_itemremake_scroll_error_impossible_slot = 4,
    [Description("This scroll cannot be used on the selected item due to its quality.")]
    s_itemremake_scroll_error_impossible_rank = 5,
    [Description("The selected item cannot use this scroll due to its level.")]
    s_itemremake_scroll_error_impossible_level = 6,
    [Description("The attributes of the selected item cannot be modified.")]
    s_itemremake_scroll_error_impossible_property = 7,
    [Description("The scroll cannot be used on the selected item.")]
    s_itemremake_scroll_error_impossible_item = 8,
    [Description("The selected items are no longer valid.")]
    s_itemremake_scroll_error_invalid_scroll = 10,
    [Description("The selected items are no longer valid.")]
    s_itemremake_scroll_error_invalid_scroll_data = 11,
    [Description("Failed to modify attributes.")]
    s_itemremake_scroll_error_server_fail_remake = 12, // and 13
    [Description("Failed to apply attributes.")]
    s_itemremake_scroll_error_server_fail_apply_before_option = 14,
    [Description("Failed to use scroll.")]
    s_itemremake_scroll_error_server_fail_consume_scroll = 15,
    [Description("Cannot lock attribute.")]
    s_itemremake_error_server_fail_lack_lock_consume_item = 16,
    [Description("This function has been temporarily restricted.")]
    s_content_shutdown_notice = 17,

    [Description("Bonus Attributes Modification Scroll Error: code=[{0},{1}]")]
    s_itemremake_scroll_error_server_default = byte.MaxValue,
}
