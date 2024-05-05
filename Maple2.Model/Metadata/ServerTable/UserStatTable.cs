

using System.Collections.Generic;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record UserStatTable(
    IReadOnlyDictionary<JobCode, IReadOnlyDictionary<short, UserStatMetadata>> Stats
) : ServerTable;

public record UserStatMetadata(
 short level,
 long strength,
 long dexterity,
 long intelligence,
 long luck,
 long hp,
 long hpRegen,
 long hpRegenInterval,
 long spirit,
 long spiritRegen,
 long spiritRegenInterval,
 long stamina,
 long staminaRegen,
 long staminaRegenInterval,
 long attackSpeed,
 long movementSpeed,
 long accuracy,
 long evasion,
 long criticalRate,
 long criticalDamage,
 long criticalEvasion,
 long defense,
 long perfectGuard,
 long jumpHeight,
 long physicalAtk,
 long magicalAtk,
 long physicalRes,
 long magicalRes,
 long minWeaponAtk,
 long maxWeaponAtk,
 long damage,
 long piercing,
 long bonusAtk,
 long sp_value
);

