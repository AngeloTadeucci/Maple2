using System.Collections.Generic;

namespace Maple2.Model.Metadata;

public record WorldMapTable(List<WorldMapTable.Map> Entries) : Table {
    public record Map(
        int Code,
        sbyte X,
        sbyte Y,
        sbyte Z,
        byte Size
    ) { }
}
