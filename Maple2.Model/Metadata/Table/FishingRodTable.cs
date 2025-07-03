namespace Maple2.Model.Metadata;

public record FishingRodTable(IReadOnlyDictionary<int, FishingRodTable.Entry> Entries) : Table {
    public record Entry(
        int ItemId,
        int MinMastery,
        int AddMastery,
        int ReduceTime);
}
