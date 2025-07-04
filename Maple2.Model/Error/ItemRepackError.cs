// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum ItemRepackError {
    ok = 0,
    [Description("This item cannot be packaged.")]
    s_item_repacking_scroll_error_invalid_target = 1,
    [Description("This item is no longer valid.")]
    s_item_repacking_scroll_error_invalid_target_data = 2,
    [Description("You cannot package right now.")]
    s_item_repacking_scroll_error_impossible_slot = 3,
    [Description("Packaging not allowed at this rank.")]
    s_item_repacking_scroll_error_impossible_rank = 4,
    [Description("Packaging not allowed at this level.")]
    s_item_repacking_scroll_error_impossible_level = 5,
    [Description("This item is no longer valid.")]
    s_item_repacking_scroll_error_invalid_scroll = 7,
    [Description("Item packaging failed.")]
    s_item_repacking_scroll_error_server_fail_consume_scroll = 12,
}
