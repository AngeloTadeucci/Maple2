using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class GuildApplication {
    public long Id { get; set; }
    public long GuildId { get; set; }
    public long ApplicantId { get; set; }

    public DateTime CreationTime { get; set; }

    public static void Configure(EntityTypeBuilder<GuildApplication> builder) {
        builder.ToTable("guild-application");
        builder.HasKey(app => app.Id);
        builder.HasOne<Guild>()
            .WithMany()
            .HasForeignKey(app => app.GuildId);
        builder.HasOne<Character>()
            .WithMany()
            .HasForeignKey(app => app.ApplicantId);

        IMutableProperty creationTime = builder.Property(app => app.CreationTime)
            .ValueGeneratedOnAdd().Metadata;
        creationTime.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
