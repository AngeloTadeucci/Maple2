using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;

namespace Maple2.Model.Game.Dungeon;

public class MiniGameUserRecord : IUserContentRecord {
    public long CharacterId { get; init; }
    public Dictionary<DungeonRewardType, int> Rewards { get; init; } = [];
    public ICollection<RewardItem> RewardItems { get; init; } = [];
    public int ClearedRounds { get; set; }
    public required int MinRound { get; init; }
    public required int TotalRounds { get; init; }
    public required bool ShowResult { get; init; }

    public MiniGameUserRecord(long characterId) {
        CharacterId = characterId;
        var enumList = new List<DungeonRewardType>((DungeonRewardType[]) System.Enum.GetValues(typeof(DungeonRewardType)));
        foreach (DungeonRewardType type in enumList) {
            Rewards[type] = 0;
        }
    }

    public void Add(RewardRecord record) {
        Rewards[DungeonRewardType.Exp] += (int) record.Exp;
        Rewards[DungeonRewardType.Meso] += (int) record.Meso;
        Rewards[DungeonRewardType.Prestige] += (int) record.PrestigeExp;

        if (record.Items != null) {
            foreach (RewardItem item in record.Items) {
                RewardItems.Add(item);
            }
        }
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(ClearedRounds);
        writer.WriteInt(TotalRounds);

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
    }
}
