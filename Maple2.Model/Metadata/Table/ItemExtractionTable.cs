namespace Maple2.Model.Metadata;

public record ItemExtractionTable(IReadOnlyDictionary<int, ItemExtractionTable.Entry> Entries) : Table {
    public record Entry(
        int TargetItemId,
        int TryCount,
        int ScrollCount,
        int ResultItemId);
}
