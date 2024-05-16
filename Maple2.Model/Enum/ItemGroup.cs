namespace Maple2.Model.Enum;

public enum ItemGroup : byte {
    // CharacterId=>Inventory, AccountId=>Storage
    Default = 0,

    // CharacterId Specific
    Gear = 1,
    Outfit = 2,
    Outfit2 = 3,
    Badge = 4,
    Medal = 5,

    SavedHair = 8,

    // AccountId specific
    Furnishing = 10,
    Home = 11,
    Plot = 12,
}
