using System.Collections.Generic;
using System.Numerics;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record ItemOptionConstantTable(
    IReadOnlyDictionary<int, IReadOnlyDictionary<int, ItemOptionConstant>> Options
) : Table;

public record ItemOptionRandomTable(
    IReadOnlyDictionary<int, IReadOnlyDictionary<int, ItemOption>> Options
) : Table;

public record ItemOptionStaticTable(
    IReadOnlyDictionary<int, IReadOnlyDictionary<int, ItemOption>> Options
) : Table;

public record ItemOptionConstant(
    IReadOnlyDictionary<BasicAttribute, int> Values,
    IReadOnlyDictionary<BasicAttribute, float> Rates,
    IReadOnlyDictionary<SpecialAttribute, int> SpecialValues,
    IReadOnlyDictionary<SpecialAttribute, float> SpecialRates);

public record ItemOption(
    float MultiplyFactor,
    ItemOption.Range<int> NumPick,
    ItemOption.Entry[] Entries
) {
    public readonly record struct Entry(
        BasicAttribute? BasicAttribute = null,
        SpecialAttribute? SpecialAttribute = null,
        Range<int>? Values = null,
        Range<float>? Rates = null);
    public readonly record struct Range<T>(T Min, T Max) where T : INumber<T>;
}

public record ItemOptionPickTable(
    IReadOnlyDictionary<int, IReadOnlyDictionary<int, ItemOptionPickTable.Option>> Options
) : Table {
    public record Option(
        IReadOnlyDictionary<BasicAttribute, int> ConstantValue,
        IReadOnlyDictionary<BasicAttribute, int> ConstantRate,
        IReadOnlyDictionary<BasicAttribute, int> StaticValue,
        IReadOnlyDictionary<BasicAttribute, int> StaticRate,
        IReadOnlyDictionary<BasicAttribute, int> RandomValue,
        IReadOnlyDictionary<BasicAttribute, int> RandomRate
    );
}

public record ItemVariationTable(
    IReadOnlyDictionary<BasicAttribute, ItemVariationTable.Range<int>[]> Values,
    IReadOnlyDictionary<BasicAttribute, ItemVariationTable.Range<float>[]> Rates,
    IReadOnlyDictionary<SpecialAttribute, ItemVariationTable.Range<int>[]> SpecialValues,
    IReadOnlyDictionary<SpecialAttribute, ItemVariationTable.Range<float>[]> SpecialRates
) : Table {
    public readonly record struct Range<T>(T Min, T Max, T Variation) where T : INumber<T>;
}

public record ItemEquipVariationTable(
    IReadOnlyDictionary<BasicAttribute, ItemEquipVariationTable.Set<int>[]> Values,
    IReadOnlyDictionary<BasicAttribute, ItemEquipVariationTable.Set<float>[]> Rates,
    IReadOnlyDictionary<SpecialAttribute, ItemEquipVariationTable.Set<int>[]> SpecialValues,
    IReadOnlyDictionary<SpecialAttribute, ItemEquipVariationTable.Set<float>[]> SpecialRates
) : Table {
    public record Set<T>(T Value, int Weight) where T : INumber<T>;
}
