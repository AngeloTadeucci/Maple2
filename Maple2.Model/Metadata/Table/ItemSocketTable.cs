namespace Maple2.Model.Metadata;

public record ItemSocketTable(IReadOnlyDictionary<int, IReadOnlyDictionary<int, ItemSocketMetadata>> Entries) : Table;

public record ItemSocketMetadata(byte MaxCount, byte OpenCount);
