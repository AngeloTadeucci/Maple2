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
