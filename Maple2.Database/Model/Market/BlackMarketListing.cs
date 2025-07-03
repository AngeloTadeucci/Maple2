using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class BlackMarketListing {
    public long Id { get; set; }
    public long ItemUid { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime ExpiryTime { get; set; }
    public long Price { get; set; }
    public int Quantity { get; set; }
    public long AccountId { get; set; }
    public long CharacterId { get; set; }
    public long Deposit { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator BlackMarketListing?(Maple2.Model.Game.BlackMarketListing? other) {
        return other == null ? null : new BlackMarketListing {
            Id = other.Id,
            ExpiryTime = other.ExpiryTime.FromEpochSeconds(),
            ItemUid = other.Item.Uid,
            Price = other.Price,
            Quantity = other.Quantity,
            CharacterId = other.CharacterId,
            AccountId = other.AccountId,
            Deposit = other.Deposit,
            CreationTime = other.CreationTime.FromEpochSeconds(),
        };
    }

    public Maple2.Model.Game.BlackMarketListing Convert(Maple2.Model.Game.Item item) {
        var entry = new Maple2.Model.Game.BlackMarketListing(item) {
            Id = Id,
            ExpiryTime = ExpiryTime.ToEpochSeconds(),
            Price = Price,
            Quantity = Quantity,
            CharacterId = CharacterId,
            AccountId = AccountId,
            Deposit = Deposit,
            CreationTime = CreationTime.ToEpochSeconds(),
        };

        return entry;
    }

    public static void Configure(EntityTypeBuilder<BlackMarketListing> builder) {
        builder.ToTable("black-market-listing");
        IMutableProperty creationTime = builder.Property(listing => listing.CreationTime)
            .ValueGeneratedOnAdd().Metadata;
        creationTime.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
