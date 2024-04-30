using System.Globalization;
using Maple2.Database.Extensions;
using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class ServerTableMapper : TypeMapper<ServerTableMetadata> {
    private readonly ServerTableParser parser;

    public ServerTableMapper(M2dReader xmlReader) {
        parser = new ServerTableParser(xmlReader);
    }

    protected override IEnumerable<ServerTableMetadata> Map() {
        yield return new ServerTableMetadata { Name = "instancefield.xml", Table = ParseInstanceField() };
        yield return new ServerTableMetadata { Name = "*scriptCondition.xml", Table = ParseScriptCondition() };
        yield return new ServerTableMetadata { Name = "*scriptFunction.xml", Table = ParseScriptFunction() };
        yield return new ServerTableMetadata { Name = "jobConditionTable.xml", Table = ParseJobCondition() };

    }

    private InstanceFieldTable ParseInstanceField() {
        var results = new Dictionary<int, InstanceFieldMetadata>();
        foreach ((int instanceId, Parser.Xml.Table.Server.InstanceField instanceField) in parser.ParseInstanceField()) {
            foreach (int fieldId in instanceField.fieldIDs) {

                InstanceFieldMetadata instanceFieldMetadata = new(
                    MapId: fieldId,
                    Type: (InstanceType) instanceField.instanceType,
                    InstanceId: instanceId,
                    BackupSourcePortal: instanceField.backupSourcePortal,
                    PoolCount: instanceField.poolCount,
                    SaveField: instanceField.isSaveField,
                    NpcStatFactorId: instanceField.npcStatFactorID,
                    MaxCount: instanceField.maxCount,
                    OpenType: instanceField.openType,
                    OpenValue: instanceField.openValue
                );

                results.Add(fieldId, instanceFieldMetadata);
            }
        }

        return new InstanceFieldTable(results);
    }

    private ScriptConditionTable ParseScriptCondition() {
        var results = new Dictionary<int, Dictionary<int, ScriptConditionMetadata>>();
        results = MergeNpcScriptConditions(results, parser.ParseNpcScriptCondition());
        results = MergeQuestScriptConditions(results, parser.ParseQuestScriptCondition());

        return new ScriptConditionTable(results);
    }

    private Dictionary<int, Dictionary<int, ScriptConditionMetadata>> MergeNpcScriptConditions(Dictionary<int, Dictionary<int, ScriptConditionMetadata>> results, IEnumerable<(int NpcId, IDictionary<int, Parser.Xml.Table.Server.NpcScriptCondition> ScriptConditions)> parser) {
        foreach ((int npcId, IDictionary<int, Parser.Xml.Table.Server.NpcScriptCondition> scripts) in parser) {
            var scriptConditions = new Dictionary<int, ScriptConditionMetadata>();
            foreach ((int scriptId, Parser.Xml.Table.Server.NpcScriptCondition scriptCondition) in scripts) {
                var questStarted = new Dictionary<int, bool>();
                foreach (string quest in scriptCondition.quest_start) {
                    KeyValuePair<int, bool> parsedQuest = ParseToKeyValuePair(quest);
                    questStarted.Add(parsedQuest.Key, parsedQuest.Value);
                }

                var questsCompleted = new Dictionary<int, bool>();
                foreach (string quest in scriptCondition.quest_complete) {
                    KeyValuePair<int, bool> parsedQuest = ParseToKeyValuePair(quest);
                    questsCompleted.Add(parsedQuest.Key, parsedQuest.Value);
                }

                var items = new List<KeyValuePair<ItemComponent, bool>>();
                for (int i = 0; i < scriptCondition.item.Length; i++) {
                    KeyValuePair<int, bool> parsedItem = ParseToKeyValuePair(scriptCondition.item[i]);
                    string itemCount = scriptCondition.itemCount.ElementAtOrDefault(i) ?? "1";
                    if (!int.TryParse(itemCount, out int itemAmount)) {
                        itemAmount = 1;
                    }
                    var item = new ItemComponent(parsedItem.Key, -1, itemAmount, ItemTag.None);
                    items.Add(new KeyValuePair<ItemComponent, bool>(item, parsedItem.Value));
                }

                scriptConditions.Add(scriptId, new ScriptConditionMetadata(
                    Id: npcId,
                    ScriptId: scriptId,
                    Type: ScriptType.Npc,
                    MaidAuthority: scriptCondition.maid_auth,
                    MaidExpired: scriptCondition.maid_expired != "!1",
                    MaidReadyToPay: scriptCondition.maid_ready_to_pay != "!1",
                    MaidClosenessRank: scriptCondition.maid_affinity_grade,
                    MaidClosenessTime: ParseToKeyValuePair(scriptCondition.maid_affinity_time),
                    MaidMoodTime: ParseToKeyValuePair(scriptCondition.maid_mood_time),
                    MaidDaysBeforeExpired: ParseToKeyValuePair(scriptCondition.maid_day_before_expired),
                    JobCode: scriptCondition.job?.Select(job => (JobCode) job).ToList() ?? new List<JobCode>(),
                    QuestStarted: questStarted,
                    QuestCompleted: questsCompleted,
                    Items: items,
                    Buff: ParseToKeyValuePair(scriptCondition.buff),
                    Meso: ParseToKeyValuePair(scriptCondition.meso),
                    Level: ParseToKeyValuePair(scriptCondition.level),
                    AchieveCompleted: ParseToKeyValuePair(scriptCondition.achieve_complete),
                    InGuild: scriptCondition.guild
                ));
            }
            results.Add(npcId, scriptConditions);
        }
        return results;
    }

    private Dictionary<int, Dictionary<int, ScriptConditionMetadata>> MergeQuestScriptConditions(Dictionary<int, Dictionary<int, ScriptConditionMetadata>> results, IEnumerable<(int NpcId, IDictionary<int, Parser.Xml.Table.Server.QuestScriptCondition> ScriptConditions)> parser) {
        foreach ((int questId, IDictionary<int, Parser.Xml.Table.Server.QuestScriptCondition> scripts) in parser) {
            var scriptConditions = new Dictionary<int, ScriptConditionMetadata>();
            foreach ((int scriptId, Parser.Xml.Table.Server.QuestScriptCondition scriptCondition) in scripts) {
                var questStarted = new Dictionary<int, bool>();
                foreach (string quest in scriptCondition.quest_start) {
                    KeyValuePair<int, bool> parsedQuest = ParseToKeyValuePair(quest);
                    questStarted.Add(parsedQuest.Key, parsedQuest.Value);
                }

                var questsCompleted = new Dictionary<int, bool>();
                foreach (string quest in scriptCondition.quest_complete) {
                    KeyValuePair<int, bool> parsedQuest = ParseToKeyValuePair(quest);
                    questsCompleted.Add(parsedQuest.Key, parsedQuest.Value);
                }

                var items = new List<KeyValuePair<ItemComponent, bool>>();
                for (int i = 0; i < scriptCondition.item.Length; i++) {
                    KeyValuePair<int, bool> parsedItem = ParseToKeyValuePair(scriptCondition.item[i]);
                    string itemCount = scriptCondition.itemCount.ElementAtOrDefault(i) ?? "1";
                    if (!int.TryParse(itemCount, out int itemAmount)) {
                        itemAmount = 1;
                    }
                    var item = new ItemComponent(parsedItem.Key, -1, itemAmount, ItemTag.None);
                    items.Add(new KeyValuePair<ItemComponent, bool>(item, parsedItem.Value));
                }

                scriptConditions.Add(scriptId, new ScriptConditionMetadata(
                    Id: questId,
                    ScriptId: scriptId,
                    Type: ScriptType.Quest,
                    MaidAuthority: scriptCondition.maid_auth,
                    MaidExpired: scriptCondition.maid_expired != "!1",
                    MaidReadyToPay: scriptCondition.maid_ready_to_pay != "!1",
                    MaidClosenessRank: scriptCondition.maid_affinity_grade,
                    MaidClosenessTime: ParseToKeyValuePair(scriptCondition.maid_affinity_time),
                    MaidMoodTime: ParseToKeyValuePair(scriptCondition.maid_mood_time),
                    MaidDaysBeforeExpired: ParseToKeyValuePair(scriptCondition.maid_day_before_expired),
                    JobCode: scriptCondition.job?.Select(job => (JobCode) job).ToList() ?? new List<JobCode>(),
                    QuestStarted: questStarted,
                    QuestCompleted: questsCompleted,
                    Items: items,
                    Buff: ParseToKeyValuePair(scriptCondition.buff),
                    Meso: ParseToKeyValuePair(scriptCondition.meso),
                    Level: ParseToKeyValuePair(scriptCondition.level),
                    AchieveCompleted: ParseToKeyValuePair(scriptCondition.achieve_complete),
                    InGuild: scriptCondition.guild
                ));
            }
            results.Add(questId, scriptConditions);
        }
        return results;
    }

    private static KeyValuePair<int, bool> ParseToKeyValuePair(string input) {
        bool value = !input.StartsWith("!");

        if (!value) {
            input = input.Replace("!", "");
        }

        if (!int.TryParse(input, out int key)) {
            key = 0;
        }
        return new KeyValuePair<int, bool>(key, value);
    }

    private ScriptFunctionTable ParseScriptFunction() {
        var results = new Dictionary<int, Dictionary<int, Dictionary<int, ScriptFunctionMetadata>>>();
        results = MergeNpcScriptFunctions(results, parser.ParseNpcScriptFunction());
        results = MergeQuestScriptFunctions(results, parser.ParseQuestScriptFunction());

        return new ScriptFunctionTable(results);
    }

    private static Dictionary<int, Dictionary<int, Dictionary<int, ScriptFunctionMetadata>>> MergeNpcScriptFunctions(Dictionary<int, Dictionary<int, Dictionary<int, ScriptFunctionMetadata>>> results, IEnumerable<(int NpcId, IDictionary<int, Parser.Xml.Table.Server.NpcScriptFunction> ScriptFunctions)> parser) {
        foreach ((int npcId, IDictionary<int, Parser.Xml.Table.Server.NpcScriptFunction> scripts) in parser) {
            var scriptDict = new Dictionary<int, Dictionary<int, ScriptFunctionMetadata>>(); // scriptIds, functionDict
            foreach ((int scriptId, Parser.Xml.Table.Server.NpcScriptFunction scriptFunction) in scripts) {
                var presentItems = new List<ItemComponent>();
                for (int i = 0; i < scriptFunction.presentItemID.Length; i++) {
                    short itemRarity = scriptFunction.presentItemRank.ElementAtOrDefault(i) != default(short) ? scriptFunction.presentItemRank.ElementAtOrDefault(i) : (short) -1;
                    int itemAmount = scriptFunction.presentItemAmount.ElementAtOrDefault(i) != default ? scriptFunction.presentItemAmount.ElementAtOrDefault(i) : 1;
                    presentItems.Add(new ItemComponent(scriptFunction.presentItemID[i], itemRarity, itemAmount, ItemTag.None));
                }

                var collectItems = new List<ItemComponent>();
                for (int i = 0; i < scriptFunction.collectItemID.Length; i++) {
                    int itemAmount = scriptFunction.collectItemAmount.ElementAtOrDefault(i) != default ? scriptFunction.collectItemAmount.ElementAtOrDefault(i) : 1;
                    collectItems.Add(new ItemComponent(scriptFunction.collectItemID[i], -1, itemAmount, ItemTag.None));
                }

                var metadata = new ScriptFunctionMetadata(
                    Id: npcId, // NpcId or QuestId
                    ScriptId: scriptId,
                    Type: ScriptType.Npc,
                    FunctionId: scriptFunction.functionID,
                    EndFunction: scriptFunction.endFunction,
                    PortalId: scriptFunction.portal,
                    UiName: scriptFunction.uiName,
                    UiArg: scriptFunction.uiArg,
                    UiArg2: scriptFunction.uiArg2,
                    MoveMapId: scriptFunction.moveFieldID,
                    MovePortalId: scriptFunction.moveFieldPortalID,
                    MoveMapMovie: scriptFunction.moveFieldMovie,
                    Emoticon: scriptFunction.emoticon,
                    PresentItems: presentItems,
                    CollectItems: collectItems,
                    SetTriggerValueTriggerId: scriptFunction.setTriggerValueTriggerID,
                    SetTriggerValueKey: scriptFunction.setTriggerValueKey,
                    SetTriggerValue: scriptFunction.setTriggerValue,
                    Divorce: scriptFunction.divorce,
                    PresentExp: scriptFunction.presentExp,
                    CollectMeso: scriptFunction.collectMeso,
                    MaidMoodIncrease: scriptFunction.maidMoodUp,
                    MaidClosenessIncrease: scriptFunction.maidAffinityUp,
                    MaidPay: scriptFunction.maidPay
                );
                if (!scriptDict.TryGetValue(scriptId, out Dictionary<int, ScriptFunctionMetadata>? functionDict)) {
                    functionDict = new Dictionary<int, ScriptFunctionMetadata> {
                        {
                            scriptFunction.functionID, metadata
                        },
                    };
                    scriptDict.Add(scriptId, functionDict);
                } else {
                    functionDict.Add(scriptFunction.functionID, metadata);
                }
            }
            results.Add(npcId, scriptDict);
        }
        return results;
    }

    private static Dictionary<int, Dictionary<int, Dictionary<int, ScriptFunctionMetadata>>> MergeQuestScriptFunctions(Dictionary<int, Dictionary<int, Dictionary<int, ScriptFunctionMetadata>>> results, IEnumerable<(int NpcId, IDictionary<int, Parser.Xml.Table.Server.QuestScriptFunction> ScriptFunctions)> parser) {
        foreach ((int questId, IDictionary<int, Parser.Xml.Table.Server.QuestScriptFunction> scripts) in parser) {
            var scriptDict = new Dictionary<int, Dictionary<int, ScriptFunctionMetadata>>(); // scriptIds, functionDict
            foreach ((int scriptId, Parser.Xml.Table.Server.QuestScriptFunction scriptFunction) in scripts) {
                var presentItems = new List<ItemComponent>();
                for (int i = 0; i < scriptFunction.presentItemID.Length; i++) {
                    short itemRarity = scriptFunction.presentItemRank.ElementAtOrDefault(i) != default(short) ? scriptFunction.presentItemRank.ElementAtOrDefault(i) : (short) -1;
                    int itemAmount = scriptFunction.presentItemAmount.ElementAtOrDefault(i) != default ? scriptFunction.presentItemAmount.ElementAtOrDefault(i) : 1;
                    presentItems.Add(new ItemComponent(scriptFunction.presentItemID[i], itemRarity, itemAmount, ItemTag.None));
                }

                var collectItems = new List<ItemComponent>();
                for (int i = 0; i < scriptFunction.collectItemID.Length; i++) {
                    int itemAmount = scriptFunction.collectItemAmount.ElementAtOrDefault(i) != default ? scriptFunction.collectItemAmount.ElementAtOrDefault(i) : 1;
                    collectItems.Add(new ItemComponent(scriptFunction.collectItemID[i], -1, itemAmount, ItemTag.None));
                }

                var metadata = new ScriptFunctionMetadata(
                    Id: questId,
                    ScriptId: scriptId,
                    Type: ScriptType.Quest,
                    FunctionId: scriptFunction.functionID,
                    EndFunction: scriptFunction.endFunction,
                    PortalId: scriptFunction.portal,
                    UiName: scriptFunction.uiName,
                    UiArg: scriptFunction.uiArg,
                    UiArg2: scriptFunction.uiArg2,
                    MoveMapId: scriptFunction.moveFieldID,
                    MovePortalId: scriptFunction.moveFieldPortalID,
                    MoveMapMovie: scriptFunction.moveFieldMovie,
                    Emoticon: scriptFunction.emoticon,
                    PresentItems: presentItems,
                    CollectItems: collectItems,
                    SetTriggerValueTriggerId: scriptFunction.setTriggerValueTriggerID,
                    SetTriggerValueKey: scriptFunction.setTriggerValueKey,
                    SetTriggerValue: scriptFunction.setTriggerValue,
                    Divorce: scriptFunction.divorce,
                    PresentExp: scriptFunction.presentExp,
                    CollectMeso: scriptFunction.collectMeso,
                    MaidMoodIncrease: scriptFunction.maidMoodUp,
                    MaidClosenessIncrease: scriptFunction.maidAffinityUp,
                    MaidPay: scriptFunction.maidPay
                );
                if (!scriptDict.TryGetValue(scriptId, out Dictionary<int, ScriptFunctionMetadata>? functionDict)) {
                    functionDict = new Dictionary<int, ScriptFunctionMetadata> {
                        {
                            scriptFunction.functionID, metadata
                        },
                    };
                    scriptDict.Add(scriptId, functionDict);
                } else {
                    functionDict.Add(scriptFunction.functionID, metadata);
                }
            }
            results.Add(questId, scriptDict);
        }
        return results;
    }

    private JobConditionTable ParseJobCondition() {
        var results = new Dictionary<int, JobConditionMetadata>();
        foreach ((int npcId, Parser.Xml.Table.Server.JobConditionTable jobCondition) in parser.ParseJobConditionTable()) {
            DateTime date = DateTime.TryParseExact(jobCondition.date, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ? date : DateTime.MinValue;
            results.Add(npcId, new JobConditionMetadata(
                NpcId: npcId,
                ScriptId: jobCondition.scriptID,
                StartedQuestId: jobCondition.quest_start,
                CompletedQuestId: jobCondition.quest_complete,
                JobCode: (JobCode) jobCondition.job,
                MaidAuthority: jobCondition.maid_auth,
                MaidClosenessTime: jobCondition.maid_affinity_time,
                MaidCosenessRank: jobCondition.maid_affinity_grade,
                Date: date.ToEpochSeconds(),
                BuffId: jobCondition.buff,
                Mesos: jobCondition.meso,
                Level: jobCondition.level,
                Home: jobCondition.home,
                Roulette: jobCondition.roulette,
                Guild: jobCondition.guild,
                CompletedAchievement: jobCondition.achieve_complete,
                IsBirthday: jobCondition.birthday,
                ChangeToJobCode: (JobCode) jobCondition.jobCode,
                MapId: jobCondition.map,
                MoveMapId: jobCondition.moveFieldID,
                MovePortalId: jobCondition.movePortalID
            ));
        }

        return new JobConditionTable(results);
    }
}
