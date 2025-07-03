namespace Maple2.Model.Metadata;

public record UgcHousingPointRewardTable(IReadOnlyDictionary<int, UgcHousingPointRewardTable.Entry> Entries) : Table {
    public record Entry(
        int DecorationScore,
        int IndividualDropBoxId);
}
