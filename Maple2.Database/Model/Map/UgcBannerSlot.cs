using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class BannerSlot {
    public long Id { get; set; }
    public long BannerId { get; set; }
    public DateTimeOffset ActivateTime { get; set; }

    public UgcItemLook? Template { get; set; }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator BannerSlot?(Maple2.Model.Game.Ugc.BannerSlot? other) {
        return other == null ? null : new BannerSlot {
            Id = other.Id,
            ActivateTime = other.ActivateTime,
            BannerId = other.BannerId,
            Template = other.Template,
        };
    }

    [return: NotNullIfNotNull(nameof(other))]
    public static implicit operator Maple2.Model.Game.Ugc.BannerSlot?(BannerSlot? other) {
        return other == null ? null : new Maple2.Model.Game.Ugc.BannerSlot(other.Id, other.ActivateTime, other.BannerId, other.Template);
    }

    public static void Configure(EntityTypeBuilder<BannerSlot> builder) {
        builder.ToTable("ugc-banner-slot");
        builder.HasKey(slot => slot.Id);

        builder.HasIndex(slot => slot.BannerId);

        builder.Property(slot => slot.Template).HasJsonConversion();
    }
}
