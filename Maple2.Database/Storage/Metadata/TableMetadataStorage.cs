using System;
using Maple2.Database.Context;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class TableMetadataStorage(MetadataContext context) {
    private readonly Lazy<ChatStickerTable> chatStickerTable = Retrieve<ChatStickerTable>(context, "chatemoticon.xml");
    private readonly Lazy<DefaultItemsTable> defaultItemsTable = Retrieve<DefaultItemsTable>(context, "defaultitems.xml");
    private readonly Lazy<ItemBreakTable> itemBreakTable = Retrieve<ItemBreakTable>(context, "itembreakingredient.xml");
    private readonly Lazy<ItemExtractionTable> itemExtractionTable = Retrieve<ItemExtractionTable>(context, "itemextraction.xml");
    private readonly Lazy<GemstoneUpgradeTable> gemstoneUpgradeTable = Retrieve<GemstoneUpgradeTable>(context, "itemgemstoneupgrade.xml");
    private readonly Lazy<JobTable> jobTable = Retrieve<JobTable>(context, "job.xml");
    private readonly Lazy<MagicPathTable> magicPathTable = Retrieve<MagicPathTable>(context, "magicpath.xml");
    private readonly Lazy<MasteryRecipeTable> masteryRecipeTable = Retrieve<MasteryRecipeTable>(context, "masteryreceipe.xml");
    private readonly Lazy<MasteryRewardTable> masteryRewardTable = Retrieve<MasteryRewardTable>(context, "mastery.xml");
    private readonly Lazy<FishTable> fishTable = Retrieve<FishTable>(context, "fish.xml");
    private readonly Lazy<FishingRodTable> fishingRodTable = Retrieve<FishingRodTable>(context, "fishingrod.xml");
    private readonly Lazy<FishingSpotTable> fishingSpotTable = Retrieve<FishingSpotTable>(context, "fishingspot.xml");
    private readonly Lazy<FishingRewardTable> fishingRewardTable = Retrieve<FishingRewardTable>(context, "fishingreward.json");
    private readonly Lazy<InstrumentTable> instrumentTable = Retrieve<InstrumentTable>(context, "instrumentcategoryinfo.xml");
    private readonly Lazy<InteractObjectTable> interactObjectTable = Retrieve<InteractObjectTable>(context, "interactobject*.xml");
    private readonly Lazy<LapenshardUpgradeTable> lapenshardUpgradeTable = Retrieve<LapenshardUpgradeTable>(context, "itemlapenshardupgrade.xml");
    private readonly Lazy<ItemSocketTable> itemSocketTable = Retrieve<ItemSocketTable>(context, "itemsocket.xml");
    private readonly Lazy<GuildTable> guildTable = Retrieve<GuildTable>(context, "guild*.xml");
    private readonly Lazy<PremiumClubTable> premiumClubTable = Retrieve<PremiumClubTable>(context, "vip*.xml");
    private readonly Lazy<IndividualItemDropTable> individualItemDropTable = Retrieve<IndividualItemDropTable>(context, "individualitemdrop*.xml");
    private readonly Lazy<ColorPaletteTable> colorPaletteTable = Retrieve<ColorPaletteTable>(context, "colorpalette.xml");
    private readonly Lazy<MeretMarketCategoryTable> meretMarketCategoryTable = Retrieve<MeretMarketCategoryTable>(context, "meretmarketcategory.xml");
    private readonly Lazy<ShopBeautyCouponTable> shopBeautyCouponTable = Retrieve<ShopBeautyCouponTable>(context, "shop_beautycoupon.xml");
    private readonly Lazy<GachaInfoTable> gachaInfoTable = Retrieve<GachaInfoTable>(context, "gacha_info.xml");
    private readonly Lazy<InsigniaTable> insigniaTable = Retrieve<InsigniaTable>(context, "nametagsymbol.xml");
    private readonly Lazy<ExpTable> expTable = Retrieve<ExpTable>(context, "exp*.xml");
    private readonly Lazy<CommonExpTable> commonExpTable = Retrieve<CommonExpTable>(context, "commonexp.xml");
    private readonly Lazy<UgcDesignTable> ugcDesignTable = Retrieve<UgcDesignTable>(context, "ugcdesign.xml");
    private readonly Lazy<LearningQuestTable> learningQuestTable = Retrieve<LearningQuestTable>(context, "learningquest.xml");

    private readonly Lazy<EnchantScrollTable> enchantScrollTable = Retrieve<EnchantScrollTable>(context, "enchantscroll.xml");
    private readonly Lazy<ItemRemakeScrollTable> itemRemakeScrollTable = Retrieve<ItemRemakeScrollTable>(context, "itemremakescroll.xml");
    private readonly Lazy<ItemRepackingScrollTable> itemRepackingScrollTable = Retrieve<ItemRepackingScrollTable>(context, "itemrepackingscroll.xml");
    private readonly Lazy<ItemSocketScrollTable> itemSocketScrollTable = Retrieve<ItemSocketScrollTable>(context, "itemsocketscroll.xml");
    private readonly Lazy<ItemExchangeScrollTable> itemExchangeScrollTable = Retrieve<ItemExchangeScrollTable>(context, "itemexchangescrolltable.xml");

    private readonly Lazy<ItemOptionConstantTable> itemOptionConstantTable = Retrieve<ItemOptionConstantTable>(context, "itemoptionconstant.xml");
    private readonly Lazy<ItemOptionRandomTable> itemOptionRandomTable = Retrieve<ItemOptionRandomTable>(context, "itemoptionrandom.xml");
    private readonly Lazy<ItemOptionStaticTable> itemOptionStaticTable = Retrieve<ItemOptionStaticTable>(context, "itemoptionstatic.xml");
    private readonly Lazy<ItemOptionPickTable> itemOptionPickTable = Retrieve<ItemOptionPickTable>(context, "itemoptionpick.xml");
    private readonly Lazy<ItemVariationTable> itemVariationTable = Retrieve<ItemVariationTable>(context, "itemoptionvariation.xml");
    private readonly Lazy<ItemEquipVariationTable> accVariationTable = Retrieve<ItemEquipVariationTable>(context, "itemoptionvariation_acc.xml");
    private readonly Lazy<ItemEquipVariationTable> armorVariationTable = Retrieve<ItemEquipVariationTable>(context, "itemoptionvariation_armor.xml");
    private readonly Lazy<ItemEquipVariationTable> petVariationTable = Retrieve<ItemEquipVariationTable>(context, "itemoptionvariation_pet.xml");
    private readonly Lazy<ItemEquipVariationTable> weaponVariationTable = Retrieve<ItemEquipVariationTable>(context, "itemoptionvariation_weapon.xml");

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
