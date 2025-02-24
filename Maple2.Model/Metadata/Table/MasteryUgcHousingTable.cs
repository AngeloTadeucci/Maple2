namespace Maple2.Model.Metadata;

public record MasteryUgcHousingTable(IReadOnlyDictionary<int, MasteryUgcHousingTable.Entry> Entries) : Table {
    public record Entry(
        int Level,
        int Exp,
        int RewardJobItemId);
}
