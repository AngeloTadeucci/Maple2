using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Model.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class PetConfig {
    public long ItemUid { get; set; }

    public required PetPotionConfig[] PotionConfigs { get; set; }
    public PetLootConfig LootConfig { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator PetConfig?(Maple2.Model.Game.PetConfig? other) {
        return other == null ? null : new PetConfig {
            PotionConfigs = other.PotionConfig,
            LootConfig = other.LootConfig,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.PetConfig?(PetConfig? other) {
        return other == null ? null : new Maple2.Model.Game.PetConfig(other.PotionConfigs, other.LootConfig);
    }

    public static void Configure(EntityTypeBuilder<PetConfig> builder) {
        builder.ToTable("pet-config");
        builder.HasKey(config => config.ItemUid);
        builder.OneToOne<PetConfig, Item>()
            .HasForeignKey<PetConfig>(config => config.ItemUid);

        builder.Property(character => character.PotionConfigs).HasJsonConversion().IsRequired();
        builder.Property(character => character.LootConfig).HasJsonConversion().IsRequired();
    }
}
