using System.Collections.Generic;

namespace Maple2.Model.Metadata;

public record BannerTable(IReadOnlyDictionary<long, BannerTable.Entry> Entries) : Table {
    public record Entry(
        long Id,
        int MapId,
        List<long> Price) { }
}
