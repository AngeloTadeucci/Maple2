using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class Medal {
    public int Id { get; set; }
    public long OwnerId { get; set; }
    public short Slot { get; set; } = -1;
    public DateTime ExpiryTime { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Medal?(Maple2.Model.Game.Medal? other) {
        return other == null ? null : new Medal {
            Id = other.Id,
            Slot = other.Slot,
            ExpiryTime = other.ExpiryTime.FromEpochSeconds(),
        };
    }

    // Use explicit Convert() here because we need medal type to construct the medal.
    public Maple2.Model.Game.Medal Convert(MedalType type) {
        var medal = new Maple2.Model.Game.Medal(Id, type) {
            Slot = Slot,
            ExpiryTime = ExpiryTime.ToEpochSeconds(),
        };

        return medal;
    }

    public static void Configure(EntityTypeBuilder<Medal> builder) {
        builder.ToTable("medal");
        builder.HasKey(medal => new { medal.OwnerId, medal.Id });

    }
}
