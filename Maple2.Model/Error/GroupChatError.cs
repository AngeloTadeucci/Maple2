// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum GroupChatError {
    none = 0,
    [Description("You cannot invite that player to the group chat.")]
    s_err_groupchat_null_target_user = 3,
    [Description("You cannot invite that player to the group chat.")]
    s_err_groupchat_add_member_target = 8,
    [Description("{0} is already participating in 3 group chats.")]
    s_err_groupchat_maxgroup = 10,
    [Description("\"{0}\" contains inappropriate words.\\nPlease enter another name.")]
    s_change_charname_err_bad_words = 13,
}
