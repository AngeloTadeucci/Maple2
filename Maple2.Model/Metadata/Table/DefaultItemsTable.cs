using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record DefaultItemsTable(
    IReadOnlyDictionary<EquipSlot, int[]> Common,
    IReadOnlyDictionary<JobCode, IReadOnlyDictionary<EquipSlot, int[]>> Job) : Table;
