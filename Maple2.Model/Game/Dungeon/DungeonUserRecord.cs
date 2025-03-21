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
    public DungeonBonusFlag BonusFlag = DungeonBonusFlag.None;
    public DungeonBonusFlag2 BonusFlag2 = DungeonBonusFlag2.None;
    public Dictionary<DungeonRewardType, int> Rewards { get; init; } = [];
    public ICollection<RewardItem> RewardItems { get; init; } = [];
    public Dictionary<DungeonRewardType, int> BonusRewards { get; init; } = [];
    public ICollection<RewardItem> BonusRewardItems { get; init; } = [];
    public int TotalSeconds;
    public int Score = -1;
    public int HighestScore = -1;

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
        }
    }

    public void Add(RewardRecord record) {
        Rewards[DungeonRewardType.Exp] += (int) record.Exp;
        Rewards[DungeonRewardType.Meso] += (int) record.Meso;
        Rewards[DungeonRewardType.Prestige] += (int) record.PrestigeExp;

        foreach (RewardItem item in record.Items) {
            RewardItems.Add(item);
        }
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteBool(IsDungeonSuccess);
        writer.WriteInt(DungeonId);
        writer.WriteBool(WithParty);
        writer.WriteInt(TotalSeconds);
        writer.WriteInt(Score);
        writer.WriteInt(HighestScore);
        writer.Write<DungeonBonusFlag>(BonusFlag);
        writer.Write<DungeonBonusFlag2>(BonusFlag2);
        writer.WriteByte();
        writer.WriteByte();

        writer.WriteInt(Rewards.Count);
        foreach ((DungeonRewardType type, int value) in Rewards) {
            writer.Write<DungeonRewardType>(type);
            writer.WriteInt(value);
        }

        writer.WriteInt(RewardItems.Count);
        foreach (RewardItem item in RewardItems) {
            writer.WriteInt(item.ItemId);
            writer.WriteInt(item.Rarity);
            writer.WriteInt(item.Amount);
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
            writer.WriteInt(item.Rarity);
            writer.WriteInt(item.Amount);
            writer.WriteBool(item.Unknown1);
            writer.WriteBool(item.Unknown2);
            writer.WriteBool(item.Unknown3);
        }
    }
}
