using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class Club {
    public long Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastModified { get; set; }

    public long LeaderId { get; set; }
    public List<ClubMember>? Members { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Club?(Maple2.Model.Game.Club? other) {
        return other == null ? null : new Club {
            // CreationTime set by DB
            LastModified = other.LastModified,
            Id = other.Id,
            Name = other.Name,
            LeaderId = other.Leader.Info.CharacterId,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Club?(Club? other) {
        return other == null ? null : new Maple2.Model.Game.Club {
            LastModified = other.LastModified,
            CreationTime = other.CreationTime.ToEpochSeconds(),
            Id = other.Id,
            Name = other.Name,
            // Leader and Members set separately
        };
    }

    public static void Configure(EntityTypeBuilder<Club> builder) {
        builder.HasKey(club => club.Id);
        builder.HasIndex(club => club.Name).IsUnique();

        builder.HasOne<Character>()
            .WithMany()
            .HasForeignKey(club => club.LeaderId)
            .IsRequired();
        builder.HasMany<ClubMember>(club => club.Members);

        builder.Property(club => club.LastModified).IsRowVersion();
        IMutableProperty creationTime = builder.Property(club => club.CreationTime)
            .ValueGeneratedOnAdd().Metadata;
        creationTime.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}

internal class ClubMember {
    public DateTime CreationTime { get; set; }

    public long ClubId { get; set; }
    public long CharacterId { get; set; }
    public Character? Character { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator ClubMember?(Maple2.Model.Game.ClubMember? other) {
        return other == null ? null : new ClubMember {
            // CreationTime set by DB
            // ClubId set by Club auto_increment
            CharacterId = other.Info.CharacterId,
        };
    }

    public static void Configure(EntityTypeBuilder<ClubMember> builder) {
        builder.ToTable("club-member");
        builder.HasKey(member => new { member.ClubId, member.CharacterId });

        builder.HasOne<Character>(member => member.Character)
            .WithMany()
            .HasForeignKey(member => member.CharacterId);
        builder.HasOne<Club>()
            .WithMany(club => club.Members)
            .HasForeignKey(member => member.ClubId);

        IMutableProperty creationTime = builder.Property(member => member.CreationTime)
            .ValueGeneratedOnAdd().Metadata;
        creationTime.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
