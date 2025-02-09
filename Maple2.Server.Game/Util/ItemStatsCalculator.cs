using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Tools.Extensions;
using Maple2.Server.Core.Formulas;
using Maple2.Tools;
using Serilog;

namespace Maple2.Server.Game.Util;

public sealed class ItemStatsCalculator {
    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required TableMetadataStorage TableMetadata { private get; init; }
    public required Lua.Lua Lua { private get; init; }
    // ReSharper restore All
    #endregion

    private const float OFFENSE_LINE_MAX_THRESHOLD = 0.5f; // 50% threshold for offense lines
    private static readonly IList<BasicAttribute> offenseBasicAttributes = [
        BasicAttribute.Strength,
        BasicAttribute.Dexterity,
        BasicAttribute.Intelligence,
        BasicAttribute.Luck,
        BasicAttribute.Accuracy,
        BasicAttribute.AttackSpeed,
        BasicAttribute.CriticalRate,
        BasicAttribute.CriticalDamage,
        BasicAttribute.PhysicalAtk,
        BasicAttribute.MagicalAtk,
        BasicAttribute.MinWeaponAtk,
        BasicAttribute.MaxWeaponAtk,
        BasicAttribute.Damage,
        BasicAttribute.Piercing,
        BasicAttribute.BonusAtk,
        BasicAttribute.PetBonusAtk,
    ];
    private static readonly IList<SpecialAttribute> offenseSpecialAttributes = [
        SpecialAttribute.TotalDamage,
        SpecialAttribute.CriticalDamage,
        SpecialAttribute.NormalNpcDamage,
        SpecialAttribute.LeaderNpcDamage,
        SpecialAttribute.EliteNpcDamage,
        SpecialAttribute.BossNpcDamage,
        SpecialAttribute.IceDamage,
        SpecialAttribute.FireDamage,
        SpecialAttribute.DarkDamage,
        SpecialAttribute.HolyDamage,
        SpecialAttribute.PoisonDamage,
        SpecialAttribute.ElectricDamage,
        SpecialAttribute.MeleeDamage,
        SpecialAttribute.RangedDamage,
        SpecialAttribute.PhysicalPiercing,
        SpecialAttribute.MagicalPiercing,
        SpecialAttribute.MeleeSplashDamage,
        SpecialAttribute.RangedSplashDamage,
        SpecialAttribute.PvpDamage,
        SpecialAttribute.SkillLevelUpTier1,
        SpecialAttribute.SkillLevelUpTier2,
        SpecialAttribute.SkillLevelUpTier3,
        SpecialAttribute.SkillLevelUpTier4,
        SpecialAttribute.SkillLevelUpTier5,
        SpecialAttribute.SkillLevelUpTier6,
        SpecialAttribute.SkillLevelUpTier7,
        SpecialAttribute.SkillLevelUpTier8,
        SpecialAttribute.SkillLevelUpTier9,
        SpecialAttribute.SkillLevelUpTier10,
        SpecialAttribute.SkillLevelUpTier11,
        SpecialAttribute.SkillLevelUpTier12,
        SpecialAttribute.SkillLevelUpTier13,
        SpecialAttribute.SkillLevelUpTier14,
        SpecialAttribute.DarkStreamDamage,
        SpecialAttribute.MaxWeaponAttack,
        SpecialAttribute.ChaosRaidAttack,
    ];
    private static readonly IList<SpecialAttribute> elementalDamageAttributes = [
        SpecialAttribute.IceDamage,
        SpecialAttribute.FireDamage,
        SpecialAttribute.DarkDamage,
        SpecialAttribute.HolyDamage,
        SpecialAttribute.PoisonDamage,
        SpecialAttribute.ElectricDamage,
    ];

