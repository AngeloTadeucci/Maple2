using Maple2.Model.Enum;
using Maple2.Model.Game;
using MaidData = Maple2.Model.Metadata.ScriptConditionMetadata.MaidData;
using WeddingData = Maple2.Model.Metadata.ScriptConditionMetadata.WeddingData;

namespace Maple2.Model.Metadata;

public record ScriptConditionTable(IReadOnlyDictionary<int, Dictionary<int, ScriptConditionMetadata>> Entries) : ServerTable;

public record ScriptConditionMetadata(
    int Id, // QuestId or NpcId
    int ScriptId,
    ScriptType Type,
    MaidData Maid,
    WeddingData Wedding,
    IReadOnlyList<JobCode> JobCode,
    IReadOnlyDictionary<int, bool> QuestStarted,
    IReadOnlyDictionary<int, bool> QuestCompleted,
    IList<KeyValuePair<ItemComponent, bool>> Items,
    KeyValuePair<int, bool> Buff,
    KeyValuePair<int, bool> Meso,
    KeyValuePair<int, bool> Level,
    KeyValuePair<int, bool> AchieveCompleted,
    bool InGuild) {

    public record MaidData(
        bool Authority,
        bool Expired,
        bool ReadyToPay,
        int ClosenessRank,
        KeyValuePair<int, bool> ClosenessTime,
        KeyValuePair<int, bool> MoodTime,
        KeyValuePair<int, bool> DaysBeforeExpired
    );

    public record WeddingData(
        MaritalStatus? UserState,
        KeyValuePair<string, bool> HallState,
        bool? HasReservation,
        int MarriageDays,
        string CoolingOff);
}
