using Maple2.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class Nurturing {
    public long AccountId { get; set; }
    public int ItemId { get; set; }
    public long Exp { get; set; }
    public short ClaimedGiftForStage { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastFeedTime { get; set; }
    public long[] PetBy { get; set; }

    public static void Configure(EntityTypeBuilder<Nurturing> builder) {
        builder.ToTable("nurturing");
        builder.HasKey(nurturing => new {
            nurturing.AccountId,
            nurturing.ItemId,
        });

        builder.Property(nurturing => nurturing.PetBy).HasJsonConversion();
    }
}
