// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum MapleopolyError : byte {
    [Description("None")]
    ok = 0,
    [Description("Not enough $itemPlural:{0}$.")]
    s_bluemarble_result_consume_fail = 1,
    [Description("You failed to escape.")]
    s_bluemarble_result_trap_escape_fail = 2,
    [Description("You escaped.")]
    s_bluemarble_result_trap_escape_success = 3,
    [Description("You have already rolled the dice.")]
    s_bluemarble_result_dice_complete = 4,
    [Description("You cannot roll the dice right now.")]
    s_bluemarble_result_dice_not_complete = 5,
}
