using System;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Model.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model.Shop;

internal class CharacterShopData {
    public int ShopId { get; set; }
    public long OwnerId { get; set; }
    public DateTime RestockTime { get; set; }
    public int RestockCount { get; set; }
    public ResetType Interval { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator CharacterShopData?(Maple2.Model.Game.Shop.CharacterShopData? other) {
        return other == null ? null : new CharacterShopData {
            ShopId = other.ShopId,
            RestockTime = other.RestockTime.FromEpochSeconds(),
            RestockCount = other.RestockCount,
            Interval = other.Interval,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Shop.CharacterShopData?(CharacterShopData? other) {
        return other == null ? null : new Maple2.Model.Game.Shop.CharacterShopData {
            ShopId = other.ShopId,
            RestockTime = other.RestockTime.ToEpochSeconds(),
            RestockCount = other.RestockCount,
            Interval = other.Interval,
        };
    }

    public static void Configure(EntityTypeBuilder<CharacterShopData> builder) {
        builder.ToTable("character-shop-data");
        builder.HasKey(info => new { info.ShopId, info.OwnerId });
    }
}
