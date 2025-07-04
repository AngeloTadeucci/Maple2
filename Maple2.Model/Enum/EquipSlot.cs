// ReSharper disable InconsistentNaming

using System.ComponentModel;

namespace Maple2.Model.Enum;

public enum EquipSlot : sbyte {
    [Description("Skin")]
    SK = 0, // Male Skin/Female Skin
    [Description("Hair")]
    HR = 1,
    [Description("Face")]
    FA = 2,
    [Description("Face Decal")]
    FD = 3,
    [Description("Left Hand")]
    LH = 4,
    [Description("Right Hand")]
    RH = 5,
    [Description("Cap")]
    CP = 6,
    [Description("Mantle")]
    MT = 7,
    [Description("Clothes")]
    CL = 8,
    [Description("Pants")]
    PA = 9,
    [Description("Gloves")]
    GL = 10,
    [Description("Shoes")]
    SH = 11,
    [Description("Face Accessory")]
    FH = 12,
    [Description("Eyewear")]
    EY = 13,
    [Description("Earring")]
    EA = 14,
    [Description("Pendant")]
    PD = 15,
    [Description("Ring")]
    RI = 16,
    [Description("Belt")]
    BE = 17,
    [Description("Ear")]
    ER = 18,
    [Description("Off Hand")]
    OH = 19, // Cannot equip in off-hand (LH/RH)
    [Description("Unknown")]
    Unknown = 20,
}
