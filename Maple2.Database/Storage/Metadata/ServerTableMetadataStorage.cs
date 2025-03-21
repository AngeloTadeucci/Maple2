using System;
using Maple2.Database.Context;
using Maple2.Model.Enum;
using Maple2.Model.Game.Event;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class ServerTableMetadataStorage {
    private readonly Lazy<InstanceFieldTable> instanceFieldTable;
    private readonly Lazy<ScriptConditionTable> scriptConditionTable;
    private readonly Lazy<ScriptFunctionTable> scriptFunctionTable;
    private readonly Lazy<JobConditionTable> jobConditionTable;
    private readonly Lazy<BonusGameTable> bonusGameTable;
    private readonly Lazy<GlobalDropItemBoxTable> globalDropItemBoxTable;
    private readonly Lazy<UserStatTable> userStatTable;
    private readonly Lazy<IndividualDropItemTable> individualDropItemTable;
    private readonly Lazy<PrestigeExpTable> prestigeExpTable;
    private readonly Lazy<PrestigeIdExpTable> prestigeIdExpTable;
    private readonly Lazy<TimeEventTable> timeEventTable;
    private readonly Lazy<GameEventTable> gameEventTable;
    private readonly Lazy<OxQuizTable> oxQuizTable;
    private readonly Lazy<ItemMergeTable> itemMergeTable;
    private readonly Lazy<ShopTable> shopTable;
    private readonly Lazy<ShopItemTable> shopItemTable;
    private readonly Lazy<BeautyShopTable> beautyShopTable;
    private readonly Lazy<MeretMarketTable> meretMarketTable;
    private readonly Lazy<FishTable> fishTable;
    private readonly Lazy<CombineSpawnTable> combineSpawnTable;

    public InstanceFieldTable InstanceFieldTable => instanceFieldTable.Value;
    public ScriptConditionTable ScriptConditionTable => scriptConditionTable.Value;
    public ScriptFunctionTable ScriptFunctionTable => scriptFunctionTable.Value;
    public JobConditionTable JobConditionTable => jobConditionTable.Value;
    public BonusGameTable BonusGameTable => bonusGameTable.Value;
    public GlobalDropItemBoxTable GlobalDropItemBoxTable => globalDropItemBoxTable.Value;
    public UserStatTable UserStatTable => userStatTable.Value;
    public IndividualDropItemTable IndividualDropItemTable => individualDropItemTable.Value;
    public PrestigeExpTable PrestigeExpTable => prestigeExpTable.Value;
    public PrestigeIdExpTable PrestigeIdExpTable => prestigeIdExpTable.Value;
    public TimeEventTable TimeEventTable => timeEventTable.Value;
    public GameEventTable GameEventTable => gameEventTable.Value;
    public OxQuizTable OxQuizTable => oxQuizTable.Value;
    public ItemMergeTable ItemMergeTable => itemMergeTable.Value;
    public ShopTable ShopTable => shopTable.Value;
    public ShopItemTable ShopItemTable => shopItemTable.Value;
    public BeautyShopTable BeautyShopTable => beautyShopTable.Value;
    public MeretMarketTable MeretMarketTable => meretMarketTable.Value;
    public FishTable FishTable => fishTable.Value;
    public CombineSpawnTable CombineSpawnTable => combineSpawnTable.Value;

    public ServerTableMetadataStorage(MetadataContext context) {
        instanceFieldTable = Retrieve<InstanceFieldTable>(context, "instancefield.xml");
        scriptConditionTable = Retrieve<ScriptConditionTable>(context, "*scriptCondition.xml");
        scriptFunctionTable = Retrieve<ScriptFunctionTable>(context, "*scriptFunction.xml");
        jobConditionTable = Retrieve<JobConditionTable>(context, "jobConditionTable.xml");
        bonusGameTable = Retrieve<BonusGameTable>(context, "bonusGame*.xml");
        globalDropItemBoxTable = Retrieve<GlobalDropItemBoxTable>(context, "globalItemDrop*.xml");
        userStatTable = Retrieve<UserStatTable>(context, "userStat*.xml");
        individualDropItemTable = Retrieve<IndividualDropItemTable>(context, "individualItemDrop.xml");
        prestigeExpTable = Retrieve<PrestigeExpTable>(context, "adventureExpTable.xml");
        prestigeIdExpTable = Retrieve<PrestigeIdExpTable>(context, "adventureExpIdTable.xml");
        timeEventTable = Retrieve<TimeEventTable>(context, "timeEventData.xml");
        gameEventTable = Retrieve<GameEventTable>(context, "gameEvent.xml");
        oxQuizTable = Retrieve<OxQuizTable>(context, "oxQuiz.xml");
        itemMergeTable = Retrieve<ItemMergeTable>(context, "itemMergeOptionBase.xml");
        shopTable = Retrieve<ShopTable>(context, "shop_game_info.xml");
        shopItemTable = Retrieve<ShopItemTable>(context, "shop_game.xml");
        beautyShopTable = Retrieve<BeautyShopTable>(context, "shop_beauty.xml");
        meretMarketTable = Retrieve<MeretMarketTable>(context, "shop_merat_custom.xml");
        fishTable = Retrieve<FishTable>(context, "fish*.xml");
        combineSpawnTable = Retrieve<CombineSpawnTable>(context, "combineSpawn*.xml");
    }

    public IEnumerable<GameEvent> GetGameEvents() {
        foreach ((int id, GameEventMetadata gameEvent) in GameEventTable.Entries) {
            if (gameEvent.EndTime < DateTime.Now) {
                continue;
            }

            yield return new GameEvent(gameEvent);
        }
    }

    private static Lazy<T> Retrieve<T>(MetadataContext context, string key) where T : ServerTable {
        var result = new Lazy<T>(() => {
            lock (context) {
                ServerTableMetadata? row = context.ServerTableMetadata.Find(key);
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
