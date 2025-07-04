namespace Maple2.Model.Metadata;

public record OxQuizTable(IReadOnlyDictionary<int, OxQuizTable.Entry> Entries) : ServerTable {
    public record Entry(
        int Id,
        int CategoryId,
        string Category,
        string Question,
        int Level,
        bool IsTrue,
        string Answer);
}
