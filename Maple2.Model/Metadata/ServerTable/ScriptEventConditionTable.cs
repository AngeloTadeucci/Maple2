using Maple2.Model.Enum;
using Maple2.Model.Error;

namespace Maple2.Model.Metadata;

public record ScriptEventConditionTable(IReadOnlyDictionary<ScriptEventType, Dictionary<int, ScriptEventConditionMetadata>> Entries) : ServerTable;

public record ScriptEventConditionMetadata(
    int Id,
    ScriptEventType EventType,
    ItemEnchantError ErrorCode,
    short Rarity,
    int[] EnchantLevel,
    int FailCount,
    EnchantDamageType DamageType,
    EnchantResult ResultType
    );