    public ItemStats? GetStats(Item item, bool rollMax = false) {
        if (item.Metadata.Option == null) {
            return null;
        }

        var stats = new ItemStats();
        int job = (int) item.Metadata.Limit.JobLimits.FirstOrDefault(JobCode.None);

        ItemOptionPickTable.Option? pick = TableMetadata.ItemOptionPickTable.Options.GetValueOrDefault(item.Metadata.Option.PickId, item.Rarity);
        if (GetConstantOption(item, job, pick, out ItemStats.Option? constantOption)) {
            stats[ItemStats.Type.Constant] = constantOption;
        }

        if (GetStaticOption(item, job, ItemStats.Type.Static, pick, out ItemStats.Option? staticOption)) {
            stats[ItemStats.Type.Static] = staticOption;
        }


        if (TableMetadata.ItemOptionRandomTable.Options.TryGetValue(item.Metadata.Option.RandomId, item.Rarity, out ItemOption? itemOption)) {
            ItemStats.Option option = GetRandomOption(itemOption, item.Type, ItemStats.Type.Random);
            RandomizeValues(item, itemOption, ref option, rollMax);
            stats[ItemStats.Type.Random] = option;
        }

        return stats;
    }

    public ItemSocket? GetSockets(Item item) {
        // Only Earring, Necklace, and Ring have sockets.
        if (!item.Type.IsAccessory || !item.Type.IsEarring && !item.Type.IsNecklace && !item.Type.IsRing) {
            return null;
        }

        int socketId = item.Metadata.Property.SocketId;
        if (item.Metadata.Property.SocketId != 0) {
            if (!TableMetadata.ItemSocketTable.Entries.TryGetValue(socketId, item.Rarity, out ItemSocketMetadata? metadata)) {
                // Fallback to rarity 0 which means any rarity.
                if (!TableMetadata.ItemSocketTable.Entries.TryGetValue(socketId, 0, out metadata)) {
                    return null;
                }
            }

            return new ItemSocket(metadata.MaxCount, metadata.OpenCount);
        }

        int maxSockets = Lua.CalcItemSocketMaxCount(item.Type.Type, item.Rarity, (ushort) (item.Metadata.Option?.LevelFactor ?? 0), item.Metadata.Property.IsSkin ? 1 : 0);
        byte openSocketCount = ItemSocketSlots.OpenSocketCount(maxSockets);
        return new ItemSocket((byte) maxSockets, openSocketCount);
    }

    public bool UpdateRandomOption(ref Item item, params LockOption[] presets) {
        if (item.Metadata.Option == null || item.Stats == null) {
            return false;
        }

        ItemStats.Option option = item.Stats[ItemStats.Type.Random];
        if (option.Count == 0) {
            return false;
        }

        // Get some random options
        if (!TableMetadata.ItemOptionRandomTable.Options.TryGetValue(item.Metadata.Option.RandomId, item.Rarity, out ItemOption? itemOption)) {
            return false;
        }
        ItemStats.Option randomOption = GetRandomOption(itemOption, item.Type, ItemStats.Type.Random, option.Count, presets);

        if (!RandomizeValues(item, itemOption, ref randomOption)) {
            return false;
        }

        // Restore locked values.
        foreach (LockOption lockOption in presets) {
            if (lockOption.TryGet(out BasicAttribute basic, out bool lockBasicValue)) {
                if (lockBasicValue) {
                    Debug.Assert(randomOption.Basic.ContainsKey(basic), "Missing basic attribute after using lock.");
                    randomOption.Basic[basic] = option.Basic[basic];
                }
            } else if (lockOption.TryGet(out SpecialAttribute special, out bool lockSpecialValue)) {
                if (lockSpecialValue) {
                    Debug.Assert(randomOption.Special.ContainsKey(special), "Missing special attribute after using lock.");
                    randomOption.Special[special] = option.Special[special];
                }
            }
        }

        // Update item with result.
        item.Stats[ItemStats.Type.Random] = randomOption;
        return true;
    }

