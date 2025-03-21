using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record RewardContentTable(IReadOnlyDictionary<int, RewardContentTable.Base> BaseEntries,
                                 IReadOnlyDictionary<int, RewardContentTable.Item> ItemEntries,
                                 IReadOnlyDictionary<int, long> MesoStaticEntries,
                                 IReadOnlyDictionary<int, Dictionary<int, long>> MesoEntries,
                                 IReadOnlyDictionary<int, long> ExpStaticEntries) : Table {
    public record Base(
        int Id,
        int MesoTableId,
        int ExpTableId,
        float MesoFactor,
        float ExpFactor,
        int ItemTableId,
        int PrestigeExpTableId);

    public record Item(
        int Id,
        Item.Data[] ItemData) {

        public record Data(
            int MinLevel,
            int MaxLevel,
            RewardItem[] RewardItems);
    }

    public record Meso(
        int Id,
        IReadOnlyDictionary<int, long> Table); // Level, Meso
}
