using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

public class Ban {
    public long Id { get; set; }
    public long AccountId { get; set; } // Always present now
    public string? IpAddress { get; set; }
    public Guid? MachineId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public static void Configure(EntityTypeBuilder<Ban> builder) {
        builder.ToTable("ban");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.AccountId).IsRequired();
        builder.Property(b => b.Reason).HasMaxLength(255).IsRequired();
        builder.Property(b => b.IpAddress).HasMaxLength(45);
        builder.Property(b => b.Details);

        builder.HasIndex(b => b.AccountId);
        builder.HasIndex(b => b.IpAddress);
        builder.HasIndex(b => b.MachineId);
        builder.HasIndex(b => b.ExpiresAt);

        IMutableProperty createdAt = builder.Property(b => b.CreatedAt).ValueGeneratedOnAdd().Metadata;
        createdAt.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