    /// <param name="item">Item</param>
    /// <param name="itemOptionMetadata">Item's Random Option Metadata</param>
    /// <param name="option"></param>
    /// <param name="rollMax">Select the highest possible roll and circumvents the randomness</param>
    public bool RandomizeValues(Item item, ItemOption itemOptionMetadata, ref ItemStats.Option option, bool rollMax = false) {
        if (item.Metadata.Option == null) {
            return false;
        }
        ItemEquipVariationTable? table = GetVariationTable(item.Type);
        if (table == null) {
            return false;
        }

        foreach (BasicAttribute attribute in option.Basic.Keys) {
            if (table.Values.TryGetValue(attribute, out ItemEquipVariationTable.Set<int>[]? values)) {
                int value = GetValue(item, itemOptionMetadata, attribute, tableValues: values, rollMax: rollMax);
                option.Basic[attribute] = new BasicOption((int) (value * option.MultiplyFactor));
            } else if (table.Rates.TryGetValue(attribute, out ItemEquipVariationTable.Set<float>[]? rates)) {
                int rateInt = GetValue(item, itemOptionMetadata, attribute, tableRates: rates, rollMax: rollMax);
                option.Basic[attribute] = new BasicOption((rateInt / 1000f) * option.MultiplyFactor);
            }
        }
        foreach (SpecialAttribute attribute in option.Special.Keys) {
            if (table.SpecialValues.TryGetValue(attribute, out ItemEquipVariationTable.Set<int>[]? values)) {
                int value = GetValue(item, itemOptionMetadata, specialAttribute: attribute, tableValues: values, rollMax: rollMax);
                option.Special[attribute] = new SpecialOption(0f, value * option.MultiplyFactor);
            } else if (table.SpecialRates.TryGetValue(attribute, out ItemEquipVariationTable.Set<float>[]? rates)) {
                int rateInt = GetValue(item, itemOptionMetadata, specialAttribute: attribute, tableRates: rates, rollMax: rollMax);
                option.Special[attribute] = new SpecialOption((rateInt / 1000f) * option.MultiplyFactor);
            }
        }

        return true;

        int GetValue(Item item, ItemOption itemOptionMetadata, BasicAttribute? attribute = null, SpecialAttribute? specialAttribute = null, ItemEquipVariationTable.Set<int>[]? tableValues = null, ItemEquipVariationTable.Set<float>[]? tableRates = null, bool rollMax = false) {
            var weightedValues = new WeightedSet<int>();
            var weightedRates = new WeightedSet<float>();
            if (item.Type.IsPet) {
                switch (item.Rarity) {
                    case < 3:
                        if (rollMax) {
                            if (tableValues != null) {
                                return tableValues[4].Value;
                            }
                            if (tableRates != null) {
                                return (int) (tableRates[4].Value * 1000);
                            }
                        }
                        // only gets values from idx 0 to 4
                        for (int i = 0; i < 5; i++) {
                            if (tableValues != null) {
                                weightedValues.Add(tableValues[i].Value, tableValues[i].Weight);
                            } else if (tableRates != null) {
                                weightedRates.Add(tableRates[i].Value, tableRates[i].Weight);
                            }
                        }
                        break;
                    default:
                        if (rollMax) {
                            if (tableValues != null) {
                                return tableValues[17].Value;
                            }
                            if (tableRates != null) {
                                return (int) (tableRates[17].Value * 1000);
                            }
                        }
                        // only gets values from idx 0 to 17 (entire array)
                        for (int i = 0; i < 18; i++) {
                            if (tableValues != null) {
                                weightedValues.Add(tableValues[i].Value, tableValues[i].Weight);
                            } else if (tableRates != null) {
                                weightedRates.Add(tableRates[i].Value, tableRates[i].Weight);
                            }
                        }
                        break;
                }
                if (tableValues != null) {
                    return weightedValues.Get();
                }
                if (tableRates != null) {
                    return (int) (weightedRates.Get() * 1000);
                }
            } else {
                switch (item.Metadata.Option!.ItemOptionType) {
                    case ItemOptionMakeType.Range:
                        switch (item.Metadata.Option.LevelFactor) {
                            case < 50:
                                if (rollMax) {
                                    if (tableValues != null) {
                                        return tableValues[2].Value;
                                    }
                                    if (tableRates != null) {
                                        return (int) (tableRates[2].Value * 1000);
                                    }
                                }
                                // only gets values from idx 0 to 2
                                for (int i = 0; i < 3; i++) {
                                    if (tableValues != null) {
                                        weightedValues.Add(tableValues[i].Value, tableValues[i].Weight);
                                    } else if (tableRates != null) {
                                        weightedRates.Add(tableRates[i].Value, tableRates[i].Weight);
                                    }
                                }
                                break;
                            case < 70:
                                if (rollMax) {
                                    if (tableValues != null) {
                                        return tableValues[9].Value;
                                    }
                                    if (tableRates != null) {
                                        return (int) (tableRates[9].Value * 1000);
                                    }
                                }
                                // only gets values from idx 2 to 9
                                for (int i = 2; i < 10; i++) {
                                    if (tableValues != null) {
                                        weightedValues.Add(tableValues[i].Value, tableValues[i].Weight);
                                    } else if (tableRates != null) {
                                        weightedRates.Add(tableRates[i].Value, tableRates[i].Weight);
                                    }
                                }
                                break;
                            case >= 70:
                                if (rollMax) {
                                    if (tableValues != null) {
                                        return tableValues[17].Value;
                                    }
                                    if (tableRates != null) {
                                        return (int) (tableRates[17].Value * 1000);
                                    }
                                }
                                // only gets values from idx 10 to 17
                                for (int i = 10; i < 18; i++) {
                                    if (tableValues != null) {
                                        weightedValues.Add(tableValues[i].Value, tableValues[i].Weight);
                                    } else if (tableRates != null) {
                                        weightedRates.Add(tableRates[i].Value, tableRates[i].Weight);
                                    }
                                }
                                break;
                        }
                        if (tableValues != null) {
                            return weightedValues.Get();
                        }
                        if (tableRates != null) {
                            return (int) (weightedRates.Get() * 1000);
                        }
                        break;
                    case ItemOptionMakeType.Base:
                    default:
                        float minRate = 0;
                        int minValue = 0;
                        if (attribute != null) {
                            ItemOption.Entry entry = itemOptionMetadata.Entries.FirstOrDefault(optionEntry => optionEntry.BasicAttribute == attribute);
                            minRate = entry.Rates?.Min ?? 0;
                            minValue = entry.Values?.Min ?? 0;
                            return GetItemVariationValue(basicAttribute: attribute, value: minValue, rate: minRate);
                        }
                        if (specialAttribute != null) {
                            ItemOption.Entry entry = itemOptionMetadata.Entries.FirstOrDefault(optionEntry => optionEntry.SpecialAttribute == specialAttribute);
                            minRate = entry.Rates?.Min ?? 0;
                            minValue = entry.Values?.Min ?? 0;
                            return GetItemVariationValue(specialAttribute: specialAttribute, value: minValue, rate: minRate);
                        }
                        break;
                }
            }

            return 0;
        }
    }

