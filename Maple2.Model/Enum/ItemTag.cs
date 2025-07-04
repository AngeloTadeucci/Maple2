// ReSharper disable InconsistentNaming, IdentifierTypo

using System.ComponentModel;

namespace Maple2.Model.Enum;

// Please leave these names as-is to match item tags found in xml.
public enum ItemTag {
    None = 0,
    [Description("Channel Chat Voucher")]
    FreeChannelChatCoupon = 1,
    [Description("World Chat Voucher")]
    FreeWorldChatCoupon = 2,
    [Description("")]
    FreeSuperChatCoupon = 3,
    [Description("Revive Voucher")]
    FreeReviveCoupon = 4,
    [Description("Template Voucher")]
    FreeDesignCoupon = 5,
    [Description("Pet Name Change Voucher")]
    FreePetNameChangeCoupon = 6,
    [Description("Weekly Dungeon Ticket, Bonus Dungeon Reward Voucher, Dungeon Ticket")]
    DungeonRewardTicketA = 7,
    [Description("Treasure Dungeon Ticket")]
    DungeonRewardTicketB = 8,
    [Description("Id=20302765")]
    DungeonRewardTicketC = 9,
    [Description("Bonus Dungeon Reward Voucher")]
    DungeonRewardTypeA = 10,
    [Description("Fantastic Bonus Dungeon Reward Voucher")]
    DungeonRewardTypeB = 11,
    [Description("Superior Bonus Dungeon Reward Voucher")]
    DungeonRewardTypeC = 12,
    [Description("")]
    DungeonRewardTypeD = 13,
    [Description("Free Rotors Walkie-talkie")]
    air_taxi = 14,
    [Description("Rotors Walkie-talkie")]
    air_taxi_advanced = 15,
    [Description("Face Change Voucher")]
    beauty_face = 16,
    [Description("Hairstyle Voucher")]
    beauty_hair = 17,
    [Description("Gear Dye Voucher")]
    beauty_itemcolor = 18,
    [Description("Skin Tone Change Voucher")]
    beauty_skin = 19,
    [Description("Cosmetics Voucher")]
    beauty_makeup = 20,
    [Description("Special Hairstyle Voucher")]
    beauty_hair_special = 21,
    [Description("")]
    beauty_hair_special_bonus = 22,
    [Description("Red Bella Figurine Package, Black Bella Figurine Package, Yellow Mika Figurine Package, Blue Mika Figurine Package")]
    Cashshop_Figure = 23,
    [Description("Party Summon Scroll")]
    party_call = 24,
    [Description("Free Expedition Specialty Reset")]
    ShadowPointReset = 25,
    [Description("Lulu's Key")]
    LulluKey = 26,
    [Description("Mystery Box")]
    LulluBox = 27,
    [Description("")]
    LulluKey01 = 28,
    [Description("")]
    LulluBox01 = 29,
    [Description("")]
    LulluKey02 = 30,
    [Description("")]
    LulluBox02 = 31,
    [Description("Luxurious Style Key")]
    LulluKey03 = 32,
    [Description("Holiday Mystery Box")]
    LulluBox03 = 33,
    [Description("Golden Jewel Key")]
    LulluKey04 = 34,
    [Description("Mystery Style Box")]
    LulluBox04 = 35,
    [Description("")]
    LulluKey05 = 36,
    [Description("Mystery Jewel Box")]
    LulluBox05 = 37,
    [Description("")]
    LulluKey06 = 38,
    [Description("")]
    LulluBox06 = 39,
    [Description("")]
    LulluKey07 = 40,
    [Description("")]
    LulluBox07 = 41,
    [Description("")]
    LulluKey08 = 42,
    [Description("Style Crate")]
    LulluBox08 = 43,
    [Description("")]
    LulluKey09 = 44,
    [Description("")]
    LulluBox09 = 45,
    [Description("")]
    LulluKey10 = 46,
    [Description("")]
    LulluBox10 = 47,
    [Description("")]
    LulluKey11 = 48,
    [Description("")]
    LulluBox11 = 49,
    [Description("")]
    LulluKey12 = 50,
    [Description("")]
    LulluBox12 = 51,
    [Description("")]
    LulluKey13 = 52,
    [Description("")]
    LulluBox13 = 53,
    [Description("")]
    LulluKey14 = 54,
    [Description("")]
    LulluBox14 = 55,
    [Description("")]
    LulluKey15 = 56,
    [Description("")]
    LulluBox15 = 57,
    [Description("")]
    LulluKey16 = 58,
    [Description("")]
    LulluBox16 = 59,
    [Description("")]
    LulluKey17 = 60,
    [Description("")]
    LulluBox17 = 61,
    [Description("")]
    LulluKey18 = 62,
    [Description("")]
    LulluBox18 = 63,
    [Description("")]
    LulluKey19 = 64,
    [Description("")]
    LulluBox19 = 65,
    [Description("")]
    LulluKey20 = 66,
    [Description("")]
    LulluBox20 = 67,
    [Description("")]
    LulluKey21 = 68,
    [Description("")]
    LulluBox21 = 69,
    [Description("")]
    LulluKey22 = 70,
    [Description("")]
    LulluBox22 = 71,
    [Description("")]
    LulluKey23 = 72,
    [Description("")]
    LulluBox23 = 73,
    [Description("")]
    LulluKey24 = 74,
    [Description("")]
    LulluBox24 = 75,
    [Description("")]
    LulluKey25 = 76,
    [Description("")]
    LulluBox25 = 77,
    [Description("")]
    LulluKey26 = 78,
    [Description("")]
    LulluBox26 = 79,
    [Description("")]
    LulluKey27 = 80,
    [Description("")]
    LulluBox27 = 81,
    [Description("")]
    LulluKey28 = 82,
    [Description("")]
    LulluBox28 = 83,
    [Description("")]
    LulluKey29 = 84,
    [Description("")]
    LulluBox29 = 85,
    [Description("")]
    LulluKey30 = 86,
    [Description("")]
    LulluBox30 = 87,
    [Description("")]
    PetBox = 88,
    [Description("")]
    DungeonLimitItem = 89,
    [Description("")]
    SkinGemDustA = 90,
    [Description("")]
    SkinGemDustB = 91,
    [Description("Special Outfit Crystal")]
    SkinCrystal = 92,
    [Description("")]
    SkinGemMaterial = 93,
    [Description("")]
    CharmStone = 94,
    [Description("")]
    ProtectStone = 95,
    [Description("Metacell")]
    MetaCell = 96,
    [Description("Red Crystal")]
    RedCrystal = 97,
    [Description("Green Crystal")]
    GreenCrystal = 98,
    [Description("Blue Crystal")]
    BlueCrystal = 99,
    [Description("Crystal Fragment")]
    CrystalPiece = 100,
    [Description("Onyx Crystal")]
    Onix = 101,
    [Description("Chaos Onyx Crystal")]
    ChaosOnix = 102,
    [Description("")]
    CrystalGemstone = 103,
    [Description("")]
    FishingLure = 104,
    [Description("Weapon Attribute Lock Scroll")]
    LockItemOptionWeapon = 105,
    [Description("Armor Attribute Lock Scroll")]
    LockItemOptionArmor = 106,
    [Description("Accessory Attribute Lock Scroll")]
    LockItemOptionAccessory = 107,
    [Description("")]
    LockItemOptionPet = 108,
    [Description("OX Quiz Host Ticket")]
    UGCEventOxOpen = 109,
    [Description("Glamour Anvil")]
    ItemExtraction = 110,
    [Description("Pet Skin Crafting Scroll")]
    PetExtraction = 111,
    [Description("Dungeon Reward Booster")]
    ExpenseRewardTicket = 112,
    [Description("Instant Gathering Voucher")]
    AutoMastery = 113,
    [Description("Blue Gem Dust")]
    DustBlue = 114,
    [Description("Purple Gem Dust")]
    DustPurple = 115,
    [Description("Orange Gem Dust")]
    DustOrange = 116,
    [Description("White Gem Dust")]
    DustWhite = 117,
    [Description("Red Gem Dust")]
    DustRed = 118,
    [Description("Green Gem Dust")]
    DustGreen = 119,
    [Description("Cyan Gem Dust")]
    DustCyan = 120,
    [Description("Yellow Gem Dust")]
    DustYellow = 121,
    [Description("Skill Tab Voucher")]
    SkillBookTreeAddTabCoupon = 122,
    [Description("")]
    EpicCharmStone = 123,
    [Description("")]
    EpicProtectStone = 124,
    [Description("Rainbow Feed, Pumpkin Pie Feed, Ghost Fish")]
    SlimeFood = 125,
    [Description("* Fireworks")]
    Firework = 126,
    [Description("Tier 1 Wisdom Gemstone")]
    GemstoneA01 = 127,
    [Description("Tier 2 Wisdom Gemstone")]
    GemstoneA02 = 128,
    [Description("Tier 3 Wisdom Gemstone")]
    GemstoneA03 = 129,
    [Description("Tier 4 Wisdom Gemstone")]
    GemstoneA04 = 130,
    [Description("Tier 5 Wisdom Gemstone")]
    GemstoneA05 = 131,
    [Description("Tier 6 Wisdom Gemstone")]
    GemstoneA06 = 132,
    [Description("Tier 7 Wisdom Gemstone")]
    GemstoneA07 = 133,
    [Description("Tier 8 Wisdom Gemstone")]
    GemstoneA08 = 134,
    [Description("Tier 9 Wisdom Gemstone")]
    GemstoneA09 = 135,
    [Description("Tier 10 Wisdom Gemstone")]
    GemstoneA10 = 136,
    [Description("Tier 1 Luck Gemstone")]
    GemstoneB01 = 137,
    [Description("Tier 2 Luck Gemstone")]
    GemstoneB02 = 138,
    [Description("Tier 3 Luck Gemstone")]
    GemstoneB03 = 139,
    [Description("Tier 4 Luck Gemstone")]
    GemstoneB04 = 140,
    [Description("Tier 5 Luck Gemstone")]
    GemstoneB05 = 141,
    [Description("Tier 6 Luck Gemstone")]
    GemstoneB06 = 142,
    [Description("Tier 7 Luck Gemstone")]
    GemstoneB07 = 143,
    [Description("Tier 8 Luck Gemstone")]
    GemstoneB08 = 144,
    [Description("Tier 9 Luck Gemstone")]
    GemstoneB09 = 145,
    [Description("Tier 10 Luck Gemstone")]
    GemstoneB10 = 146,
    [Description("Tier 1 Destruction Gemstone")]
    GemstoneC01 = 147,
    [Description("Tier 2 Destruction Gemstone ")]
    GemstoneC02 = 148,
    [Description("Tier 3 Destruction Gemstone")]
    GemstoneC03 = 149,
    [Description("Tier 4 Destruction Gemstone")]
    GemstoneC04 = 150,
    [Description("Tier 5 Destruction Gemstone")]
    GemstoneC05 = 151,
    [Description("Tier 6 Destruction Gemstone")]
    GemstoneC06 = 152,
    [Description("Tier 7 Destruction Gemstone")]
    GemstoneC07 = 153,
    [Description("Tier 8 Destruction Gemstone")]
    GemstoneC08 = 154,
    [Description("Tier 9 Destruction Gemstone")]
    GemstoneC09 = 155,
    [Description("Tier 10 Destruction Gemstone")]
    GemstoneC10 = 156,
    [Description("Tier 1 Life Gemstone")]
    GemstoneD01 = 157,
    [Description("Tier 2 Life Gemstone ")]
    GemstoneD02 = 158,
    [Description("Tier 3 Life Gemstone")]
    GemstoneD03 = 159,
    [Description("Tier 4 Life Gemstone")]
    GemstoneD04 = 160,
    [Description("Tier 5 Life Gemstone")]
    GemstoneD05 = 161,
    [Description("Tier 6 Life Gemstone")]
    GemstoneD06 = 162,
    [Description("Tier 7 Life Gemstone")]
    GemstoneD07 = 163,
    [Description("Tier 8 Life Gemstone")]
    GemstoneD08 = 164,
    [Description("Tier 9 Life Gemstone")]
    GemstoneD09 = 165,
    [Description("Tier 10 Life Gemstone")]
    GemstoneD10 = 166,
    [Description("Tier 1 Power Gemstone")]
    GemstoneE01 = 167,
    [Description("Tier 2 Power Gemstone")]
    GemstoneE02 = 168,
    [Description("Tier 3 Power Gemstone")]
    GemstoneE03 = 169,
    [Description("Tier 4 Power Gemstone")]
    GemstoneE04 = 170,
    [Description("Tier 5 Power Gemstone")]
    GemstoneE05 = 171,
    [Description("Tier 6 Power Gemstone")]
    GemstoneE06 = 172,
    [Description("Tier 7 Power Gemstone")]
    GemstoneE07 = 173,
    [Description("Tier 8 Power Gemstone")]
    GemstoneE08 = 174,
    [Description("Tier 9 Power Gemstone")]
    GemstoneE09 = 175,
    [Description("Tier 10 Power Gemstone")]
    GemstoneE10 = 176,
    [Description("Tier 1 Dex Gemstone")]
    GemstoneF01 = 177,
    [Description("Tier 2 Dex Gemstone")]
    GemstoneF02 = 178,
    [Description("Tier 3 Dex Gemstone")]
    GemstoneF03 = 179,
    [Description("Tier 4 Dex Gemstone")]
    GemstoneF04 = 180,
    [Description("Tier 5 Dex Gemstone")]
    GemstoneF05 = 181,
    [Description("Tier 6 Dex Gemstone")]
    GemstoneF06 = 182,
    [Description("Tier 7 Dex Gemstone")]
    GemstoneF07 = 183,
    [Description("Tier 8 Dex Gemstone")]
    GemstoneF08 = 184,
    [Description("Tier 9 Dex Gemstone")]
    GemstoneF09 = 185,
    [Description("Tier 10 Dex Gemstone")]
    GemstoneF10 = 186,
    [Description("Tier 1 Accuracy Gemstone")]
    GemstoneG01 = 187,
    [Description("Tier 2 Accuracy Gemstone")]
    GemstoneG02 = 188,
    [Description("Tier 3 Accuracy Gemstone")]
    GemstoneG03 = 189,
    [Description("Tier 4 Accuracy Gemstone")]
    GemstoneG04 = 190,
    [Description("Tier 5 Accuracy Gemstone")]
    GemstoneG05 = 191,
    [Description("Tier 6 Accuracy Gemstone")]
    GemstoneG06 = 192,
    [Description("Tier 7 Accuracy Gemstone")]
    GemstoneG07 = 193,
    [Description("Tier 8 Accuracy Gemstone")]
    GemstoneG08 = 194,
    [Description("Tier 9 Accuracy Gemstone")]
    GemstoneG09 = 195,
    [Description("Tier 10 Accuracy Gemstone")]
    GemstoneG10 = 196,
    [Description("Tier 1 Offense Gemstone")]
    GemstoneH01 = 197,
    [Description("Tier 2 Offense Gemstone")]
    GemstoneH02 = 198,
    [Description("Tier 3 Offense Gemstone")]
    GemstoneH03 = 199,
    [Description("Tier 4 Offense Gemstone")]
    GemstoneH04 = 200,
    [Description("Tier 5 Offense Gemstone")]
    GemstoneH05 = 201,
    [Description("Tier 6 Offense Gemstone")]
    GemstoneH06 = 202,
    [Description("Tier 7 Offense Gemstone")]
    GemstoneH07 = 203,
    [Description("Tier 8 Offense Gemstone")]
    GemstoneH08 = 204,
    [Description("Tier 9 Offense Gemstone")]
    GemstoneH09 = 205,
    [Description("Tier 10 Offense Gemstone")]
    GemstoneH10 = 206,
    [Description("Toad's Toolkit")]
    EnchantJockerItemNormal = 207,
    [Description("Toad's Toolkit")]
    EnchantJockerItemRare = 208,
    [Description("Toad's Toolkit")]
    EnchantJockerItemElite = 209,
    [Description("Toad's Toolkit")]
    EnchantJockerItemExcellent = 210,
    [Description("Toad's Toolkit")]
    EnchantJockerItemLegendary = 211,
    [Description("Toad's Toolkit")]
    EnchantJockerItemEpic = 212,
    [Description("Daily Mission Insta-Completion Voucher")]
    FameCompletionTicket = 213,
    [Description("")]
    PrismShard = 214,
    [Description("")]
    PrismStone = 215,
    [Description("")]
    WeddingHallCoupon_Grade1 = 216,
    [Description("")]
    WeddingHallCoupon_Grade2 = 217,
    [Description("")]
    WeddingHallCoupon_Grade3 = 218,

    // Not defined in client mapping.
    PetEXP = 250,
}
