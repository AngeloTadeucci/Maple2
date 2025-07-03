namespace Maple2.Model.Metadata;

public record DungeonRankRewardTable(IReadOnlyDictionary<int, DungeonRankRewardTable.Entry> Entries) : Table {
    public record Entry(
        int Id,
        Entry.Item[] Items
    ) {
        public record Item(
            int Rank,
            int ItemId,
            int SystemMailId
        );
    }
}
