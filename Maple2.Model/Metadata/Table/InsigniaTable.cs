using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record InsigniaTable(IReadOnlyDictionary<int, InsigniaTable.Entry> Entries) : Table {
    public record Entry(
        InsigniaConditionType Type,
        int Code,
        int BuffId,
        short BuffLevel);
}
