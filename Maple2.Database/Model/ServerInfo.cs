using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maple2.Database.Model;

internal class ServerInfo {
    public required string Key { get; set; }
    public DateTime LastModified { get; set; }

    public static void Configure(EntityTypeBuilder<ServerInfo> builder) {
        builder.ToTable("server-info");
        builder.HasKey(info => info.Key);
    }
}
