using System.Numerics;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record ItemMetadata(
    int Id,
    string? Name,
    EquipSlot[] SlotNames,
    string Mesh,
    DefaultHairMetadata[] DefaultHairs,
    ItemMetadataLife Life,
    ItemMetadataProperty Property,
    ItemMetadataCustomize Customize,
    ItemMetadataLimit Limit,
    ItemMetadataSkill? Skill,
    ItemMetadataFunction? Function,
    ItemMetadataAdditionalEffect[] AdditionalEffects,
    ItemMetadataOption? Option,
    ItemMetadataMusic? Music,
    ItemMetadataHousing? Housing,
    ItemMetadataInstall? Install) : ISearchResult;

public record ItemMetadataLife(
    long ExpirationDuration,
    long ExpirationTimestamp);

public record ItemMetadataProperty(
    bool IsSkin,
    int SkinType, // 99 = Template
    int SlotMax,
    int Type,
    int SubType,
    string Category,
    string BlackMarketCategory,
    ItemTag Tag,
    int Group,
    int Collection,
    int GearScore,
    int PetId,
    int Ride,
    int TradableCount,
    int TradableCountDeduction,
    int RepackCount,
    int RepackConsumeCount,
    int[] RepackScrollIds,
    bool DisableDrop,
    int SocketId,
    bool IsFragment,
    int[] SetOptionIds,
    long[] SellPrices,
    long[] CustomSellPrices,
    int ShopId);

public record ItemMetadataCustomize(
    int ColorPalette,
    int DefaultColorIndex);

public record ItemMetadataLimit(
    Gender Gender,
    int Level,
    TransferType TransferType, // [0-7]
    int TradeMaxRarity,
    bool ShopSell,
    bool EnableBreak,
    bool EnableEnchant,
    bool EnableMeretMarket,
    bool EnableSocketTransfer,
    bool RequireVip,
    bool RequireWedding,
    int GlamorForgeCount,
    JobCode[] JobLimits,
    JobCode[] JobRecommends);

public record ItemMetadataSkill(
    int Id,
    short Level,
    int WeaponId,
    short WeaponLevel);

public record ItemMetadataFunction(
    ItemFunction Type,
    string Name,
    string Parameters,
    bool OnlyShadowWorld);

public record ItemMetadataAdditionalEffect(
    int Id,
    short Level,
    bool PickUpEffect);

public record ItemMetadataOption(
    int StaticId,
    ItemOptionMakeType StaticType,
    int RandomId,
    ItemOptionMakeType ItemOptionType,
    int ConstantId,
    ItemOptionMakeType ConstantType,
    int LevelFactor,
    int PickId);

public record ItemMetadataMusic(
    int PlayCount,
    int MasteryValue,
    int MasteryValueMax,
    bool IsCustomNote,
    int NoteLengthMax,
    string FileName,
    int PlayTime);

public record ItemMetadataHousing(
    int TrophyId,
    int TrophyLevel,
    int InteriorLevel,
    HousingCategory HousingCategory,
    bool IsNotAllowedInBlueprint
);

public record ItemMetadataInstall(
    bool IsSolidCube,
    int FunctionId,
    int ObjectCubeId,
    MapAttribute MapAttribute
);

public record DefaultHairMetadata(
    Vector3 BackPosition = default,
    Vector3 BackRotation = default,
    Vector3 FrontPosition = default,
    Vector3 FrontRotation = default,
    float MinScale = 0f,
    float MaxScale = 0f);
