using Maple2.Database.Context;
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
    private readonly Lazy<TimeEventTable> timeEventTable;
    private readonly Lazy<GameEventTable> gameEventTable;
    private readonly Lazy<ItemMergeTable> itemMergeTable;

    public InstanceFieldTable InstanceFieldTable => instanceFieldTable.Value;
    public ScriptConditionTable ScriptConditionTable => scriptConditionTable.Value;
    public ScriptFunctionTable ScriptFunctionTable => scriptFunctionTable.Value;
    public JobConditionTable JobConditionTable => jobConditionTable.Value;
    public BonusGameTable BonusGameTable => bonusGameTable.Value;
    public GlobalDropItemBoxTable GlobalDropItemBoxTable => globalDropItemBoxTable.Value;
    public UserStatTable UserStatTable => userStatTable.Value;
    public IndividualDropItemTable IndividualDropItemTable => individualDropItemTable.Value;
    public PrestigeExpTable PrestigeExpTable => prestigeExpTable.Value;
    public TimeEventTable TimeEventTable => timeEventTable.Value;
    public GameEventTable GameEventTable => gameEventTable.Value;
    public ItemMergeTable ItemMergeTable => itemMergeTable.Value;

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
        timeEventTable = Retrieve<TimeEventTable>(context, "timeEventData.xml");
        gameEventTable = Retrieve<GameEventTable>(context, "gameEvent.xml");
        itemMergeTable = Retrieve<ItemMergeTable>(context, "itemMergeOptionBase.xml");
    }

    public IEnumerable<GameEvent> GetGameEvents() {
        foreach ((int id, GameEventMetadata gameEvent) in GameEventTable.Entries) {
            if (gameEvent.EndTime < DateTimeOffset.Now.DateTime) {
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
