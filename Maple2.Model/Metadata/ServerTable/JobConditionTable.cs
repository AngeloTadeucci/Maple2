using System.Collections.Generic;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record JobConditionTable(IReadOnlyDictionary<int,  JobConditionMetadata> Entries) : ServerTable;

public record JobConditionMetadata(
    int NpcId,
    int ScriptId,
    int StartedQuestId,
    int CompletedQuestId,
    JobCode JobCode,
    bool MaidAuthority,
    int MaidClosenessTime,
    int MaidCosenessRank,
    long Date,
    int BuffId,
    int Mesos,
    short Level,
    bool Home,
    bool Roulette,
    bool Guild,
    int CompletedAchievement,
    bool IsBirthday,
    JobCode ChangeToJobCode,
    int MapId,
    int MoveMapId,
    int MovePortalId);
