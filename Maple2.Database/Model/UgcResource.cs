using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class UgcResource {
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public string Path { get; set; } = string.Empty;
    public UgcType Type { get; set; }

    public DateTime LastModified { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.UgcResource?(UgcResource? other) {
        return other == null ? null : new Maple2.Model.Game.UgcResource {
            Id = other.Id,
            Path = other.Path,
            Type = other.Type,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator UgcResource?(Maple2.Model.Game.UgcResource? other) {
        return other == null ? null : new UgcResource {
            Id = other.Id,
            Path = other.Path,
            Type = other.Type,
        };
    }

    public static void Configure(EntityTypeBuilder<UgcResource> builder) {
        builder.ToTable("ugcresource");
        builder.HasKey(ugc => ugc.Id);
        builder.HasIndex(ugc => ugc.OwnerId);

        builder.Property(ugc => ugc.LastModified)
            .IsRowVersion();
    }
}
