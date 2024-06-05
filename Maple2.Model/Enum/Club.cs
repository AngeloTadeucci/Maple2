namespace Maple2.Model.Enum;

public enum ClubState : byte {
    Staged = 1,
    Established = 2,
}

public enum ClubInviteReply {
    Accept = 0,
    Reject = 76,
    Fail = 77,
}
