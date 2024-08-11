using System.Collections.Generic;
using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Model.Metadata;

public record ItemMergeTable(
    IReadOnlyDictionary<int, Dictionary<int, ItemMergeSlot>> Entries) : ServerTable;

public record ItemMergeSlot(
    int Slot,
    long MesoCost,
    ItemComponent[] Materials,
    IReadOnlyDictionary<BasicAttribute, ItemMergeOption> BasicOptions,
    IReadOnlyDictionary<SpecialAttribute, ItemMergeOption> SpecialOptions
);

public record ItemMergeOption(
    int[] Values,
    float[] Rates,
    int[] Weights
);
