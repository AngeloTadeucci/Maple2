using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record ItemMergeTable(
    IReadOnlyDictionary<int, Dictionary<int, ItemMergeTable.Entry>> Entries) : ServerTable {
    public record Entry(
        int Slot,
        long MesoCost,
        ItemComponent[] Materials,
        IReadOnlyDictionary<BasicAttribute, Option> BasicOptions,
        IReadOnlyDictionary<SpecialAttribute, Option> SpecialOptions
    );

    public record Option(
        Range<int>[] Values,
        Range<int>[] Rates,
        int[] Weights);

    public readonly record struct Range<T>(T Min, T Max) where T : INumber<T>;
}

