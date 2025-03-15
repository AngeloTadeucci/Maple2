using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game.Dungeon;

public class DungeonRecord : IByteSerializable {
    public readonly int DungeonId;
    public byte DailyClears { get; set; }
    public byte WeeklyClears { get; set; }
    public long DailyResetTimestamp { get; set; }
    public long WeeklyResetTimestamp { get; set; }
    public long ClearTimestamp { get; init; }
    public int TotalClears { get; init; }
    public short LifetimeRecord { get; set; }
    public short WeeklyRecord { get; set; }
    public byte ExtraDailyClears { get; set; }
    public byte ExtraWeeklyClears { get; set; }

    public DungeonRecord(int dungeonId) {
        DungeonId = dungeonId;
        LifetimeRecord = -1;
        WeeklyRecord = -1;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteInt(DungeonId);
        writer.WriteLong(WeeklyResetTimestamp);
        writer.WriteByte(WeeklyClears);
        writer.WriteByte(DailyClears);
        writer.WriteLong(DailyResetTimestamp);
        writer.WriteByte(ExtraDailyClears);
        writer.WriteByte(ExtraWeeklyClears);
        writer.WriteLong(ClearTimestamp);
        writer.WriteInt(TotalClears);
        writer.WriteShort(LifetimeRecord);
        writer.WriteLong(WeeklyResetTimestamp); // Reusing it here.
        writer.WriteShort(WeeklyClears);
        writer.WriteByte();
    }
}
