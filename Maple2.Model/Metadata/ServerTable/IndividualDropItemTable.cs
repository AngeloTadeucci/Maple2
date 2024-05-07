using System.Collections.Generic;
using System.Numerics;

namespace Maple2.Model.Metadata;

public record IndividualDropItemTable(
    IReadOnlyDictionary<int, IDictionary<int, IndividualDropItemTable.Entry>> Entries) : ServerTable {

    public record Entry(
        int GroupId,
        int SmartDropRate,
        IList<Entry.DropCount> DropCounts,
        int MinLevel,
        bool ServerDrop,
        bool SmartGender,
        IList<Item> Items) {

        public record DropCount(
            int Count,
            int Probability);
    }

    public record Item(
        int[] Ids,
        bool Announce,
        int ProperJobWeight,
        int ImproperJobWeight,
        int Weight,
        Range<int> DropCount,
        IList<Item.Rarity> Rarities,
        int EnchantLevel,
        int SocketDataId,
        bool DeductTradeCount,
        bool DeductRepackLimit,
        bool Bind,
        bool DisableBreak,
        int[] MapIds,
        int QuestId) {

        public record Rarity(
            int Probability,
            short Grade);
    }

    public readonly record struct Range<T>(T Min, T Max) where T : INumber<T>;
}
