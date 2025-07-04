using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record EnchantOptionTable(IReadOnlyDictionary<int, EnchantOptionMetadata> Entries) : ServerTable;

public record EnchantOptionMetadata(
    int Id,
    int Slot,
    int EnchantLevel,
    short Rarity,
    float Rate,
    int MinLevel,
    int MaxLevel,
    BasicAttribute[] Attributes);
