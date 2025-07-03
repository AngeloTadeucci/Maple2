using Maple2.Model.Enum;
using Maple2.Tools;

namespace Maple2.Model.Game.Dungeon;

public interface IUserContentRecord : IByteSerializable {
    public long CharacterId { get; }
    public Dictionary<DungeonRewardType, int> Rewards { get; }
    public ICollection<RewardItem> RewardItems { get; }
}
