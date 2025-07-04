using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model.Metadata;

[Table("checksum")]
public class TableChecksum {

    public required string TableName { get; set; }

    public uint Crc32C { get; set; }

    public DateTime LastModified { get; set; }

    internal static void Configure(EntityTypeBuilder<TableChecksum> builder) {
        builder.HasKey(entry => entry.TableName);
        builder.Property(entry => entry.LastModified).IsRowVersion();
    }
}