    // Used to calculate the default constant attributes for a given item.
    private bool GetConstantOption(Item item, int job, ItemOptionPickTable.Option? pick, [NotNullWhen(true)] out ItemStats.Option? option) {
        option = null;
        if (item.Metadata.Option == null) {
            return false;
        }

        if (TableMetadata.ItemOptionConstantTable.Options.TryGetValue(item.Metadata.Option.ConstantId, item.Rarity, out ItemOptionConstant? itemOptionConstant)) {
            option = ConstantItemOption(itemOptionConstant);
        }

        if (item.Metadata.Option.ConstantType == ItemOptionMakeType.Lua && pick != null) {
            if (option == null) {
                option = new ItemStats.Option();
            }

            foreach ((BasicAttribute attribute, int deviation) in pick.ConstantValue) {
                int currentValue = option.Basic.TryGetValue(attribute, out BasicOption basicOption) ? basicOption.Value : 0;
                int value = ConstValue(attribute, currentValue, deviation, item.Type.Type, job, item.Metadata.Option.LevelFactor, item.Rarity, (ushort) item.Metadata.Limit.Level);
                if (value > 0) {
                    option.Basic[attribute] = new BasicOption(value);
                }
            }
        }
        return option != null;
    }

    // Used to calculate the default static attributes for a given item.
    private bool GetStaticOption(Item item, int job, ItemStats.Type statsType, ItemOptionPickTable.Option? pick, [NotNullWhen(true)] out ItemStats.Option? option) {
        option = null;
        if (item.Metadata.Option == null) {
            return false;
        }

        if (TableMetadata.ItemOptionStaticTable.Options.TryGetValue(item.Metadata.Option.StaticId, item.Rarity, out ItemOption? itemOption)) {
            // We're using RandomItemOption here considering the logic is the same.
            option = RandomItemOption(itemOption, item.Type, statsType);
        }

        if (item.Metadata.Option.StaticType == ItemOptionMakeType.Lua && pick != null) {
            if (option == null) {
                option = new ItemStats.Option();
            }

            foreach ((BasicAttribute attribute, int deviation) in pick.StaticValue) {
                int currentValue = option.Basic.TryGetValue(attribute, out BasicOption basicOption) ? basicOption.Value : 0;
                int value = StaticValue(attribute, currentValue, deviation, item.Type.Type, job, item.Metadata.Option.LevelFactor, item.Rarity, (ushort) item.Metadata.Limit.Level);
                if (value > 0) {
                    option.Basic[attribute] = new BasicOption(value);
                }
            }
            return true;
        }
        return option != null;
    }

