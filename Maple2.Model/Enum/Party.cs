using System.ComponentModel;

namespace Maple2.Model.Enum;

public enum PartyInviteResponse : byte {
    Accept = 1,
    RejectInvite = 9,
    RejectTimeout = 12,
}

public enum PartyVoteType : byte {
    Kick = 1,
    ReadyCheck = 2,
}

public enum PartyMessage {
    [Description("The voting period has ended.")]
    s_party_vote_expired,
    [Description("The party leader has reset the dungeon.")]
    s_field_enteracne_party_notify_reset_dungeon,
    [Description("The vote to kick failed.")]
    s_party_vote_rejected_kick_user,
}
