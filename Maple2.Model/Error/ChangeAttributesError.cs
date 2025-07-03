// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum ChangeAttributesError {
    [Description("You do not own this item.")]
    s_itemremake_error_server_not_in_inven = 1,
    [Description("This item's bonus attributes cannot be modified.")]
    s_itemremake_error_server_impossible = 2,
    [Description("Incorrect item.")]
    s_itemremake_error_server_null_status = 3, // and 4
    [Description("Not enough materials.")]
    s_itemremake_error_server_lack_price = 5,
    [Description("The Bonus Attribute change failed.")]
    s_itemremake_error_server_fail_apply_option = 6, // and 7
    [Description("Cannot lock attribute.")]
    s_itemremake_error_server_fail_lack_lock_consume_item = 9,
    [Description("This function has been temporarily restricted.")]
    s_content_shutdown_notice = 10,

    [Description("Bonus Attributes Modification Error: code=[{0},{1}]")]
    s_itemremake_error_server_default = byte.MaxValue,
}
