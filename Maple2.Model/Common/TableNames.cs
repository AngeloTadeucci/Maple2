namespace Maple2.Model.Common;

public static class TableNames {
    // Common table names
    public const string CHAT_EMOTICON = "chatemoticon.xml";
    public const string DEFAULT_ITEMS = "defaultitems.xml";
    public const string ITEM_BREAK_INGREDIENT = "itembreakingredient.xml";
    public const string ITEM_GEMSTONE_UPGRADE = "itemgemstoneupgrade.xml";
    public const string ITEM_EXTRACTION = "itemextraction.xml";
    public const string JOB = "job.xml";
    public const string MAGIC_PATH = "magicpath.xml";
    public const string INSTRUMENT_CATEGORY_INFO = "instrumentcategoryinfo.xml";
    public const string INTERACT_OBJECT = "interactobject*.xml";
    public const string ITEM_LAPENSHARD_UPGRADE = "itemlapenshardupgrade.xml";
    public const string ITEM_SOCKET = "itemsocket.xml";
    public const string MASTERY_RECIPE = "masteryreceipe.xml";
    public const string MASTERY = "mastery.xml";
    public const string GUILD = "guild*.xml";
    public const string VIP = "vip*.xml";
    public const string INDIVIDUAL_ITEM_DROP = "individualitemdrop*.xml";
    public const string COLOR_PALETTE = "colorpalette.xml";
    public const string MERET_MARKET_CATEGORY = "meretmarketcategory.xml";
    public const string SHOP_BEAUTY_COUPON = "shop_beautycoupon.xml";
    public const string SHOP_FURNISHING = "na/shop_*.xml";
    public const string GACHA_INFO = "gacha_info.xml";
    public const string NAME_TAG_SYMBOL = "nametagsymbol.xml";
    public const string EXP = "exp*.xml";
    public const string COMMON_EXP = "commonexp.xml";
    public const string UGC_DESIGN = "ugcdesign.xml";
    public const string MASTERY_UGC_HOUSING = "masteryugchousing.xml";
    public const string UGC_HOUSING_POINT_REWARD = "ugchousingpointreward.xml";
    public const string LEARNING_QUEST = "learningquest.xml";
    public const string BLACK_MARKET_TABLE = "blackmarkettable.xml";
    public const string CHANGE_JOB = "changejob.xml";
    public const string CHAPTER_BOOK = "chapterbook.xml";
    public const string FIELD_MISSION = "fieldmission.xml";
    public const string WORLD_MAP = "newworldmap.xml";
    public const string SURVIVAL_SKIN_INFO = "maplesurvivalskininfo.xml";
    public const string BANNER = "banner.xml";
    public const string WEDDING = "wedding*.xml";
    public const string REWARD_CONTENT = "rewardcontent*.xml";
    public const string SEASON_DATA = "seasondata*.xml";
    public const string SMART_PUSH = "smartpush.xml";
    public const string AUTO_ACTION = "autoactionpricepackage.xml";

    // Prestige / Adventure
    public const string PRESTIGE_LEVEL_ABILITY = "adventurelevelability.xml";
    public const string PRESTIGE_LEVEL_REWARD = "adventurelevelreward.xml";
    public const string PRESTIGE_MISSION = "adventurelevelmission.xml";

    // Fishing
    public const string FISHING_ROD = "fishingrod.xml";

    // Scrolls
    public const string ENCHANT_SCROLL = "enchantscroll.xml";
    public const string ITEM_REMAKE_SCROLL = "itemremakescroll.xml";
    public const string ITEM_REPACKING_SCROLL = "itemrepackingscroll.xml";
    public const string ITEM_SOCKET_SCROLL = "itemsocketscroll.xml";
    public const string ITEM_EXCHANGE_SCROLL = "itemexchangescrolltable.xml";

    // Item Options
    public const string ITEM_OPTION_CONSTANT = "itemoptionconstant.xml";
    public const string ITEM_OPTION_RANDOM = "itemoptionrandom.xml";
    public const string ITEM_OPTION_STATIC = "itemoptionstatic.xml";
    public const string ITEM_OPTION_PICK = "itemoptionpick.xml";
    public const string ITEM_OPTION_VARIATION = "itemoptionvariation.xml";
    public const string ITEM_OPTION_VARIATION_ACC = "itemoptionvariation_acc.xml";
    public const string ITEM_OPTION_VARIATION_ARMOR = "itemoptionvariation_armor.xml";
    public const string ITEM_OPTION_VARIATION_PET = "itemoptionvariation_pet.xml";
    public const string ITEM_OPTION_VARIATION_WEAPON = "itemoptionvariation_weapon.xml";

    // Set Items
    public const string SET_ITEM = "setitem*.xml";

    // Dungeon
    public const string DUNGEON_ROOM = "dungeonroom.xml";
    public const string DUNGEON_RANK_REWARD = "dungeonrankreward.xml";
    public const string DUNGEON_CONFIG = "dungeonconfig.xml";
    public const string DUNGEON_MISSION = "dungeonmission.xml";

    public static readonly Dictionary<string, string> ItemOptionVariationTableNames = new Dictionary<string, string> {
        { "acc", ITEM_OPTION_VARIATION_ACC },
        { "armor", ITEM_OPTION_VARIATION_ARMOR },
        { "pet", ITEM_OPTION_VARIATION_PET },
        { "weapon", ITEM_OPTION_VARIATION_WEAPON },
    };
}
