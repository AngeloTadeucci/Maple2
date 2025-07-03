// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum BeautyError {
    [Description("Not enough currency.")]
    lack_currency = 2, // The client somehow determines which currency is lacking.
    [Description("This dye can't be used.")]
    s_beauty_msg_error_color = 11,
    [Description("You have reached the maximum number of hairstyle slots.")]
    s_beauty_msg_error_style_slot_extend_max = 12,
    [Description("")]
    none = 17,
    [Description("You have reached the maximum number of hairstyle slots.\nSaved hairstyles can be deleted by the hair designer.")]
    s_beauty_msg_error_style_slot_max = 18,
    [Description("Not enough merets.\nDo you want to buy merets?")]
    s_err_lack_merat_ask = 19,
    [Description("Incorrect character gender.")]
    s_beauty_msg_error_style_disable_gender = 22,
    [Description("This hairstyle is already applied.")]
    s_beauty_msg_error_style_apply_already = 23,
    [Description("During the beauty day event, discounted hair cannot be stored.")]
    s_beauty_msg_error_style_save_cant_not_beauty_day = 24,

    [Description("System Error. Code = {0}\nWill return to the game.")]
    s_beauty_msg_error_code = byte.MaxValue,
}
