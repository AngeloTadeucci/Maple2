using Maple2.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class ItemStorage {
    public long AccountId { get; set; }

    public long Meso { get; set; }
    public short Expand { get; set; }

    public static void Configure(EntityTypeBuilder<ItemStorage> builder) {
        builder.ToTable("item-storage");
        builder.HasKey(storage => storage.AccountId);
        builder.OneToOne<ItemStorage, Account>()
            .HasForeignKey<ItemStorage>(storage => storage.AccountId);
    }
}
