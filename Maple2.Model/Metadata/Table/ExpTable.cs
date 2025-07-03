using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record ExpTable(
    IReadOnlyDictionary<int, IReadOnlyDictionary<int, long>> ExpBase,
    IReadOnlyDictionary<int, long> NextExp) : Table;

public record CommonExpTable(
    IReadOnlyDictionary<ExpType, CommonExpTable.Entry> Entries) : Table {
    public record Entry(
        int ExpTableId,
        float Factor);
}
