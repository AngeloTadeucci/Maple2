using System;
using Maple2.Database.Context;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class ServerTableMetadataStorage(MetadataContext context) {
    private readonly Lazy<InstanceFieldTable> instanceFieldTable = Retrieve<InstanceFieldTable>(context, "instancefield.xml");
    private readonly Lazy<ScriptConditionTable> scriptConditionTable = Retrieve<ScriptConditionTable>(context, "*scriptCondition.xml");
    private readonly Lazy<ScriptFunctionTable> scriptFunctionTable = Retrieve<ScriptFunctionTable>(context, "*scriptFunction.xml");
    private readonly Lazy<JobConditionTable> jobConditionTable = Retrieve<JobConditionTable>(context, "jobConditionTable.xml");
    private readonly Lazy<BonusGameTable> bonusGameTable = Retrieve<BonusGameTable>(context, "bonusGame*.xml");
    private readonly Lazy<GlobalDropItemBoxTable> globalDropItemBoxTable = Retrieve<GlobalDropItemBoxTable>(context, "globalItemDrop*.xml");
    private readonly Lazy<UserStatTable> userStatTable = Retrieve<UserStatTable>(context, "userStat*.xml");

    public InstanceFieldTable InstanceFieldTable => instanceFieldTable.Value;
    public ScriptConditionTable ScriptConditionTable => scriptConditionTable.Value;
    public ScriptFunctionTable ScriptFunctionTable => scriptFunctionTable.Value;
    public JobConditionTable JobConditionTable => jobConditionTable.Value;
    public BonusGameTable BonusGameTable => bonusGameTable.Value;
    public GlobalDropItemBoxTable GlobalDropItemBoxTable => globalDropItemBoxTable.Value;
    public UserStatTable UserStatTable => userStatTable.Value;

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
