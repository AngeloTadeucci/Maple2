// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum CharacterCreateError : byte {
    [Description("Enter at least 2 letters.")]
    s_char_err_name = 1,
    [Description("This name cannot be used.")]
    s_char_err_ban_all = 8,
    [Description("Contains a forbidden word ({0}).")]
    s_char_err_ban_any = 9,
    [Description("You can make up to {0} characters.")]
    s_char_err_char_count = 6,
    [Description("You can make up to {0} characters.")]
    s_char_err_char_count_by_gameevent = 6,
    [Description("Incorrect gear.")]
    s_char_err_invalid_def_item = 10,
    [Description("This name is already being used.")]
    s_char_err_already_taken = 11,
    [Description("The character cannot be created because of a job restriction.")]
    s_char_err_job_forbidden = 12,
    [Description("Abnormal activity detected. Character creation will be limited for a period of time.")]
    s_char_err_creation_restriction = 14,

    [Description("The character cannot be created because of a system error.")]
    s_char_err_system = byte.MaxValue,
}
