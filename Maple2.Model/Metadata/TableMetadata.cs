using System;
using System.Text.Json.Serialization;

namespace Maple2.Model.Metadata;

public class TableMetadata {
    public required string Name { get; set; }
    public required Table Table { get; set; }

    protected bool Equals(TableMetadata other) {
        return Name == other.Name && Table.Equals(other.Table);
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;

        return Equals((TableMetadata) obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Name, Table);
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "!")]
[JsonDerivedType(typeof(JobTable), typeDiscriminator: "job")]
[JsonDerivedType(typeof(ItemBreakTable), typeDiscriminator: "itembreak")]
[JsonDerivedType(typeof(ItemExtractionTable), typeDiscriminator: "itemextraction")]
[JsonDerivedType(typeof(GemstoneUpgradeTable), typeDiscriminator: "gemstoneupgrade")]
[JsonDerivedType(typeof(MagicPathTable), typeDiscriminator: "magicpath")]
[JsonDerivedType(typeof(InstrumentTable), typeDiscriminator: "instrument")]
[JsonDerivedType(typeof(InteractObjectTable), typeDiscriminator: "interactobject")]
[JsonDerivedType(typeof(ItemOptionConstantTable), typeDiscriminator: "itemoptionconstant")]
[JsonDerivedType(typeof(ItemOptionRandomTable), typeDiscriminator: "itemoptionrandom")]
[JsonDerivedType(typeof(ItemOptionStaticTable), typeDiscriminator: "itemoptionstatic")]
[JsonDerivedType(typeof(ItemOptionPickTable), typeDiscriminator: "itemoptionpick")]
[JsonDerivedType(typeof(ItemVariationTable), typeDiscriminator: "itemvariation")]
[JsonDerivedType(typeof(ItemEquipVariationTable), typeDiscriminator: "itemequipvariation")]
[JsonDerivedType(typeof(EnchantScrollTable), typeDiscriminator: "enchantscroll")]
[JsonDerivedType(typeof(ItemRemakeScrollTable), typeDiscriminator: "itemremakescroll")]
[JsonDerivedType(typeof(ItemRepackingScrollTable), typeDiscriminator: "itemrepackingscroll")]
[JsonDerivedType(typeof(LapenshardUpgradeTable), typeDiscriminator: "lapenshardupgrade")]
[JsonDerivedType(typeof(ItemSocketTable), typeDiscriminator: "itemsocket")]
[JsonDerivedType(typeof(ItemSocketScrollTable), typeDiscriminator: "itemsocketscroll")]
[JsonDerivedType(typeof(ItemExchangeScrollTable), typeDiscriminator: "itemexchangescroll")]
[JsonDerivedType(typeof(ChatStickerTable), typeDiscriminator: "chatsticker")]
[JsonDerivedType(typeof(MasteryRecipeTable), typeDiscriminator: "masteryrecipe")]
[JsonDerivedType(typeof(MasteryRewardTable), typeDiscriminator: "masteryreward")]
[JsonDerivedType(typeof(FishTable), typeDiscriminator: "fish")]
[JsonDerivedType(typeof(FishingSpotTable), typeDiscriminator: "fishingspot")]
[JsonDerivedType(typeof(FishingRodTable), typeDiscriminator: "fishingrod")]
[JsonDerivedType(typeof(FishingRewardTable), typeDiscriminator: "fishingreward")]
[JsonDerivedType(typeof(GuildTable), typeDiscriminator: "guild")]
[JsonDerivedType(typeof(PremiumClubTable), typeDiscriminator: "vip")]
[JsonDerivedType(typeof(IndividualItemDropTable), typeDiscriminator: "individualitemdrop")]
[JsonDerivedType(typeof(ColorPaletteTable), typeDiscriminator: "colorpalette")]
[JsonDerivedType(typeof(SetItemTable), typeDiscriminator: "setitem")]
[JsonDerivedType(typeof(DefaultItemsTable), typeDiscriminator: "defaultitems")]
[JsonDerivedType(typeof(MeretMarketCategoryTable), typeDiscriminator: "meretmarketcategory")]
[JsonDerivedType(typeof(ShopBeautyCouponTable), typeDiscriminator: "shopbeautycoupon")]
[JsonDerivedType(typeof(GachaInfoTable), typeDiscriminator: "gacha_info")]
[JsonDerivedType(typeof(InsigniaTable), typeDiscriminator: "nametagsymbol")]
[JsonDerivedType(typeof(ExpTable), typeDiscriminator: "exp")]
[JsonDerivedType(typeof(CommonExpTable), typeDiscriminator: "commonexp")]
[JsonDerivedType(typeof(UgcDesignTable), typeDiscriminator: "ugc_design")]
[JsonDerivedType(typeof(LearningQuestTable), typeDiscriminator: "learningquest")]
[JsonDerivedType(typeof(PrestigeLevelAbilityTable), typeDiscriminator: "adventurelevelability")]
[JsonDerivedType(typeof(PrestigeLevelRewardTable), typeDiscriminator: "adventurelevelreward")]
[JsonDerivedType(typeof(PrestigeMissionTable), typeDiscriminator: "adventurelevelmission")]
[JsonDerivedType(typeof(BlackMarketTable), typeDiscriminator: "blackmarkettable")]
[JsonDerivedType(typeof(ChangeJobTable), typeDiscriminator: "changejob")]
[JsonDerivedType(typeof(ChapterBookTable), typeDiscriminator: "chapterbook")]
public abstract record Table;
