using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class MesoListingBase {
    public long Id { get; set; }
    public long AccountId { get; set; }
    public long CharacterId { get; set; }
    public long Price { get; set; }
    public long Amount { get; set; }

    public DateTime LastModified { get; set; }
}

internal class MesoListing : MesoListingBase {
    public DateTime CreationTime { get; set; }
    public DateTime ExpiryTime { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator MesoListing?(Maple2.Model.Game.MesoListing? other) {
        return other == null ? null : new MesoListing {
            Id = other.Id,
            AccountId = other.AccountId,
            CharacterId = other.CharacterId,
            Price = other.Price,
            Amount = other.Amount,
            ExpiryTime = other.ExpiryTime.FromEpochSeconds(),
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.MesoListing?(MesoListing? other) {
        return other == null ? null : new Maple2.Model.Game.MesoListing {
            Id = other.Id,
            AccountId = other.AccountId,
            CharacterId = other.CharacterId,
            Price = other.Price,
            Amount = other.Amount,
            CreationTime = other.CreationTime.ToEpochSeconds(),
            ExpiryTime = other.ExpiryTime.ToEpochSeconds(),
        };
    }

    public static void Configure(EntityTypeBuilder<MesoListing> builder) {
        builder.ToTable("meso-market");
        builder.HasKey(listing => listing.Id);
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(listing => listing.AccountId)
            .IsRequired();
        builder.HasOne<Character>()
            .WithMany()
            .HasForeignKey(listing => listing.CharacterId)
            .IsRequired();

        builder.Property(listing => listing.LastModified).IsRowVersion();
        IMutableProperty creationTime = builder.Property(listing => listing.CreationTime)
            .ValueGeneratedOnAdd().Metadata;
        creationTime.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}

internal class SoldMesoListing : MesoListingBase {
    public DateTime ListedTime { get; set; }
    public DateTime SoldTime { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator SoldMesoListing?(MesoListing? other) {
        return other == null ? null : new SoldMesoListing {
            Id = other.Id,
            AccountId = other.AccountId,
            CharacterId = other.CharacterId,
            Price = other.Price,
            Amount = other.Amount,
            ListedTime = other.CreationTime,
            LastModified = other.LastModified,
        };
    }

    public static void Configure(EntityTypeBuilder<SoldMesoListing> builder) {
        builder.ToTable("meso-market-sold");
        builder.HasKey(listing => listing.Id);

        builder.Property(listing => listing.LastModified).ValueGeneratedOnAdd();
        IMutableProperty soldTime = builder.Property(listing => listing.SoldTime)
            .ValueGeneratedOnAdd().Metadata;
        soldTime.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
