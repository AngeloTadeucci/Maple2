namespace Maple2.Model.Enum;

public enum AttendGiftCurrencyType : byte {
    None = 0,
    Meso = 1,
    Meret = 2,
}

public enum AttendGiftRequirement {
    None,
    NotUserValue,
    UserValue,
    ItemId,
}
