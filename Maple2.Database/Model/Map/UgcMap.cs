using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class UgcMap {
    public long Id { get; set; }
    public long OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;
    public int MapId { get; set; }
    public bool Indoor { get; set; }
    public int Number { get; set; }
    public int ApartmentNumber { get; set; }

    // Referenced by UgcMapCube
    public ICollection<UgcMapCube>? Cubes = [];

    public DateTimeOffset ExpiryTime { get; set; }
    public DateTimeOffset LastModified { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator UgcMap?(Maple2.Model.Game.PlotInfo? other) {
        return other == null ? null : new UgcMap {
            Id = other.Id,
            OwnerId = other.OwnerId,
            MapId = other.MapId,
            Number = other.Number,
            ApartmentNumber = other.ApartmentNumber,
            ExpiryTime = other.ExpiryTime.FromEpochSeconds(),
        };
    }

    public static void Configure(EntityTypeBuilder<UgcMap> builder) {
        builder.ToTable("ugcmap");
        builder.HasKey(map => map.Id);
        builder.HasIndex(map => map.OwnerId);
        builder.HasIndex(map => map.MapId);

        builder.Property(map => map.LastModified)
            .IsRowVersion();
    }
}
