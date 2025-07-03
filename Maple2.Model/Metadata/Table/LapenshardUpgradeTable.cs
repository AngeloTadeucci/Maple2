using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record LapenshardUpgradeTable(IReadOnlyDictionary<int, LapenshardUpgradeTable.Entry> Entries) : Table {
    public record Entry(short Level, int GroupId, int NextItemId, int RequireCount, IReadOnlyList<Ingredient> Ingredients, long Meso);
    public record Ingredient(ItemTag ItemTag, int Amount);
}
