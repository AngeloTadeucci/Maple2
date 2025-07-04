using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record PrestigeMissionTable(IReadOnlyDictionary<int, PrestigeMissionMetadata> Entries) : Table;

public record PrestigeMissionMetadata(
    int Id,
    int Count,
    ItemComponent Item);
