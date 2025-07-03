using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class Buddy {
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public long BuddyId { get; set; }
    public Character? BuddyCharacter { get; set; }
    public BuddyType Type { get; set; }
    public required string Message { get; set; }

    public DateTime LastModified { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Buddy?(Maple2.Model.Game.BuddyEntry? other) {
        return other == null ? null : new Buddy {
            Id = other.Id,
            OwnerId = other.OwnerId,
            BuddyId = other.BuddyId,
            Type = other.Type,
            Message = other.Message,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.BuddyEntry?(Buddy? other) {
        return other == null ? null : new Maple2.Model.Game.BuddyEntry {
            Id = other.Id,
            OwnerId = other.OwnerId,
            BuddyId = other.BuddyId,
            LastModified = other.LastModified.ToEpochSeconds(),
            Type = other.Type,
            Message = other.Message,
        };
    }

    public static void Configure(EntityTypeBuilder<Buddy> builder) {
        builder.ToTable("buddy");
        builder.HasKey(buddy => buddy.Id);

        builder.HasOne<Character>()
            .WithMany()
            .HasForeignKey(buddy => buddy.OwnerId)
            .IsRequired();
        builder.HasOne<Character>(buddy => buddy.BuddyCharacter)
            .WithMany()
            .HasForeignKey(buddy => buddy.BuddyId)
            .IsRequired();

        builder.Property(buddy => buddy.LastModified)
            .ValueGeneratedOnAddOrUpdate();
    }
}
