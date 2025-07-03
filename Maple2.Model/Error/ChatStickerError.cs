// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum ChatStickerError : byte {
    [Description("Failed to add stickers.")]
    s_msg_chat_emoticon_add_failed = 1,
    [Description("You can't add these stickers for one of the following reasons: - pack is expired - pack has shorter duration than what you have - you have a permanent version of the sticker pack.")]
    s_msg_chat_emoticon_add_failed_already_exist = 2,
}
