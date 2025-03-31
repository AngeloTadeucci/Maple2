using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class DungeonRecord {
    public int DungeonId { get; set; }
    public long OwnerId { get; set; }
    public DateTime ClearTime { get; init; }
    public int TotalClears { get; init; }
    public byte CurrentSubClears { get; set; }
    public byte CurrentClears { get; set; }
    public short LifetimeRecord { get; set; }
    public short CurrentRecord { get; set; }
    public byte ExtraCurrentSubClears { get; set; }
    public byte ExtraCurrentClears { get; set; }
    public DateTime DailyResetTime { get; set; }
    public DateTime UnionCooldownTime { get; set; }
    public DateTime CooldownTime { get; set; }
    public DungeonRecordFlag Flag { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator DungeonRecord?(Maple2.Model.Game.Dungeon.DungeonRecord? other) {
        return other == null ? null : new DungeonRecord {
            DungeonId = other.DungeonId,
            CurrentSubClears = other.UnionSubClears,
            CurrentClears = other.UnionClears,
            DailyResetTime = other.UnionSubCooldownTimestamp.FromEpochSeconds(),
            UnionCooldownTime = other.UnionCooldownTimestamp.FromEpochSeconds(),
            ClearTime = other.ClearTimestamp.FromEpochSeconds(),
            CooldownTime = other.CooldownTimestamp.FromEpochSeconds(),
            TotalClears = other.TotalClears,
            LifetimeRecord = other.LifetimeRecord,
            CurrentRecord = other.CurrentRecord,
            ExtraCurrentSubClears = other.ExtraSubClears,
            ExtraCurrentClears = other.ExtraClears,
            Flag = other.Flag,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Dungeon.DungeonRecord?(DungeonRecord? other) {
        return other == null ? null : new Maple2.Model.Game.Dungeon.DungeonRecord(other.DungeonId) {
            UnionSubClears = other.CurrentSubClears,
            UnionClears = other.CurrentClears,
            UnionSubCooldownTimestamp = other.DailyResetTime.ToEpochSeconds(),
            UnionCooldownTimestamp = other.UnionCooldownTime.ToEpochSeconds(),
            ClearTimestamp = other.ClearTime.ToEpochSeconds(),
            CooldownTimestamp = other.CooldownTime.ToEpochSeconds(),
            TotalClears = other.TotalClears,
            LifetimeRecord = other.LifetimeRecord,
            CurrentRecord = other.CurrentRecord,
            ExtraSubClears = other.ExtraCurrentSubClears,
            ExtraClears = other.ExtraCurrentClears,
            Flag = other.Flag,
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
