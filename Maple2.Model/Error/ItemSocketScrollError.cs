// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum ItemSocketScrollError {
    none = 0,
    [Description("This item is not eligible.")]
    s_itemsocket_scroll_error_invalid_target = 1,
    [Description("The selected items are no longer valid.")]
    s_itemsocket_scroll_error_invalid_scroll = 2,
    [Description("The selected item cannot use this scroll.")]
    s_itemsocket_scroll_error_invalid_disable = 3,
    [Description("This item cannot have any more sockets.")]
    s_itemsocket_scroll_error_socket_unlock_all = 4,
    [Description("The selected item cannot use this scroll.")]
    s_itemsocket_scroll_error_impossible_slot = 5,
    [Description("This scroll cannot be used on the selected item due to its quality.")]
    s_itemsocket_scroll_error_impossible_rank = 6,
    [Description("The selected item cannot use this scroll due to its level.")]
    s_itemsocket_scroll_error_impossible_level = 7,
    [Description("The selected item cannot use this scroll.")]
    s_itemsocket_scroll_error_impossible_usepart = 8,
    [Description("Failed to use scroll.")]
    s_itemsocket_scroll_error_server_fail_consume_scroll = 9,
    [Description("Socket activation failed.")]
    s_itemsocket_scroll_error_server_fail_unlock_socket = 10,
    [Description("This item already has as many active sockets as the scroll can add.")]
    s_itemsocket_scroll_error_already_socket_unlock = 11,

    [Description("Socket scroll error. Code: {0}, {1}")]
    s_itemsocket_scroll_error_server_default = byte.MaxValue,
}
