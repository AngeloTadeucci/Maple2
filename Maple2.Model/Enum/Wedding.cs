namespace Maple2.Model.Enum;

public enum MaritalStatus : short {
    Single = 0,
    Engaged = 1,
    Married = 2,
    Divorce = 3,
}

public enum MarriageExpType : short {
    None = 0,
    coupleMessage = 2,
    Online = 4, // ??
    logoutPanelty = 6,
    attendance = 7, // ??

    // TODO: figure out the values for these
    marriedLife,
    weddingExpItem,
}

public enum MarriageExpLimit {
    none,
    day,
    month,
}

public enum ProposalResponse : short {
    Accept = 1,
    Decline = 2,
    Timeout = 6,
}
