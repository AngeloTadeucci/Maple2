using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record GemstoneUpgradeTable(IReadOnlyDictionary<int, GemstoneUpgradeTable.Entry> Entries) : Table {
    public record Entry(short Level, int NextItemId, IReadOnlyList<Ingredient> Ingredients);
    public record Ingredient(ItemTag ItemTag, int Amount);
}
