using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record UnlimitedEnchantOptionTable(IReadOnlyDictionary<int, Dictionary<int, UnlimitedEnchantOptionTable.Option>> Entries) : ServerTable { // Slot, Level
    public record Option(
        IReadOnlyDictionary<BasicAttribute, int> Values,
        IReadOnlyDictionary<BasicAttribute, float> Rates,
        IReadOnlyDictionary<SpecialAttribute, int> SpecialValues,
        IReadOnlyDictionary<SpecialAttribute, float> SpecialRates);
}

