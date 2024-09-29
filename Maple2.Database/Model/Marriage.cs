using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class Marriage {
    public long Id { get; set; }
    public long Partner1Id { get; set; }
    public long Partner2Id { get; set; }
    public MaritalStatus Status { get; set; }
    public IList<MarriageExp> ExpHistory { get; set; } = new List<MarriageExp>();
    public required string Profile { get; set; }
    public required string Partner1Message { get; set; }
    public required string Partner2Message { get; set; }
    public DateTime CreationTime { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Marriage?(Maple2.Model.Game.Marriage? other) {
        return other == null ? null : new Marriage {
            Partner1Id = other.Partner1.CharacterId,
            Partner2Id = other.Partner2.CharacterId,
            Status = other.Status,
            Profile = other.Profile,
            Partner1Message = other.Partner1.Message,
            Partner2Message = other.Partner2.Message,
            CreationTime = other.CreationTime.FromEpochSeconds(),
        };
    }

    public static void Configure(EntityTypeBuilder<Marriage> builder) {
        builder.ToTable("marriage");
        builder.HasKey(marriage => marriage.Id);
        builder.Property(marriage => marriage.ExpHistory).HasJsonConversion().IsRequired();

        builder.HasOne<Character>()
            .WithMany()
            .HasForeignKey(marriage => marriage.Partner1Id)
            .IsRequired();
        builder.HasOne<Character>()
            .WithMany()
            .HasForeignKey(marriage => marriage.Partner2Id)
            .IsRequired();
        builder.Property(marriage => marriage.CreationTime)
            .ValueGeneratedOnAdd();
    }
}

internal class MarriageExp {
    public MarriageExpType Type { get; set; }
    public long Amount { get; set; }
    public DateTime Time { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator MarriageExp?(Maple2.Model.Game.MarriageExp? other) {
        return other == null ? null : new MarriageExp {
            Type = other.Type,
            Amount = other.Amount,
            Time = other.Time.FromEpochSeconds(),
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.MarriageExp?(MarriageExp? other) {
        if (other == null) {
            return null;
        }

        return new Maple2.Model.Game.MarriageExp {
            Type = other.Type,
            Amount = other.Amount,
            Time = other.Time.ToEpochSeconds(),
        };
    }
}
