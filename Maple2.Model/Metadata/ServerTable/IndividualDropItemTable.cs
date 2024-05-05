using System.Collections.Generic;
using System.Numerics;

namespace Maple2.Model.Metadata;

public record IndividualDropItemTable(
    IReadOnlyDictionary<int, IDictionary<byte, IList<IndividualDropItemTable.Entry>>> Entries) : ServerTable {

    public record Entry(
        int GroupId,
        int SmartDropRate,
        Range<int> DropCount,
        Range<int> DropCountProbability,
        int MinLevel,
        bool ServerDrop,
        bool SmartGender,
        IList<Item> Items);

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
        bool QuestConstraint) {

        public record Rarity(
            int Probability,
            short Grade);
    }

    public readonly record struct Range<T>(T Min, T Max) where T : INumber<T>;
}
