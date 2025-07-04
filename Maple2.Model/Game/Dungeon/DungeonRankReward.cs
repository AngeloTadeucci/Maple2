using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Dungeon;

public class DungeonRankReward : IByteSerializable {
    public readonly int Id;
    public int RankClaimed { get; set; }
    public long UpdatedTimestamp { get; set; }

    public DungeonRankReward(int id) {
        Id = id;
    }
    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(Id);
        writer.WriteInt(RankClaimed);
        writer.WriteLong(UpdatedTimestamp);
    }
}
