using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class Achievement {
    public long OwnerId { get; set; }
    public int Id { get; set; }

    public int CompletedCount { get; set; }
    public int CurrentGrade { get; set; }
    public int RewardGrade { get; set; }
    public bool Favorite { get; set; }
    public long Counter { get; set; }
    public AchievementCategory Category { get; set; }
    public required IDictionary<int, long> Grades { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Achievement?(Maple2.Model.Game.Achievement? other) {
        if (other == null) {
            return null;
        }
        return new Achievement {
            Id = other.Id,
            CompletedCount = other.Grades.Count,
            CurrentGrade = other.CurrentGrade,
            RewardGrade = other.RewardGrade,
            Favorite = other.Favorite,
            Counter = other.Counter,
            Category = other.Category,
            Grades = other.Grades,
        };
    }

    // Use explicit Convert() here because we need metadata to construct Achievement.
    public Maple2.Model.Game.Achievement Convert(AchievementMetadata metadata) {
        return new Maple2.Model.Game.Achievement(metadata) {
            CurrentGrade = CurrentGrade,
            RewardGrade = RewardGrade,
            Favorite = Favorite,
            Counter = Counter,
            Category = Category,
            Grades = Grades,
        };
    }

    public static void Configure(EntityTypeBuilder<Achievement> builder) {
        builder.ToTable("achievement");
        builder.HasKey(achieve => new { achieve.OwnerId, achieve.Id });
        builder.Property(achieve => achieve.Grades).HasJsonConversion().IsRequired();
    }
}