    // Used to calculate the default random attributes for a given item.
    private ItemStats.Option GetRandomOption(ItemOption itemOption, in ItemType itemType, ItemStats.Type statsType, int count = -1, params LockOption[] presets) {
        return RandomItemOption(itemOption, itemType, statsType, count, presets);
    }

    private ItemEquipVariationTable? GetVariationTable(in ItemType type) {
        if (type.IsAccessory) {
            return TableMetadata.AccessoryVariationTable;
        }
        if (type.IsArmor) {
            return TableMetadata.ArmorVariationTable;
        }
        if (type.IsWeapon) {
            return TableMetadata.WeaponVariationTable;
        }
        if (type.IsCombatPet) { // StoragePet cannot have variations
            return TableMetadata.PetVariationTable;
        }

        return null;
    }

    /// <returns>If the expected value is a rate, it will be multiplied by 1000. Conversion needs to be done afterward.</returns>
    private int GetItemVariationValue(BasicAttribute? basicAttribute = null, SpecialAttribute? specialAttribute = null, int value = 0, float rate = 0) {
        if (basicAttribute != null) {
            if (TableMetadata.ItemVariationTable.Values.TryGetValue(basicAttribute.Value, out ItemVariationTable.Range<int>[]? values)) {
                foreach (ItemVariationTable.Range<int> range in values) {
                    if (value >= range.Min && value <= range.Max) {
                        return Random.Shared.Next(value, value + range.Variation + 1);
                    }
                }
            } else if (TableMetadata.ItemVariationTable.Rates.TryGetValue(basicAttribute.Value, out ItemVariationTable.Range<float>[]? rates)) {
                foreach (ItemVariationTable.Range<float> range in rates) {
                    if (rate >= range.Min && rate <= range.Max) {
                        int convertedVariation = (int) range.Variation * 1000;
                        int convertedRate = (int) rate * 1000;
                        return Random.Shared.Next(convertedRate, convertedRate + convertedVariation + 1);
                    }
                }
            }
        } else if (specialAttribute != null) {
            if (TableMetadata.ItemVariationTable.SpecialValues.TryGetValue(specialAttribute.Value, out ItemVariationTable.Range<int>[]? values)) {
                foreach (ItemVariationTable.Range<int> range in values) {
                    if (value >= range.Min && value <= range.Max) {
                        return Random.Shared.Next(value, value + range.Variation + 1);
                    }
                }
            } else if (TableMetadata.ItemVariationTable.SpecialRates.TryGetValue(specialAttribute.Value, out ItemVariationTable.Range<float>[]? rates)) {
                foreach (ItemVariationTable.Range<float> range in rates) {
                    if (rate >= range.Min && rate <= range.Max) {
                        int convertedVariation = (int) range.Variation * 1000;
                        int convertedRate = (int) rate * 1000;
                        return Random.Shared.Next(convertedRate, convertedRate + convertedVariation + 1);
                    }
                }
            }
        }
        return 0;
    }

