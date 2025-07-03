namespace Maple2.Model.Enum;

public enum ShopCurrencyType : byte {
    Meso = 0,
    Item = 1,
    ValorToken = 2,
    Treva = 3,
    Meret = 4,
    Rue = 5,
    HaviFruit = 6,
    GuildCoin = 7,
    ReverseCoin = 8,
    EventMeret = 9,
    GameMeret = 10, // RedMeret
    MentorToken = 11,
    MenteeToken = 12,
    StarPoint = 13,
    MesoToken = 14,
}

public enum ShopFrameType : byte {
    Default = 0,
    Unknown = 1,
    Star = 2,
    StyleCrate = 3,
    Capsule = 4,
}

public enum ShopItemLabel : byte {
    None = 0,
    New = 1,
    Event = 2,
    HalfPrice = 3,
    Special = 4,
}

public enum ShopBuyDay : byte {
    Sunday = 1,
    Monday = 2,
    Tuesday = 3,
    Wednesday = 4,
    Thursday = 5,
    Friday = 6,
    Saturday = 7,
}
