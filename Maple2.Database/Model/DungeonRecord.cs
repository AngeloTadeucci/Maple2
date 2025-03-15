using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class DungeonRecord {
    public int DungeonId { get; set; }
    public long OwnerId { get; set; }
    public DateTime ClearTime { get; init; }
    public int TotalClears { get; init; }
    public byte DailyClears { get; set; }
    public byte WeeklyClears { get; set; }
    public short LifetimeRecord { get; set; }
    public short WeeklyRecord { get; set; }
    public byte ExtraDailyClears { get; set; }
    public byte ExtraWeeklyClears { get; set; }
    public DateTime DailyResetTime { get; set; }
    public DateTime WeeklyResetTime { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator DungeonRecord?(Maple2.Model.Game.Dungeon.DungeonRecord? other) {
        return other == null ? null : new DungeonRecord {
            DungeonId = other.DungeonId,
            DailyClears = other.DailyClears,
            WeeklyClears = other.WeeklyClears,
            DailyResetTime = other.DailyResetTimestamp.FromEpochSeconds(),
            WeeklyResetTime = other.WeeklyResetTimestamp.FromEpochSeconds(),
            ClearTime = other.ClearTimestamp.FromEpochSeconds(),
            TotalClears = other.TotalClears,
            LifetimeRecord = other.LifetimeRecord,
            WeeklyRecord = other.WeeklyRecord,
            ExtraDailyClears = other.ExtraDailyClears,
            ExtraWeeklyClears = other.ExtraWeeklyClears,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Dungeon.DungeonRecord?(DungeonRecord? other) {
        return other == null ? null : new Maple2.Model.Game.Dungeon.DungeonRecord(other.DungeonId) {
            DailyClears = other.DailyClears,
            WeeklyClears = other.WeeklyClears,
            DailyResetTimestamp = other.DailyResetTime.ToEpochSeconds(),
            WeeklyResetTimestamp = other.WeeklyResetTime.ToEpochSeconds(),
            ClearTimestamp = other.ClearTime.ToEpochSeconds(),
            TotalClears = other.TotalClears,
            LifetimeRecord = other.LifetimeRecord,
            WeeklyRecord = other.WeeklyRecord,
            ExtraDailyClears = other.ExtraDailyClears,
            ExtraWeeklyClears = other.ExtraWeeklyClears,
        };
    }

    public static void Configure(EntityTypeBuilder<DungeonRecord> builder) {
        builder.ToTable("dungeon-record");
        builder.HasKey(record => new { record.OwnerId, record.DungeonId });
        builder.HasOne<Character>()
            .WithMany()
            .HasForeignKey(record => record.OwnerId);

    }
}
