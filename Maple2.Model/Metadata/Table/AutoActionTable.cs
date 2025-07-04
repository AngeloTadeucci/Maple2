namespace Maple2.Model.Metadata;

public record AutoActionTable(IReadOnlyDictionary<string, IReadOnlyDictionary<int, AutoActionMetaData>> Entries) : Table;

public record AutoActionMetaData(
    string Content,
    int Id,
    int Duration,
    long MeretCost,
    long MesoCost);
