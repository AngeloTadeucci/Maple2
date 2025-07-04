namespace Maple2.Model.Game;

public readonly record struct ItemType(byte Group, byte Type) {
    public ItemType(int id) : this((byte) (id / 10000000), (byte) ((id % 10000000) / 100000)) { }

    public bool IsSkin => Group is 0 && Type is 0;
    public bool IsHair => Group is 1 && Type is 2;
    public bool IsFace => Group is 1 && Type is 3;
    public bool IsDecal => Group is 1 && Type is 4;
    public bool IsEar => Group is 1 && Type is 5;

    public bool IsAccessory => Group is 1 && Type is 12 or >= 18 and <= 21;
    public bool IsEarring => Group is 1 && Type is 12;
    public bool IsCape => Group is 1 && Type is 18;
    public bool IsNecklace => Group is 1 && Type is 19;
    public bool IsRing => Group is 1 && Type is 20;
    public bool IsBelt => Group is 1 && Type is 21;

    public bool IsArmor => Group is 1 && Type is >= 13 and <= 17 or 22;
    public bool IsHat => Group is 1 && Type is 13;
    public bool IsClothes => Group is 1 && Type is 14;
    public bool IsPants => Group is 1 && Type is 15;
    public bool IsGloves => Group is 1 && Type is 16;
    public bool IsShoes => Group is 1 && Type is 17;
    public bool IsOverall => Group is 1 && Type is 22;

    public bool IsWeapon => Group is 1 && Type is >= 30 and <= 39 or >= 40 and <= 49 or >= 50 and <= 59;
    public bool IsOneHandWeapon => Group is 1 && Type is >= 30 and <= 39;
    public bool IsOffHandWeapon => Group is 1 && Type is >= 40 and <= 49;
    public bool IsTwoHandWeapon => Group is 1 && Type is >= 50 and <= 59;
    public bool IsBludgeon => Group is 1 && Type is 30;
    public bool IsDagger => Group is 1 && Type is 31;
    public bool IsLongsword => Group is 1 && Type is 32;
    public bool IsScepter => Group is 1 && Type is 33;
    public bool IsThrowingStar => Group is 1 && Type is 34;
    public bool IsSpellbook => Group is 1 && Type is 40;
    public bool IsShield => Group is 1 && Type is 41;
    public bool IsGreatsword => Group is 1 && Type is 50;
    public bool IsBow => Group is 1 && Type is 51;
    public bool IsStaff => Group is 1 && Type is 52;
    public bool IsCannon => Group is 1 && Type is 53;
    public bool IsBlade => Group is 1 && Type is 54;
    public bool IsKnuckle => Group is 1 && Type is 55;
    public bool IsOrb => Group is 1 && Type is 56;

    public bool IsObjectWeapon => Group is 1 && Type is >= 80 and <= 89;
    public bool IsFishingDummy => Group is 1 && Type is 90;
    public bool IsInstrumentDummy => Group is 1 && Type is 91;

    public bool IsConsumable => Group is 2 && Type is 0;
    public bool IsEmote => Group is 2 && Type is 2;
    public bool IsItemPack => Group is 2 && Type is 3;
    public bool IsCompanionCookie => Group is 2 && Type is 4;
    public bool IsBeautyVoucher => Group is 2 && Type is 5;
    public bool IsAdBalloon => Group is 2 && Type is 6;
    public bool IsBuddyBadgeChest => Group is 2 && Type is 7;
    public bool IsSuperChatTheme => Group is 2 && Type is 8;
    public bool IsMedal => Group is 2 && Type is 9;
    public bool IsStickerPack => Group is 2 && Type is 11;
    public bool IsOutfitCapsule => Group is 2 && Type is 20;
    public bool IsOutfitCoin => Group is 2 && Type is 22;
    public bool IsExchangeTranscendenceCrystal => Group is 2 && Type is 30;

    public bool IsMisc => Group is 3 && Type is 0;
    public bool IsScroll => Group is 3 && Type is 10;
    public bool IsFishingRod => Group is 3 && Type is 20;
    public bool IsMastery => Group is 3 && Type is 30;
    public bool IsInstrument => Group is 3 && Type is 40;
    public bool IsMusicScore => Group is 3 && Type is 50 or 51;
    public bool IsPresetMusicScore => Group is 3 && Type is 51;
    public bool IsCustomMusicScore => Group is 3 && Type is 51;
    public bool IsBlueprint => Group is 3 && Type is 52;
    public bool IsFragment => Group is 3 && Type is 60;
    public bool IsTranscendenceCrystal => Group is 3 && Type is 70;
    public bool IsBook => Group is 3 && Type is 90;

    public bool IsGemstone => Group is 4 && Type is 2;
    public bool IsGemDust => Group is 4 && Type is 3;
    public bool IsAirMount => Group is 4 && Type is 4;
    public bool IsLapenshard => Group is 4 && Type is 10 or 20 or 30;
    public bool IsRedLapenshard => Group is 4 && Type is 10;
    public bool IsBlueLapenshard => Group is 4 && Type is 20;
    public bool IsGreenLapenshard => Group is 4 && Type is 30;

    public bool IsFurnishing => Group is 5 && Type is >= 1 and <= 4 or 7 or 8 or 9;
    public bool IsFloorFurnishing => Group is 5 && Type is 1;
    public bool IsDisplayFurnishing => Group is 5 && Type is 2;
    public bool IsSkillFurnishing => Group is 5 && Type is 3;
    public bool IsMonsterBox => Group is 5 && Type is 5;
    public bool IsGroundMount => Group is 5 && Type is 6;
    public bool IsSouvenir => Group is 5 && Type is 7;
    public bool IsMaid => Group is 5 && Type is 8;
    public bool IsHousePackage => Group is 5 && Type is 90;
    public bool IsRoomPackage => Group is 5 && Type is 91;
    public bool IsOutfitPackage => Group is 5 && Type is 92;
    public bool IsFurnishingSet => Group is 5 && Type is 93;
    public bool IsPetCapsule => Group is 5 && Type is 94;
    public bool IsPetFood => Group is 5 && Type is 95;

    public bool IsPet => Group is 6 && Type is 0 or >= 10 and <= 29;
    public bool IsStoragePet => Group is 6 && Type is 0;
    public bool IsCombatPet => Group is 6 && Type is >= 10 and <= 29;
    public bool IsPetCandy => Group is 6 && Type is 30;
    public bool IsPetTrap => Group is 6 && Type is 31;

    public bool IsBadge => Group is 7;
    public bool IsPetSkin => Group is 7 && Type is 1;
    public bool IsChatBubble => Group is 7 && Type is 2;
    public bool IsNameTag => Group is 7 && Type is 3;
    public bool IsDamageSkin => Group is 7 && Type is 4;
    public bool IsTombstone => Group is 7 && Type is 5;
    public bool IsSwimTube => Group is 7 && Type is 6;
    public bool IsFishingBadge => Group is 7 && Type is 7;
    public bool IsBuddyBadge => Group is 7 && Type is 8;
    public bool IsEffectBadge => Group is 7 && Type is 9;
}
