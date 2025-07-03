using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model.Metadata;

[Table("schema_version")]
public class SchemaVersion {
    public int Id { get; set; }
    public string SchemaHash { get; set; } = null!;
    public DateTime UpdatedAt { get; set; }

    internal static void Configure(EntityTypeBuilder<SchemaVersion> builder) {
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.UpdatedAt).IsRowVersion();
    }
}
