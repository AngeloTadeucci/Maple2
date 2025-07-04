// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Error;

public enum EmoteError : byte {
    [Description("System Error: Emote")]
    s_dynamic_action_item_invalid = 0,
    [Description("You have already learned this emote.")]
    s_dynamic_action_already_learn = 1,
}

public enum BuddyEmoteError : byte {
    [Description("You can't use that buddy emote.")]
    s_couple_emotion_failed_request_not_exist_skill = 0,
    [Description("You must wait until your partner has finished their current emote.")]
    s_couple_emotion_failed_request_already_recv = 1,
    [Description("You must wait until your partner has finished their current emote.")]
    s_couple_emotion_failed_request_already_in_action = 2,
    [Description("{0} is not accepting buddy emote invites.")]
    s_couple_emotion_failed_requset_auto_decline = 3,
    [Description("Your partner has become busy with something else and cannot participate in the buddy emote.")]
    s_couple_emotion_failed_accept_request_user_wrong_state = 4,
    [Description("Your partner is busy with something else and can't participate in a buddy emote right now.")]
    s_couple_emotion_failed_request_wrong_state_target_user = 5,
    [Description("The character who invited you to participate in a buddy emote could not be found.")]
    s_couple_emotion_failed_accept_cannot_find_request_user = 6,
    [Description("{0} can't participate in a buddy emote from their current location.")]
    s_couple_emotion_target_user_wrong_position = 7,
    [Description("Your partner is too far away to participate in a buddy emote.")]
    s_couple_emotion_failed_teleport_limit_distance = 8,
    [Description("You cannot participate in a buddy emote on this map.")]
    s_couple_emotion_cannot_request_in_this_map = 9,

    [Description("Unable to initiate buddy emote.")]
    s_couple_emotion_failed = byte.MaxValue,
}
