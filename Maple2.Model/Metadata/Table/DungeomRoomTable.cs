using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record DungeonRoomTable(IReadOnlyDictionary<int, DungeonRoomTable.DungeonRoomMetadata> Entries) : Table {
    public record DungeonRoomMetadata(
        int Id,
        short Level,
        DungeonPlayType PlayType,
        DungeonGroupType GroupType,
        DungeonCooldownType CooldownType,
        int CooldownValue,
        int DurationTick,
        int LobbyFieldId,
        int[] FieldIds,
        DungeonRoomReward Reward,
        DungeonRoomLimit Limit,
        int PlayerCountFactorId,
        short CustomMonsterLevel,
        int HelperRequireClearCount,
        bool DisabledFindHelper,
        int RankTableId,
        bool LeaveAfterCloseReward,
        int[] PartyMissions,
        int[] UserMissions,
        bool MoveToBackupField
        );
}

public record DungeonRoomReward(
    bool AccountWide,
    int Count,
    int SubRewardCount,
    long Exp,
    float ExpRate,
    long Meso,
    int[] LimitedDropBoxIds,
    int[] UnlimitedDropBoxIds,
    int UnionRewardId,
    int SeasonRankRewardId,
    int ScoreBonusId
);

public record DungeonRoomLimit(
    int MinUserCount,
    int MaxUserCount,
    int GearScore,
    int MinLevel,
    int RequiredAchievementId,
    bool VipOnly,
    DayOfWeek[] DayOfWeeks,
    int[] ClearDungeonIds,
    bool EquippedRecommendedWeapon,
    bool PartyOnly,
    bool ChangeMaxUsers,
    bool DisableMeretRevival,
    bool DisableMesoRevival,
    int MaxRevivalCount
);
