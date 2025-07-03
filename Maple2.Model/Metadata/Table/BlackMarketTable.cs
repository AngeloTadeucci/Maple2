namespace Maple2.Model.Metadata;

public record BlackMarketTable(IReadOnlyDictionary<int, string[]> Entries) : Table;
