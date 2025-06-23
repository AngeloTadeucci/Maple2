using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record SetItemTable(IReadOnlyDictionary<int, SetItemTable.Entry> Entries) : Table {
    public record Entry(SetItemInfoMetadata Info, SetBonusMetadata[] Options);
}

public record SetItemInfoMetadata(
    int Id,
    string Name,
    int[] ItemIds,
    int OptionId);

public record SetBonusMetadata(
    int Count,
    SetBonusAdditionalEffect[] AdditionalEffects,
    IReadOnlyDictionary<BasicAttribute, long> Values,
    IReadOnlyDictionary<BasicAttribute, float> Rates,
    IReadOnlyDictionary<SpecialAttribute, float> SpecialValues,
    IReadOnlyDictionary<SpecialAttribute, float> SpecialRates);

public record SetBonusAdditionalEffect(int Id, short Level);
