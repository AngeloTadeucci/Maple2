using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Dungeon;

public class DungeonRecord : IByteSerializable {
    public readonly int DungeonId;
    public byte UnionSubClears { get; set; }
    public byte UnionClears { get; set; }
    public long UnionSubCooldownTimestamp { get; set; }
    public long UnionCooldownTimestamp { get; set; }
    public long CooldownTimestamp { get; set; }
    public long ClearTimestamp { get; set; }
    public int TotalClears { get; set; }
    public short LifetimeRecord { get; set; }
    public short CurrentRecord { get; set; }
    public byte ExtraSubClears { get; set; }
    public byte ExtraClears { get; set; }
    public DungeonRecordFlag Flag { get; set; }

    public DungeonRecord(int dungeonId) {
        DungeonId = dungeonId;
        LifetimeRecord = -1;
        CurrentRecord = -1;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(DungeonId);
        writer.WriteLong(UnionCooldownTimestamp);
        writer.WriteByte(UnionClears);
        writer.WriteByte(UnionSubClears);
        writer.WriteLong(UnionSubCooldownTimestamp);
        writer.WriteByte(ExtraSubClears);
        writer.WriteByte(ExtraClears);
        writer.WriteLong(ClearTimestamp);
        writer.WriteInt(TotalClears);
        writer.WriteShort(LifetimeRecord);
        writer.WriteLong(CooldownTimestamp);
        writer.WriteShort(CurrentRecord);
        writer.Write<DungeonRecordFlag>(Flag);
    }
}
