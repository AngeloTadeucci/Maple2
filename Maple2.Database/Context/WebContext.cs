using Maple2.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Database.Context;

public sealed class WebContext(DbContextOptions options) : DbContext(options) {
    internal DbSet<UgcResource> UgcResource { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UgcResource>(Maple2.Database.Model.UgcResource.Configure);
    }
}
