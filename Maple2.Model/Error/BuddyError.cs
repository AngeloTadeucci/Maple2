// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum BuddyError : byte {
    ok = 0,
    [Description("Character not found.")]
    s_buddy_err_miss_id = 1,
    [Description("You have already sent a friend request to {0}.")]
    s_buddy_err_already_request_somebody = 2,
    [Description("{0} is already your friend.")]
    s_buddy_err_already_buddy = 3,
    [Description("You cannot add yourself.")]
    s_buddy_err_my_id_ex = 4,
    [Description("You cannot send a friend request to {0}.")]
    s_buddy_err_request_somebody = 5,
    [Description("You cannot block {0}.")]
    s_buddy_err_max_block = 6,
    [Description("No friends can be added now.")]
    s_buddy_err_max_buddy = 7,
    [Description("{0} cannot add any friends right now.")]
    s_buddy_err_target_full = 8,
    [Description("{0} has declined your friend request.")]
    s_buddy_refused_request_from_somebody = 9, // and 11

    [Description("System Error: Community")]
    s_buddy_err_unknown = byte.MaxValue,
}
