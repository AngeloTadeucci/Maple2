namespace Maple2.Model.Metadata;

public record BannerTable(List<BannerTable.Entry> Entries) : Table {
    public record Entry(
        long Id,
        int MapId,
        List<long> Price);
}
