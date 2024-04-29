using System.Collections.Generic;
using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record ScriptConditionTable(IReadOnlyDictionary<int, Dictionary<int, ScriptConditionMetadata>> Entries) : ServerTable;

public record ScriptConditionMetadata(
    int Id, // QuestId or NpcId
    int ScriptId,
    ScriptType Type,
    bool MaidAuthority,
    bool MaidExpired,
    bool MaidReadyToPay,
    int MaidClosenessRank,
    KeyValuePair<int, bool> MaidClosenessTime,
    KeyValuePair<int, bool> MaidMoodTime,
    KeyValuePair<int, bool> MaidDaysBeforeExpired,
    IReadOnlyList<JobCode> JobCode,
    IReadOnlyDictionary<int, bool> QuestStarted,
    IReadOnlyDictionary<int, bool> QuestCompleted,
    IList<KeyValuePair<ItemComponent, bool>> Items,
    KeyValuePair<int, bool> Buff,
    KeyValuePair<int, bool> Meso,
    KeyValuePair<int, bool> Level,
    KeyValuePair<int, bool> AchieveCompleted,
    bool InGuild);
