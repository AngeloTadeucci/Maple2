using System.Collections.Concurrent;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;

namespace Maple2.Model.Game.Dungeon;

public class DungeonUserRecord : IUserContentRecord {
    public readonly int DungeonId;
    public long CharacterId { get; init; }
    public bool IsDungeonSuccess;
    public bool WithParty { get; set; }
    public readonly ConcurrentDictionary<DungeonAccumulationRecordType, int> AccumulationRecords;
    public Dictionary<int, DungeonMission> Missions = [];
    public DungeonBonusFlag BonusFlag = DungeonBonusFlag.None;
    public Dictionary<DungeonRewardType, int> Rewards { get; init; } = [];
    public ICollection<RewardItem> RewardItems { get; init; } = [];
    public Dictionary<DungeonRewardType, int> BonusRewards { get; init; } = [];
    public ICollection<RewardItem> BonusRewardItems { get; init; } = [];
    public int TotalSeconds;
    public int Score = -1;
    public int HighestScore = -1;
    public DungeonBonusFlag Flag = DungeonBonusFlag.None;
    public int Round;

    public DungeonUserRecord(int dungeonId, long characterId) {
        DungeonId = dungeonId;
        CharacterId = characterId;
        AccumulationRecords = [];
        var accumulationEnumList = new List<DungeonAccumulationRecordType>((DungeonAccumulationRecordType[]) System.Enum.GetValues(typeof(DungeonAccumulationRecordType)));
        foreach (DungeonAccumulationRecordType type in accumulationEnumList) {
            AccumulationRecords[type] = 0;
        }
        var rewardTypeEnumList = new List<DungeonRewardType>((DungeonRewardType[]) System.Enum.GetValues(typeof(DungeonRewardType)));
        foreach (DungeonRewardType type in rewardTypeEnumList) {
            Rewards[type] = 0;
            BonusRewards[type] = 0;
        }
    }

    public void Add(RewardRecord record) {
        BonusRewards[DungeonRewardType.Exp] += (int) record.Exp;
        BonusRewards[DungeonRewardType.Meso] += (int) record.Meso;
        BonusRewards[DungeonRewardType.Prestige] += (int) record.PrestigeExp;

        foreach (RewardItem item in record.Items) {
            BonusRewardItems.Add(item);
        }
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteBool(IsDungeonSuccess);
        writer.WriteInt(DungeonId);
        writer.WriteBool(WithParty);
        writer.WriteInt(TotalSeconds);
        writer.WriteInt(HighestScore);
        writer.WriteInt(Score);
        writer.Write<DungeonBonusFlag>(BonusFlag); // Client reads this as 4 separate bytes.
        writer.WriteInt(Rewards.Count);
        foreach ((DungeonRewardType type, int value) in Rewards) {
            writer.Write<DungeonRewardType>(type);
            writer.WriteInt(value);
        }

        writer.WriteInt(RewardItems.Count);
        foreach (RewardItem item in RewardItems) {
            writer.WriteInt(item.ItemId);
            writer.WriteInt(item.Amount);
            writer.WriteInt(item.Rarity);
            writer.WriteBool(item.Unknown1);
            writer.WriteBool(item.Unknown2);
            writer.WriteBool(item.Unknown3);
        }

        writer.WriteInt(BonusRewards.Count);
        foreach ((DungeonRewardType type, int value) in BonusRewards) {
            writer.Write<DungeonRewardType>(type);
            writer.WriteInt(value);
        }

        writer.WriteInt(BonusRewardItems.Count);
        foreach (RewardItem item in BonusRewardItems) {
            writer.WriteInt(item.ItemId);
            writer.WriteInt(item.Amount);
            writer.WriteInt(item.Rarity);
            writer.WriteBool(item.Unknown1);
            writer.WriteBool(item.Unknown2);
            writer.WriteBool(item.Unknown3);
        }
    }
}
