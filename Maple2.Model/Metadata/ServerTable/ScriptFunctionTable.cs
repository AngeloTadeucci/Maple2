using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record ScriptFunctionTable(IReadOnlyDictionary<int, Dictionary<int, Dictionary<int, ScriptFunctionMetadata>>> Entries) : ServerTable;

public record ScriptFunctionMetadata(
    int Id, // QuestId or NpcId
    int ScriptId,
    ScriptType Type,
    int FunctionId,
    bool EndFunction,
    int PortalId,
    string UiName,
    string UiArg,
    string UiArg2,
    int MoveMapId,
    int MovePortalId,
    string MoveMapMovie,
    string Emoticon,
    IList<ItemComponent> PresentItems,
    IList<ItemComponent> CollectItems,
    int SetTriggerValueTriggerId,
    string SetTriggerValueKey,
    string SetTriggerValue,
    string Divorce,
    long PresentExp,
    long CollectMeso,
    int MaidMoodIncrease,
    int MaidClosenessIncrease,
    bool MaidPay);
