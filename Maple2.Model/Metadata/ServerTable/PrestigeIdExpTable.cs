using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record PrestigeIdExpTable(
    IReadOnlyDictionary<int, PrestigeIdExpTable.Entry> Entries) : ServerTable {

    public record Entry(
        int Id,
        long Value,
        ExpType Type);
}

