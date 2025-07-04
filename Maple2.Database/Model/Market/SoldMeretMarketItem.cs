using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class SoldMeretMarketItem {
    public long Id { get; set; }
    public long MarketId { get; set; }
    public long Price { get; set; }
    public long CharacterId { get; set; }
    public DateTime SoldTime { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator SoldMeretMarketItem?(Maple2.Model.Game.PremiumMarketItem? other) {
        return other == null ? null : new SoldMeretMarketItem {
            MarketId = other.Id,
            Price = other.Metadata.SalePrice > 0 ? other.Metadata.SalePrice : other.Price,
        };
    }

    public static void Configure(EntityTypeBuilder<SoldMeretMarketItem> builder) {
        builder.ToTable("meret-market-sold");
        builder.HasKey(listing => listing.Id);

        builder.Property(listing => listing.SoldTime).ValueGeneratedOnAdd();
        IMutableProperty soldTime = builder.Property(listing => listing.SoldTime)
            .ValueGeneratedOnAdd().Metadata;
        soldTime.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
