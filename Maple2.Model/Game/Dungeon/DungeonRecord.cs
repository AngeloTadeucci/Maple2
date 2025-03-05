using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Dungeon;

public class DungeonRecord : IByteSerializable {
    public readonly int DungeonId;
    public long ResetTimestamp { get; init; }
    public long ClearTimestamp { get; init; }
    public int ClearCount { get; init; }
    public short LifetimeRecord { get; init; }
    public short WeeklyRecord { get; init; }

    public DungeonRecord(int dungeonId) {
        DungeonId = dungeonId;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(DungeonId);
        writer.WriteLong(ResetTimestamp);
        writer.WriteByte();
        writer.WriteByte();
        writer.WriteLong(); // Timestamp
        writer.WriteByte();
        writer.WriteByte();
        writer.WriteLong(ClearTimestamp);
        writer.WriteInt(ClearCount);
        writer.WriteShort(LifetimeRecord);
        writer.WriteShort(WeeklyRecord);
        writer.WriteLong(); // Timestamp
        writer.WriteByte();
    }
}
