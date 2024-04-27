using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class Account {
    public long Id { get; set; }
    public required string Username { get; set; }
    public Guid MachineId { get; set; }
    public int MaxCharacters { get; set; }
    public int PrestigeLevel { get; set; }
    public long PrestigeExp { get; set; }
    public long PremiumTime { get; set; }
    public IList<int> PremiumRewardsClaimed { get; set; } // TODO: clear list on daily reset
    public required AccountCurrency Currency { get; set; }
    public required MarketLimits MarketLimits { get; set; }

    public DateTime CreationTime { get; set; }
    public DateTime LastModified { get; set; }

    public bool Online { get; set; }

    public ICollection<Character>? Characters { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Account?(Maple2.Model.Game.Account? other) {
        if (other == null) {
            return null;
        }

        return new Account {
            LastModified = other.LastModified,
            Id = other.Id,
            Username = other.Username,
            MachineId = other.MachineId,
            MaxCharacters = other.MaxCharacters,
            PrestigeLevel = other.PrestigeLevel,
            PrestigeExp = other.PrestigeExp,
            PremiumTime = other.PremiumTime,
            PremiumRewardsClaimed = other.PremiumRewardsClaimed,
            Currency = new AccountCurrency(),
            MarketLimits = new MarketLimits {
                MesoListed = other.MesoMarketListed,
                MesoPurchased = other.MesoMarketPurchased,
            },
            Online = other.Online,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Account?(Account? other) {
        if (other == null) {
            return null;
        }

        return new Maple2.Model.Game.Account {
            LastModified = other.LastModified,
            Id = other.Id,
            Username = other.Username,
            MachineId = other.MachineId,
            MaxCharacters = other.MaxCharacters,
            PrestigeLevel = other.PrestigeLevel,
            PrestigeExp = other.PrestigeExp,
            PremiumTime = other.PremiumTime,
            PremiumRewardsClaimed = other.PremiumRewardsClaimed,
            MesoMarketListed = other.MarketLimits.MesoListed,
            MesoMarketPurchased = other.MarketLimits.MesoPurchased,
            Online = other.Online,
        };
    }

    public static void Configure(EntityTypeBuilder<Account> builder) {
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Username).IsRequired();
        builder.HasIndex(account => account.Username)
            .IsUnique();
        builder.Property(account => account.MaxCharacters)
            .HasDefaultValue(4);
        builder.HasMany(account => account.Characters);
        builder.Property(account => account.Currency).HasJsonConversion().IsRequired();
        builder.Property(account => account.MarketLimits).HasJsonConversion().IsRequired();
        builder.Property(account => account.PremiumRewardsClaimed).HasJsonConversion();

        builder.Property(account => account.LastModified).IsRowVersion();
        IMutableProperty creationTime = builder.Property(account => account.CreationTime)
            .ValueGeneratedOnAdd().Metadata;
        creationTime.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}

internal class AccountCurrency {
    public long Meret { get; set; }
    public long GameMeret { get; set; }
    public long MesoToken { get; set; }
}

internal class MarketLimits {
    public int MesoListed { get; set; }
    public int MesoPurchased { get; set; }
}

