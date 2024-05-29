using System.Collections.Generic;
using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record FieldMissionTable(IReadOnlyDictionary<int, FieldMissionTable.Entry> Entries) : Table {
    public record Entry(
        int MissionCount,
        ItemComponent? Item,
        int StatPoints) {
    }
}
