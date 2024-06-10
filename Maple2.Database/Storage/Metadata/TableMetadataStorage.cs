using System;
using Maple2.Database.Context;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class TableMetadataStorage {
    private readonly Lazy<ChatStickerTable> chatStickerTable;
    private readonly Lazy<DefaultItemsTable> defaultItemsTable;
    private readonly Lazy<ItemBreakTable> itemBreakTable;
    private readonly Lazy<ItemExtractionTable> itemExtractionTable;
    private readonly Lazy<GemstoneUpgradeTable> gemstoneUpgradeTable;
    private readonly Lazy<JobTable> jobTable;
    private readonly Lazy<MagicPathTable> magicPathTable;
    private readonly Lazy<MasteryRecipeTable> masteryRecipeTable;
    private readonly Lazy<MasteryRewardTable> masteryRewardTable;
    private readonly Lazy<FishTable> fishTable;
    private readonly Lazy<FishingRodTable> fishingRodTable;
    private readonly Lazy<FishingSpotTable> fishingSpotTable;
    private readonly Lazy<FishingRewardTable> fishingRewardTable;
    private readonly Lazy<InstrumentTable> instrumentTable;
    private readonly Lazy<InteractObjectTable> interactObjectTable;
    private readonly Lazy<LapenshardUpgradeTable> lapenshardUpgradeTable;
    private readonly Lazy<ItemSocketTable> itemSocketTable;
    private readonly Lazy<GuildTable> guildTable;
    private readonly Lazy<PremiumClubTable> premiumClubTable;
    private readonly Lazy<IndividualItemDropTable> individualItemDropTable;
    private readonly Lazy<ColorPaletteTable> colorPaletteTable;
    private readonly Lazy<MeretMarketCategoryTable> meretMarketCategoryTable;
    private readonly Lazy<ShopBeautyCouponTable> shopBeautyCouponTable;
    private readonly Lazy<GachaInfoTable> gachaInfoTable;
    private readonly Lazy<InsigniaTable> insigniaTable;
    private readonly Lazy<ExpTable> expTable;
    private readonly Lazy<CommonExpTable> commonExpTable;
    private readonly Lazy<UgcDesignTable> ugcDesignTable;
    private readonly Lazy<LearningQuestTable> learningQuestTable;
    private readonly Lazy<PrestigeLevelAbilityTable> prestigeLevelAbilityTable;
    private readonly Lazy<PrestigeLevelRewardTable> prestigeLevelRewardTable;
    private readonly Lazy<PrestigeMissionTable> prestigeMissionTable;
    private readonly Lazy<BlackMarketTable> blackMarketTable;
    private readonly Lazy<ChangeJobTable> changeJobTable;
    private readonly Lazy<ChapterBookTable> chapterBookTable;
    private readonly Lazy<FieldMissionTable> fieldMissionTable;
    private readonly Lazy<WorldMapTable> worldMapTable;
    private readonly Lazy<SurvivalSkinInfoTable> survivalSkinInfoTable;

    private readonly Lazy<EnchantScrollTable> enchantScrollTable;
    private readonly Lazy<ItemRemakeScrollTable> itemRemakeScrollTable;
    private readonly Lazy<ItemRepackingScrollTable> itemRepackingScrollTable;
    private readonly Lazy<ItemSocketScrollTable> itemSocketScrollTable;
    private readonly Lazy<ItemExchangeScrollTable> itemExchangeScrollTable;

    private readonly Lazy<ItemOptionConstantTable> itemOptionConstantTable;
    private readonly Lazy<ItemOptionRandomTable> itemOptionRandomTable;
    private readonly Lazy<ItemOptionStaticTable> itemOptionStaticTable;
    private readonly Lazy<ItemOptionPickTable> itemOptionPickTable;
    private readonly Lazy<ItemVariationTable> itemVariationTable;
    private readonly Lazy<ItemEquipVariationTable> accVariationTable;
    private readonly Lazy<ItemEquipVariationTable> armorVariationTable;
    private readonly Lazy<ItemEquipVariationTable> petVariationTable;
    private readonly Lazy<ItemEquipVariationTable> weaponVariationTable;

    public ChatStickerTable ChatStickerTable => chatStickerTable.Value;
    public DefaultItemsTable DefaultItemsTable => defaultItemsTable.Value;
    public ItemBreakTable ItemBreakTable => itemBreakTable.Value;
    public ItemExtractionTable ItemExtractionTable => itemExtractionTable.Value;
    public GemstoneUpgradeTable GemstoneUpgradeTable => gemstoneUpgradeTable.Value;
    public JobTable JobTable => jobTable.Value;
    public MagicPathTable MagicPathTable => magicPathTable.Value;
    public MasteryRecipeTable MasteryRecipeTable => masteryRecipeTable.Value;
    public MasteryRewardTable MasteryRewardTable => masteryRewardTable.Value;
    public FishTable FishTable => fishTable.Value;
    public FishingRodTable FishingRodTable => fishingRodTable.Value;
    public FishingSpotTable FishingSpotTable => fishingSpotTable.Value;
    public FishingRewardTable FishingRewardTable => fishingRewardTable.Value;
    public InstrumentTable InstrumentTable => instrumentTable.Value;
    public InteractObjectTable InteractObjectTable => interactObjectTable.Value;
    public LapenshardUpgradeTable LapenshardUpgradeTable => lapenshardUpgradeTable.Value;
    public ItemSocketTable ItemSocketTable => itemSocketTable.Value;
    public GuildTable GuildTable => guildTable.Value;
    public PremiumClubTable PremiumClubTable => premiumClubTable.Value;
    public IndividualItemDropTable IndividualItemDropTable => individualItemDropTable.Value;
    public ColorPaletteTable ColorPaletteTable => colorPaletteTable.Value;
    public MeretMarketCategoryTable MeretMarketCategoryTable => meretMarketCategoryTable.Value;
    public ShopBeautyCouponTable ShopBeautyCouponTable => shopBeautyCouponTable.Value;
    public GachaInfoTable GachaInfoTable => gachaInfoTable.Value;
    public InsigniaTable InsigniaTable => insigniaTable.Value;
    public ExpTable ExpTable => expTable.Value;
    public CommonExpTable CommonExpTable => commonExpTable.Value;
    public UgcDesignTable UgcDesignTable => ugcDesignTable.Value;
    public LearningQuestTable LearningQuestTable => learningQuestTable.Value;
    public PrestigeLevelAbilityTable PrestigeLevelAbilityTable => prestigeLevelAbilityTable.Value;
    public PrestigeLevelRewardTable PrestigeLevelRewardTable => prestigeLevelRewardTable.Value;
    public PrestigeMissionTable PrestigeMissionTable => prestigeMissionTable.Value;
    public BlackMarketTable BlackMarketTable => blackMarketTable.Value;
    public ChangeJobTable ChangeJobTable => changeJobTable.Value;
    public ChapterBookTable ChapterBookTable => chapterBookTable.Value;
    public FieldMissionTable FieldMissionTable => fieldMissionTable.Value;
    public WorldMapTable WorldMapTable => worldMapTable.Value;
    public SurvivalSkinInfoTable SurvivalSkinInfoTable => survivalSkinInfoTable.Value;

    public EnchantScrollTable EnchantScrollTable => enchantScrollTable.Value;
    public ItemRemakeScrollTable ItemRemakeScrollTable => itemRemakeScrollTable.Value;
    public ItemRepackingScrollTable ItemRepackingScrollTable => itemRepackingScrollTable.Value;
    public ItemSocketScrollTable ItemSocketScrollTable => itemSocketScrollTable.Value;
    public ItemExchangeScrollTable ItemExchangeScrollTable => itemExchangeScrollTable.Value;

    public ItemOptionConstantTable ItemOptionConstantTable => itemOptionConstantTable.Value;
    public ItemOptionRandomTable ItemOptionRandomTable => itemOptionRandomTable.Value;
    public ItemOptionStaticTable ItemOptionStaticTable => itemOptionStaticTable.Value;
    public ItemOptionPickTable ItemOptionPickTable => itemOptionPickTable.Value;
    public ItemVariationTable ItemVariationTable => itemVariationTable.Value;
    public ItemEquipVariationTable AccessoryVariationTable => accVariationTable.Value;
    public ItemEquipVariationTable ArmorVariationTable => armorVariationTable.Value;
    public ItemEquipVariationTable PetVariationTable => petVariationTable.Value;
    public ItemEquipVariationTable WeaponVariationTable => weaponVariationTable.Value;

    public TableMetadataStorage(MetadataContext context) {
        chatStickerTable = Retrieve<ChatStickerTable>(context, "chatemoticon.xml");
        defaultItemsTable = Retrieve<DefaultItemsTable>(context, "defaultitems.xml");
        itemBreakTable = Retrieve<ItemBreakTable>(context, "itembreakingredient.xml");
        itemExtractionTable = Retrieve<ItemExtractionTable>(context, "itemextraction.xml");
        gemstoneUpgradeTable = Retrieve<GemstoneUpgradeTable>(context, "itemgemstoneupgrade.xml");
        jobTable = Retrieve<JobTable>(context, "job.xml");
        magicPathTable = Retrieve<MagicPathTable>(context, "magicpath.xml");
        masteryRecipeTable = Retrieve<MasteryRecipeTable>(context, "masteryreceipe.xml");
        masteryRewardTable = Retrieve<MasteryRewardTable>(context, "mastery.xml");
        fishTable = Retrieve<FishTable>(context, "fish.xml");
        fishingRodTable = Retrieve<FishingRodTable>(context, "fishingrod.xml");
        fishingSpotTable = Retrieve<FishingSpotTable>(context, "fishingspot.xml");
        fishingRewardTable = Retrieve<FishingRewardTable>(context, "fishingreward.json");
        instrumentTable = Retrieve<InstrumentTable>(context, "instrumentcategoryinfo.xml");
        interactObjectTable = Retrieve<InteractObjectTable>(context, "interactobject*.xml");
        lapenshardUpgradeTable = Retrieve<LapenshardUpgradeTable>(context, "itemlapenshardupgrade.xml");
        itemSocketTable = Retrieve<ItemSocketTable>(context, "itemsocket.xml");
        guildTable = Retrieve<GuildTable>(context, "guild*.xml");
        premiumClubTable = Retrieve<PremiumClubTable>(context, "vip*.xml");
        individualItemDropTable = Retrieve<IndividualItemDropTable>(context, "individualitemdrop*.xml");
        colorPaletteTable = Retrieve<ColorPaletteTable>(context, "colorpalette.xml");
        meretMarketCategoryTable = Retrieve<MeretMarketCategoryTable>(context, "meretmarketcategory.xml");
        shopBeautyCouponTable = Retrieve<ShopBeautyCouponTable>(context, "shop_beautycoupon.xml");
        gachaInfoTable = Retrieve<GachaInfoTable>(context, "gacha_info.xml");
        insigniaTable = Retrieve<InsigniaTable>(context, "nametagsymbol.xml");
        expTable = Retrieve<ExpTable>(context, "exp*.xml");
        commonExpTable = Retrieve<CommonExpTable>(context, "commonexp.xml");
        ugcDesignTable = Retrieve<UgcDesignTable>(context, "ugcdesign.xml");
        learningQuestTable = Retrieve<LearningQuestTable>(context, "learningquest.xml");
        prestigeLevelAbilityTable = Retrieve<PrestigeLevelAbilityTable>(context, "adventurelevelability.xml");
        prestigeLevelRewardTable = Retrieve<PrestigeLevelRewardTable>(context, "adventurelevelreward.xml");
        prestigeMissionTable = Retrieve<PrestigeMissionTable>(context, "adventurelevelmission.xml");
        blackMarketTable = Retrieve<BlackMarketTable>(context, "blackmarkettable.xml");
        changeJobTable = Retrieve<ChangeJobTable>(context, "changejob.xml");
        chapterBookTable = Retrieve<ChapterBookTable>(context, "chapterbook.xml");
        fieldMissionTable = Retrieve<FieldMissionTable>(context, "fieldmission.xml");
        worldMapTable = Retrieve<WorldMapTable>(context, "newworldmap.xml");
        survivalSkinInfoTable = Retrieve<SurvivalSkinInfoTable>(context, "maplesurvivalskininfo.xml");
        enchantScrollTable = Retrieve<EnchantScrollTable>(context, "enchantscroll.xml");
        itemRemakeScrollTable = Retrieve<ItemRemakeScrollTable>(context, "itemremakescroll.xml");
        itemRepackingScrollTable = Retrieve<ItemRepackingScrollTable>(context, "itemrepackingscroll.xml");
        itemSocketScrollTable = Retrieve<ItemSocketScrollTable>(context, "itemsocketscroll.xml");
        itemExchangeScrollTable = Retrieve<ItemExchangeScrollTable>(context, "itemexchangescrolltable.xml");
        itemOptionConstantTable = Retrieve<ItemOptionConstantTable>(context, "itemoptionconstant.xml");
        itemOptionRandomTable = Retrieve<ItemOptionRandomTable>(context, "itemoptionrandom.xml");
        itemOptionStaticTable = Retrieve<ItemOptionStaticTable>(context, "itemoptionstatic.xml");
        itemOptionPickTable = Retrieve<ItemOptionPickTable>(context, "itemoptionpick.xml");
        itemVariationTable = Retrieve<ItemVariationTable>(context, "itemoptionvariation.xml");
        accVariationTable = Retrieve<ItemEquipVariationTable>(context, "itemoptionvariation_acc.xml");
        armorVariationTable = Retrieve<ItemEquipVariationTable>(context, "itemoptionvariation_armor.xml");
        petVariationTable = Retrieve<ItemEquipVariationTable>(context, "itemoptionvariation_pet.xml");
        weaponVariationTable = Retrieve<ItemEquipVariationTable>(context, "itemoptionvariation_weapon.xml");
    }

    private static Lazy<T> Retrieve<T>(MetadataContext context, string key) where T : Table {
        var result = new Lazy<T>(() => {
            lock (context) {
                TableMetadata? row = context.TableMetadata.Find(key);
                if (row?.Table is not T result) {
                    throw new InvalidOperationException($"Row does not exist: {key}");
                }

                return result;
            }
        });

#if !DEBUG
        // No lazy loading for RELEASE build.
        _ = result.Value;
#endif
        return result;
    }
}
