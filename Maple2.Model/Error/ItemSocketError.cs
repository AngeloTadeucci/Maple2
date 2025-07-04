// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum ItemSocketError {
    [Description("The selected target is not in your inventory.")]
    s_itemsocketsystem_error_invalid_target = 1,
    [Description("The selected item is not in your inventory.")]
    s_itemsocketsystem_error_invalid_target_gemstone = 2,
    [Description("This item cannot be used as material.")]
    s_itemsocketsystem_error_invalid_target_ingredient = 3,
    [Description("Confirm the number of items you're using as a catalyst.")]
    s_itemsocketsystem_error_invalid_target_ingredient_count = 4,
    [Description("A socket system error has occurred.\nPlease close the window and try again.\nCode = {0}, {1}")]
    s_itemsocketsystem_error_server_default_msgbox = 5, // 5 to 12
    [Description("This socket is locked.")]
    s_itemsocketsystem_error_socket_lock = 15,
    [Description("This socket already has a gemstone.")]
    s_itemsocketsystem_error_socket_used = 16,
    [Description("This socket is empty.")]
    s_itemsocketsystem_error_socket_empty = 17,
    [Description("Sockets cannot be extended further.")]
    s_itemsocketsystem_error_socket_unlock_all = 18,
    [Description("Cannot be upgraded further.")]
    s_itemsocketsystem_error_gemstone_maxlevel = 20,
    [Description("Not enough materials.")]
    s_itemsocketsystem_error_lack_price = 21,
    // 22 = NOP
    [Description("")]
    s_itemsocketsystem_error_bind_owner = 23,

    [Description("Socket system error. Code: {0}, {1}")]
    s_itemsocketsystem_error_server_default = byte.MaxValue,
}
