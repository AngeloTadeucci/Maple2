using System.Collections.Generic;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record QuestMetadata(
    int Id,
    string? Name,
    QuestMetadataBasic Basic,
    QuestMetadataRequire Require,
    QuestMetadataReward AcceptReward,
    QuestMetadataReward CompleteReward,
    QuestMetadataGoToNpc GoToNpc,
    QuestMetadataGoToDungeon GoToDungeon,
    ConditionMetadata[] Conditions) : ISearchResult;

public record QuestMetadataBasic(
    int ChapterId,
    QuestType Type,
    int Account, // 1 and 2 are both account quests. Unsure of the difference.
    int StandardLevel,
    bool Forfeitable,
    string EventTag,
    bool AutoStart,
    bool Disabled,
    bool UsePostbox, // quest is sent to player remotely
    int StartNpc,
    int CompleteNpc,
    int[]? CompleteMaps,
    int[]? ProgressMaps);

public record QuestMetadataRequire(
    short Level,
    short MaxLevel,
    JobCode[] Job,
    int[] Quest,
    int[] SelectableQuest,
    int Achievement,
    (int, int) UnrequiredAchievement, // (AchievementId, Grade) Player CANNOT have this achievement to start the quest.
    int GearScore);

public record QuestMetadataReward(
    int Meso,
    int Exp,
    ExpType RelativeExp,
    int GuildFund,
    int GuildExp,
    int GuildCoin,
    int MenteeCoin,
    int MissionPoint,
    int Treva,
    int Rue,
    List<QuestMetadataReward.Item> EssentialItem,
    List<QuestMetadataReward.Item> EssentialJobItem) {

    public record Item(int Id, int Rarity, int Amount);
}

public record QuestMetadataGoToNpc(
    bool Enabled,
    int MapId,
    int PortalId);

public record QuestMetadataGoToDungeon(
    QuestState State,
    int MapId,
    int InstanceId);


