namespace Maple2.Model.Enum;

[Flags]
public enum TransferFlag {
    None = 0,
    Split = 2,
    Trade = 4,
    Bind = 8,
    LimitTrade = 16,
}

public enum TransferType {
    Tradable = 0,
    Untradeable = 1,
    BindOnLoot = 2,
    BindOnEquip = 3,
    BindOnUse = 4,
    BindOnTrade = 5,
    BlackMarketOnly = 6,
    BindPet = 7,
}