    private static ItemStats.Option ConstantItemOption(ItemOptionConstant option) {
        var statResult = new Dictionary<BasicAttribute, BasicOption>();
        var specialResult = new Dictionary<SpecialAttribute, SpecialOption>();

        foreach ((BasicAttribute attribute, int value) in option.Values) {
            statResult.Add(attribute, new BasicOption(value));
        }
        foreach ((BasicAttribute attribute, float rate) in option.Rates) {
            statResult.Add(attribute, new BasicOption(rate));
        }
        foreach ((SpecialAttribute attribute, int value) in option.SpecialValues) {
            specialResult.Add(attribute, new SpecialOption(0f, value));
        }
        foreach ((SpecialAttribute attribute, float rate) in option.SpecialRates) {
            specialResult.Add(attribute, new SpecialOption(rate));
        }
        return new ItemStats.Option(statResult, specialResult);
    }

    private static ItemStats.Option RandomItemOption(ItemOption option, in ItemType itemType, ItemStats.Type statsType, int count = -1, params LockOption[] presets) {
        var statResult = new Dictionary<BasicAttribute, BasicOption>();
        var specialResult = new Dictionary<SpecialAttribute, SpecialOption>();

        int total = count < 0 ? Random.Shared.Next(option.NumPick.Min, option.NumPick.Max + 1) : count;
        if (total == 0) {
            return new ItemStats.Option(statResult, specialResult, multiplyFactor: option.MultiplyFactor);
        }
        // Ensures that there are enough options to choose.
        total = Math.Min(total, option.Entries.Length);

        // Compute locked options first.
        foreach (LockOption preset in presets) {
            if (preset.TryGet(out BasicAttribute basic, out bool _)) {
                ItemOption.Entry entry = option.Entries.FirstOrDefault(e => e.BasicAttribute == basic);
                // Ignore any invalid presets, they will get populated with valid data below.
                AddResult(entry, statResult, specialResult);
            } else if (preset.TryGet(out SpecialAttribute special, out bool _)) {
                ItemOption.Entry entry = option.Entries.FirstOrDefault(e => e.SpecialAttribute == special);
                // Ignore any invalid presets, they will get populated with valid data below.
                AddResult(entry, statResult, specialResult);
            }
        }

        while (statResult.Count + specialResult.Count < total) {
            ItemOption.Entry entry = option.Entries.Random();
            if (statsType == ItemStats.Type.Random &&
                !IsValidStat(itemType, total, statResult, specialResult, entry)) {
                continue;
            }
            if (!AddResult(entry, statResult, specialResult)) {
                Log.Error("Failed to select random item option: {Entry}", entry); // Invalid entry
            }
        }

        return new ItemStats.Option(statResult, specialResult, multiplyFactor: option.MultiplyFactor);

        // Helper function
        bool AddResult(ItemOption.Entry entry, IDictionary<BasicAttribute, BasicOption> statDict, IDictionary<SpecialAttribute, SpecialOption> specialDict) {
            if (entry.BasicAttribute == null && entry.SpecialAttribute == null || entry.Values == null && entry.Rates == null) {
                return false;
            }

            if (entry.BasicAttribute != null) {
                var attribute = (BasicAttribute) entry.BasicAttribute;
                if (statDict.ContainsKey(attribute)) return true; // Cannot add duplicate values, retry.

                if (entry.Values != null) {
                    statDict.Add(attribute, new BasicOption(Random.Shared.Next(entry.Values.Value.Min, entry.Values.Value.Max + 1)));
                } else if (entry.Rates != null) {
                    float delta = entry.Rates.Value.Max - entry.Rates.Value.Min;
                    statDict.Add(attribute, new BasicOption(Random.Shared.NextSingle() * delta + entry.Rates.Value.Min));
                }
                return true;
            }
            if (entry.SpecialAttribute != null) {
                var attribute = (SpecialAttribute) entry.SpecialAttribute;
                if (specialDict.ContainsKey(attribute)) return true; // Cannot add duplicate values, retry.

                if (entry.Values != null) {
                    specialDict.Add(attribute, new SpecialOption(0f, Random.Shared.Next(entry.Values.Value.Min, entry.Values.Value.Max + 1)));
                } else if (entry.Rates != null) {
                    float delta = entry.Rates.Value.Max - entry.Rates.Value.Min;
                    specialDict.Add(attribute, new SpecialOption(Random.Shared.NextSingle() * delta + entry.Rates.Value.Min));
                }
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Verifies if the new attribute being added meets the offense line threshold.
    /// </summary>
    /// <returns></returns>
    private static bool IsValidStat(ItemType itemType, int statLineCount, IDictionary<BasicAttribute, BasicOption> statDict, IDictionary<SpecialAttribute, SpecialOption> specialDict, ItemOption.Entry entry) {
        if (itemType.IsAccessory) {
            int currentElementalStats = specialDict.Keys.Count(stat => elementalDamageAttributes.Contains(stat));
            if (entry.SpecialAttribute != null && elementalDamageAttributes.Contains((SpecialAttribute) entry.SpecialAttribute)) {
                currentElementalStats++;
            }
            return currentElementalStats < 2;
        }

        if (itemType.IsArmor) {
            int offenseStatCount = statDict.Keys.Count(stat => offenseBasicAttributes.Contains(stat));
            offenseStatCount += specialDict.Keys.Count(stat => offenseSpecialAttributes.Contains(stat));

            if (entry.BasicAttribute != null && offenseBasicAttributes.Contains((BasicAttribute) entry.BasicAttribute)) {
                offenseStatCount++;
            } else if (entry.SpecialAttribute != null && offenseSpecialAttributes.Contains((SpecialAttribute) entry.SpecialAttribute)) {
                offenseStatCount++;
            }

            return (float) offenseStatCount / statLineCount < OFFENSE_LINE_MAX_THRESHOLD;
        }

        return true;
    }

    private int ConstValue(BasicAttribute attribute, int statValue, int deviation, int type, int job, int levelFactor, int rarity, ushort level) {
        (float, float) range = attribute switch {
            BasicAttribute.Strength => Lua.ConstantValueStr(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.Dexterity => Lua.ConstantValueDex(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.Intelligence => Lua.ConstantValueInt(statValue, deviation, type, job, levelFactor, rarity, level, 1), // TODO: handle a7
            BasicAttribute.Luck => Lua.ConstantValueLuk(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.Health => Lua.ConstantValueHp(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.CriticalRate => Lua.ConstantValueCap(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.Defense => Lua.ConstantValueNdd(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.MagicalAtk => Lua.ConstantValueMap(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.PhysicalRes => Lua.ConstantValuePar(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.MagicalRes => Lua.ConstantValueMar(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.MinWeaponAtk => Lua.ConstantValueWapMin(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.MaxWeaponAtk => Lua.ConstantValueWapMax(statValue, deviation, type, job, levelFactor, rarity, level),
            _ => (0, 0),
        };
        return (int) Math.Max(range.Item1, range.Item2);
    }

    private int StaticValue(BasicAttribute attribute, int statValue, int deviation, int type, int job, int levelFactor, int rarity, ushort level) {
        (float, float) range = attribute switch {
            BasicAttribute.Health => Lua.StaticValueHp(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.Defense => Lua.StaticValueNdd(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.PhysicalAtk => Lua.StaticValuePap(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.MagicalAtk => Lua.StaticValueMap(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.PhysicalRes => Lua.StaticValuePar(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.MagicalRes => Lua.StaticValueMar(statValue, deviation, type, job, levelFactor, rarity, level),
            BasicAttribute.MaxWeaponAtk => Lua.StaticValueWapMax(statValue, deviation, type, job, levelFactor, rarity, level),
            _ => (0, 0),
        };
        return Random.Shared.Next((int) range.Item1, (int) range.Item2 + 1);
    }

    private float StaticRate(BasicAttribute attribute, int statValue, int deviation, int type, int job, int levelFactor, int rarity, ushort level) {
        (float, float) range = attribute switch {
            BasicAttribute.PerfectGuard => Lua.StaticRateAbp(statValue, deviation, type, job, levelFactor, rarity, level),
            _ => (0, 0),
        };
        return Random.Shared.NextSingle() * (range.Item2 - range.Item1) + range.Item1;
    }
}
