using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class SkillTab {
    public long CharacterId { get; set; }
    public long Id { get; set; }
    public required string Name { get; set; }
    public required IDictionary<int, int> Skills;

    public DateTime CreationTime { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator SkillTab?(Maple2.Model.Game.SkillTab? other) {
        return other == null ? null : new SkillTab {
            Id = other.Id,
            Name = other.Name,
            Skills = new Dictionary<int, int>(other.Skills),
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.SkillTab?(SkillTab? other) {
        if (other == null) {
            return null;
        }

        return new Maple2.Model.Game.SkillTab(other.Name) {
            Id = other.Id,
            Skills = new Dictionary<int, int>(other.Skills),
        };
    }

    public static void Configure(EntityTypeBuilder<SkillTab> builder) {
        builder.ToTable("skill-tab");
        builder.HasKey(tab => new { tab.CharacterId, tab.Id });
        builder.HasIndex(tab => tab.CharacterId);
        builder.Property(tab => tab.Skills).HasJsonConversion().IsRequired();

        builder.HasOne<Character>()
            .WithMany()
            .HasForeignKey(tab => tab.CharacterId);

        IMutableProperty creationTime = builder.Property(club => club.CreationTime)
            .ValueGeneratedOnAdd().Metadata;
        creationTime.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
