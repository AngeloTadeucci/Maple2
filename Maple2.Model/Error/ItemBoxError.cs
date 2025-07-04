// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum ItemBoxError : short {
    ok = 2,
    [Description("Unable to open boxes.")]
    s_err_cannot_open_multi_itembox_inventory_fail = 3,
    [Description("No additional containers can be opened, as your inventory is full. Please make space in your inventory.")]
    s_err_cannot_open_multi_itembox_inventory = 4,
}
