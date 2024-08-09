using System.Collections.Generic;
using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record ItemMergeTable(
    IReadOnlyDictionary<int, IReadOnlyDictionary<int, ItemOptionConstant>> Options
) : Table {
    public record Entry(
        int Id,
        IReadOnlyDictionary<int, ItemMergeSlot> Slots);
}

public record ItemMergeSlot(
    int Slot,
    long MesoCost,
    ItemComponent[] Materials,
    IReadOnlyDictionary<BasicAttribute, ItemMergeOption> BasicOptions,
    IReadOnlyDictionary<SpecialAttribute, ItemMergeOption> SpecialOptions
);

public record ItemMergeOption(
    int MinValue,
    int[] Values,
    float MinRate,
    float[] Rates,
    int[] Weights
);
