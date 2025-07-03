namespace Maple2.Model.Metadata;

public record PrestigeLevelAbilityTable(IReadOnlyDictionary<int, PrestigeLevelAbilityMetadata> Entries) : Table;

public record PrestigeLevelAbilityMetadata(
    int Id,
    int RequiredLevel,
    int Interval,
    int MaxCount,
    int BuffId,
    float StartValue,
    float AddValue);
