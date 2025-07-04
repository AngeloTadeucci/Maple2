// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum MyInfoError {
    none = 0,
    [Description("Contains a forbidden word ({0}).")]
    s_ban_check_err_any_word = 1,

    custom_message = byte.MaxValue,
}
