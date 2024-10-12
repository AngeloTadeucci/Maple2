namespace Maple2.Model.Enum;

public enum MaritalStatus : short {
    Single = 0,
    Engaged = 1,
    Married = 2,
    ConsentualDivorce = 3,
    ForceDivorce = 4,
    DivorceCoolOff = 5,
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

[Flags]
public enum WeddingHallEntryType {
    Guest = 1,
    Bride = 2,
    Groom = 4,
    GroomBride = Groom | Bride,
}

public enum WeddingHallState {
    weddingComplete,

}
