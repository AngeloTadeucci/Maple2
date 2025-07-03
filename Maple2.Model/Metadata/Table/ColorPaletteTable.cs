using Maple2.Model.Common;

namespace Maple2.Model.Metadata;

public record ColorPaletteTable(IReadOnlyDictionary<int, IReadOnlyDictionary<int, ColorPaletteTable.Entry>> Entries) : Table {

    public record Entry(Color Primary,
                        Color Secondary,
                        Color Tertiary,
                        int AchieveId,
                        int AchieveGrade);
}
