using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class WeddingHall {
    public long Id { get; set; }
    public long MarriageId { get; set; }
    public DateTime CeremonyTime { get; set; }
    public int PackageId { get; set; }
    public int PackageHallId { get; set; }
    public long OwnerId { get; set; }
    public bool Public { get; set; }
    public Dictionary<long, long> GuestList { get; set; } = new(); // CharacterId, AccountId
    public DateTime CreationTime { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator WeddingHall?(Maple2.Model.Game.WeddingHall? other) {
        return other == null ? null : new WeddingHall {
            Id = other.Id,
            MarriageId = other.MarriageId,
            CeremonyTime = other.CeremonyTime.FromEpochSeconds(),
            PackageId = other.PackageId,
            PackageHallId = other.PackageHallId,
            Public = other.Public,
            CreationTime = other.CreationTime.FromEpochSeconds(),
            GuestList = other.GuestList,
            OwnerId = other.ReserverCharacterId,
        };
    }

    // Use explicit Convert() here because we need marriage data to construct Wedding Hall.
    public Maple2.Model.Game.WeddingHall Convert(Maple2.Model.Game.Marriage marriage) {
        Maple2.Model.Game.MarriagePartner reserver = marriage.Partner1.Info!.CharacterId == OwnerId ? marriage.Partner1 : marriage.Partner2;
        Maple2.Model.Game.MarriagePartner partner = marriage.Partner1.Info!.CharacterId == OwnerId ? marriage.Partner2 : marriage.Partner1;
        var hall = new Maple2.Model.Game.WeddingHall {
            Id = Id,
            MarriageId = MarriageId,
            CeremonyTime = CeremonyTime.ToEpochSeconds(),
            PackageId = PackageId,
            PackageHallId = PackageHallId,
            Public = Public,
            ReserverCharacterId = reserver.CharacterId,
            ReserverAccountId = reserver.AccountId,
            ReserverName = reserver.Info!.Name,
            PartnerName = partner.Info!.Name,
            CreationTime = CreationTime.ToEpochSeconds(),
            GuestList = GuestList,
        };

        return hall;
    }

    public static void Configure(EntityTypeBuilder<WeddingHall> builder) {
        builder.ToTable("wedding-hall");
        builder.HasKey(hall => hall.Id);
        builder.Property(hall => hall.GuestList).HasJsonConversion().IsRequired();

        builder.OneToOne<WeddingHall, Marriage>()
            .HasForeignKey<WeddingHall>(hall => hall.MarriageId);
        builder.Property(hall => hall.CreationTime)
            .ValueGeneratedOnAdd();
    }
}
