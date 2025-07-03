using Maple2.Database.Context;
using Maple2.Model.Common;
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
    private readonly Lazy<FishingRodTable> fishingRodTable;
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
    private readonly Lazy<FurnishingShopTable> furnishingShopTable;
    private readonly Lazy<GachaInfoTable> gachaInfoTable;
    private readonly Lazy<InsigniaTable> insigniaTable;
    private readonly Lazy<ExpTable> expTable;
    private readonly Lazy<CommonExpTable> commonExpTable;
    private readonly Lazy<UgcDesignTable> ugcDesignTable;
    private readonly Lazy<MasteryUgcHousingTable> masteryUgcHousingTable;
    private readonly Lazy<UgcHousingPointRewardTable> ugcHousingPointRewardTable;
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
    private readonly Lazy<BannerTable> bannerTable;
    private readonly Lazy<WeddingTable> weddingTable;
    private readonly Lazy<RewardContentTable> rewardContentTable;
    private readonly Lazy<SeasonDataTable> seasonDataTable;
    private readonly Lazy<SmartPushTable> smartPushTable;
    private readonly Lazy<AutoActionTable> autoActionTable;

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

    private readonly Lazy<DungeonRoomTable> dungeonRoomTable;
    private readonly Lazy<DungeonRankRewardTable> dungeonRankRewardTable;
    private readonly Lazy<DungeonConfigTable> dungeonConfigTable;
    private readonly Lazy<DungeonMissionTable> dungeonMissionTable;

    public ChatStickerTable ChatStickerTable => chatStickerTable.Value;
    public DefaultItemsTable DefaultItemsTable => defaultItemsTable.Value;
    public ItemBreakTable ItemBreakTable => itemBreakTable.Value;
    public ItemExtractionTable ItemExtractionTable => itemExtractionTable.Value;
    public GemstoneUpgradeTable GemstoneUpgradeTable => gemstoneUpgradeTable.Value;
    public JobTable JobTable => jobTable.Value;
    public MagicPathTable MagicPathTable => magicPathTable.Value;
    public MasteryRecipeTable MasteryRecipeTable => masteryRecipeTable.Value;
    public MasteryRewardTable MasteryRewardTable => masteryRewardTable.Value;
    public FishingRodTable FishingRodTable => fishingRodTable.Value;
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
    public FurnishingShopTable FurnishingShopTable => furnishingShopTable.Value;
    public GachaInfoTable GachaInfoTable => gachaInfoTable.Value;
    public InsigniaTable InsigniaTable => insigniaTable.Value;
    public ExpTable ExpTable => expTable.Value;
    public CommonExpTable CommonExpTable => commonExpTable.Value;
    public UgcDesignTable UgcDesignTable => ugcDesignTable.Value;
    public MasteryUgcHousingTable MasteryUgcHousingTable => masteryUgcHousingTable.Value;
    public UgcHousingPointRewardTable UgcHousingPointRewardTable => ugcHousingPointRewardTable.Value;
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
    public BannerTable BannerTable => bannerTable.Value;
    public WeddingTable WeddingTable => weddingTable.Value;
    public RewardContentTable RewardContentTable => rewardContentTable.Value;
    public SeasonDataTable SeasonDataTable => seasonDataTable.Value;
    public SmartPushTable SmartPushTable => smartPushTable.Value;
    public AutoActionTable AutoActionTable => autoActionTable.Value;

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

    public DungeonRoomTable DungeonRoomTable => dungeonRoomTable.Value;
    public DungeonRankRewardTable DungeonRankRewardTable => dungeonRankRewardTable.Value;
    public DungeonConfigTable DungeonConfigTable => dungeonConfigTable.Value;
    public DungeonMissionTable DungeonMissionTable => dungeonMissionTable.Value;

    public TableMetadataStorage(MetadataContext context) {
        chatStickerTable = Retrieve<ChatStickerTable>(context, TableNames.CHAT_EMOTICON);
        defaultItemsTable = Retrieve<DefaultItemsTable>(context, TableNames.DEFAULT_ITEMS);
        itemBreakTable = Retrieve<ItemBreakTable>(context, TableNames.ITEM_BREAK_INGREDIENT);
        itemExtractionTable = Retrieve<ItemExtractionTable>(context, TableNames.ITEM_EXTRACTION);
        gemstoneUpgradeTable = Retrieve<GemstoneUpgradeTable>(context, TableNames.ITEM_GEMSTONE_UPGRADE);
        jobTable = Retrieve<JobTable>(context, TableNames.JOB);
        magicPathTable = Retrieve<MagicPathTable>(context, TableNames.MAGIC_PATH);
        masteryRecipeTable = Retrieve<MasteryRecipeTable>(context, TableNames.MASTERY_RECIPE);
        masteryRewardTable = Retrieve<MasteryRewardTable>(context, TableNames.MASTERY);
        fishingRodTable = Retrieve<FishingRodTable>(context, TableNames.FISHING_ROD);
        instrumentTable = Retrieve<InstrumentTable>(context, TableNames.INSTRUMENT_CATEGORY_INFO);
        interactObjectTable = Retrieve<InteractObjectTable>(context, TableNames.INTERACT_OBJECT);
        lapenshardUpgradeTable = Retrieve<LapenshardUpgradeTable>(context, TableNames.ITEM_LAPENSHARD_UPGRADE);
        itemSocketTable = Retrieve<ItemSocketTable>(context, TableNames.ITEM_SOCKET);
        guildTable = Retrieve<GuildTable>(context, TableNames.GUILD);
        premiumClubTable = Retrieve<PremiumClubTable>(context, TableNames.VIP);
        individualItemDropTable = Retrieve<IndividualItemDropTable>(context, TableNames.INDIVIDUAL_ITEM_DROP);
        colorPaletteTable = Retrieve<ColorPaletteTable>(context, TableNames.COLOR_PALETTE);
        meretMarketCategoryTable = Retrieve<MeretMarketCategoryTable>(context, TableNames.MERET_MARKET_CATEGORY);
        shopBeautyCouponTable = Retrieve<ShopBeautyCouponTable>(context, TableNames.SHOP_BEAUTY_COUPON);
        furnishingShopTable = Retrieve<FurnishingShopTable>(context, TableNames.SHOP_FURNISHING);
        gachaInfoTable = Retrieve<GachaInfoTable>(context, TableNames.GACHA_INFO);
        insigniaTable = Retrieve<InsigniaTable>(context, TableNames.NAME_TAG_SYMBOL);
        expTable = Retrieve<ExpTable>(context, TableNames.EXP);
        commonExpTable = Retrieve<CommonExpTable>(context, TableNames.COMMON_EXP);
        ugcDesignTable = Retrieve<UgcDesignTable>(context, TableNames.UGC_DESIGN);
        masteryUgcHousingTable = Retrieve<MasteryUgcHousingTable>(context, TableNames.MASTERY_UGC_HOUSING);
        ugcHousingPointRewardTable = Retrieve<UgcHousingPointRewardTable>(context, TableNames.UGC_HOUSING_POINT_REWARD);
        learningQuestTable = Retrieve<LearningQuestTable>(context, TableNames.LEARNING_QUEST);
        prestigeLevelAbilityTable = Retrieve<PrestigeLevelAbilityTable>(context, TableNames.PRESTIGE_LEVEL_ABILITY);
        prestigeLevelRewardTable = Retrieve<PrestigeLevelRewardTable>(context, TableNames.PRESTIGE_LEVEL_REWARD);
        prestigeMissionTable = Retrieve<PrestigeMissionTable>(context, TableNames.PRESTIGE_MISSION);
        blackMarketTable = Retrieve<BlackMarketTable>(context, TableNames.BLACK_MARKET_TABLE);
        changeJobTable = Retrieve<ChangeJobTable>(context, TableNames.CHANGE_JOB);
        chapterBookTable = Retrieve<ChapterBookTable>(context, TableNames.CHAPTER_BOOK);
        fieldMissionTable = Retrieve<FieldMissionTable>(context, TableNames.FIELD_MISSION);
        worldMapTable = Retrieve<WorldMapTable>(context, TableNames.WORLD_MAP);
        survivalSkinInfoTable = Retrieve<SurvivalSkinInfoTable>(context, TableNames.SURVIVAL_SKIN_INFO);
        bannerTable = Retrieve<BannerTable>(context, TableNames.BANNER);
        weddingTable = Retrieve<WeddingTable>(context, TableNames.WEDDING);
        rewardContentTable = Retrieve<RewardContentTable>(context, TableNames.REWARD_CONTENT);
        enchantScrollTable = Retrieve<EnchantScrollTable>(context, TableNames.ENCHANT_SCROLL);
        itemRemakeScrollTable = Retrieve<ItemRemakeScrollTable>(context, TableNames.ITEM_REMAKE_SCROLL);
        itemRepackingScrollTable = Retrieve<ItemRepackingScrollTable>(context, TableNames.ITEM_REPACKING_SCROLL);
        itemSocketScrollTable = Retrieve<ItemSocketScrollTable>(context, TableNames.ITEM_SOCKET_SCROLL);
        itemExchangeScrollTable = Retrieve<ItemExchangeScrollTable>(context, TableNames.ITEM_EXCHANGE_SCROLL);
        itemOptionConstantTable = Retrieve<ItemOptionConstantTable>(context, TableNames.ITEM_OPTION_CONSTANT);
        itemOptionRandomTable = Retrieve<ItemOptionRandomTable>(context, TableNames.ITEM_OPTION_RANDOM);
        itemOptionStaticTable = Retrieve<ItemOptionStaticTable>(context, TableNames.ITEM_OPTION_STATIC);
        itemOptionPickTable = Retrieve<ItemOptionPickTable>(context, TableNames.ITEM_OPTION_PICK);
        itemVariationTable = Retrieve<ItemVariationTable>(context, TableNames.ITEM_OPTION_VARIATION);
        accVariationTable = Retrieve<ItemEquipVariationTable>(context, TableNames.ITEM_OPTION_VARIATION_ACC);
        armorVariationTable = Retrieve<ItemEquipVariationTable>(context, TableNames.ITEM_OPTION_VARIATION_ARMOR);
        petVariationTable = Retrieve<ItemEquipVariationTable>(context, TableNames.ITEM_OPTION_VARIATION_PET);
        weaponVariationTable = Retrieve<ItemEquipVariationTable>(context, TableNames.ITEM_OPTION_VARIATION_WEAPON);
        dungeonRoomTable = Retrieve<DungeonRoomTable>(context, TableNames.DUNGEON_ROOM);
        dungeonRankRewardTable = Retrieve<DungeonRankRewardTable>(context, TableNames.DUNGEON_RANK_REWARD);
        dungeonConfigTable = Retrieve<DungeonConfigTable>(context, TableNames.DUNGEON_CONFIG);
        dungeonMissionTable = Retrieve<DungeonMissionTable>(context, TableNames.DUNGEON_MISSION);
        seasonDataTable = Retrieve<SeasonDataTable>(context, TableNames.SEASON_DATA);
        smartPushTable = Retrieve<SmartPushTable>(context, TableNames.SMART_PUSH);
        autoActionTable = Retrieve<AutoActionTable>(context, TableNames.AUTO_ACTION);

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
