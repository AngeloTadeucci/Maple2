namespace Maple2.Model.Metadata;

public record MasteryDifferentialFactorTable(IReadOnlyDictionary<int, MasteryDifferentialFactorTable.Entry> Entries) : Table {
    public record Entry(
        int Differential,
        int Factor);
}


