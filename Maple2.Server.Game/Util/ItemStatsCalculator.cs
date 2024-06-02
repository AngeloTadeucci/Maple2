using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Tools.Extensions;
using Maple2.Server.Core.Formulas;
using Serilog;

namespace Maple2.Server.Game.Util;

public sealed class ItemStatsCalculator {
    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required TableMetadataStorage TableMetadata { private get; init; }
    public required Lua.Lua Lua { private get; init; }
    // ReSharper restore All
    #endregion

    public ItemStats? GetStats(Item item) {
        if (item.Metadata.Option == null) {
            return null;
        }

        var stats = new ItemStats();
        int job = (int) item.Metadata.Limit.JobLimits.FirstOrDefault(JobCode.Newbie);

        ItemOptionPickTable.Option? pick = TableMetadata.ItemOptionPickTable.Options.GetValueOrDefault(item.Metadata.Option.PickId, item.Rarity);
        if (GetConstantOption(item, job, pick, out ItemStats.Option? constantOption)) {
            stats[ItemStats.Type.Constant] = constantOption;
        }

        if (GetStaticOption(item, job, pick, out ItemStats.Option? staticOption)) {
            stats[ItemStats.Type.Static] = staticOption;
        }

        if (GetRandomOption(item, out ItemStats.Option? option)) {
            RandomizeValues(item.Type, ref option);
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
        if (!GetRandomOption(item, out ItemStats.Option? randomOption, option.Count, presets)) {
            return false;
        }

        if (!RandomizeValues(item.Type, ref randomOption)) {
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

    // TODO: These should technically be weighted towards the lower end.
    public bool RandomizeValues(in ItemType type, ref ItemStats.Option option) {
        ItemEquipVariationTable? table = GetVariationTable(type);
        if (table == null) {
            return false;
        }

        foreach (BasicAttribute attribute in option.Basic.Keys) {
            int index = Random.Shared.Next(2, 18);
            if (table.Values.TryGetValue(attribute, out int[]? values)) {
                int value = values.ElementAtOrDefault(index);
                option.Basic[attribute] = new BasicOption(value);
            } else if (table.Rates.TryGetValue(attribute, out float[]? rates)) {
                float rate = rates.ElementAtOrDefault(index);
                option.Basic[attribute] = new BasicOption(rate);
            }
        }
        foreach (SpecialAttribute attribute in option.Special.Keys) {
            int index = Random.Shared.Next(2, 18);
            if (table.SpecialValues.TryGetValue(attribute, out int[]? values)) {
                int value = values.ElementAtOrDefault(index);
                option.Special[attribute] = new SpecialOption(value);
            } else if (table.SpecialRates.TryGetValue(attribute, out float[]? rates)) {
                float rate = rates.ElementAtOrDefault(index);
                option.Special[attribute] = new SpecialOption(rate);
            }
        }

        return true;
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

        if (item.Metadata.Option.LevelFactor > 50 && pick != null) {
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
            return true;
        }

        return false;
    }

    // Used to calculate the default static attributes for a given item.
    private bool GetStaticOption(Item item, int job, ItemOptionPickTable.Option? pick, [NotNullWhen(true)] out ItemStats.Option? option) {
        option = null;
        if (item.Metadata.Option == null || item.Metadata.Option.LevelFactor < 50) {
            return false;
        }

        if (TableMetadata.ItemOptionStaticTable.Options.TryGetValue(item.Metadata.Option.StaticId, item.Rarity, out ItemOption? itemOption)) {
            // We're using RandomItemOption here considering the logic is the same.
            option = RandomItemOption(itemOption);
        }

        if (pick != null) {
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

        return false;
    }

    // Used to calculate the default random attributes for a given item.
    private bool GetRandomOption(Item item, [NotNullWhen(true)] out ItemStats.Option? option, int count = -1, params LockOption[] presets) {
        if (item.Metadata.Option == null) {
            option = null;
            return false;
        }

        if (TableMetadata.ItemOptionRandomTable.Options.TryGetValue(item.Metadata.Option.RandomId, item.Rarity, out ItemOption? itemOption)) {
            option = RandomItemOption(itemOption, count, presets);
            return true;
        }

        option = null;
        return false;
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
            specialResult.Add(attribute, new SpecialOption(value));
        }
        foreach ((SpecialAttribute attribute, float rate) in option.SpecialRates) {
            specialResult.Add(attribute, new SpecialOption(rate));
        }
        return new ItemStats.Option(statResult, specialResult);
    }

    private static ItemStats.Option RandomItemOption(ItemOption option, int count = -1, params LockOption[] presets) {
        var statResult = new Dictionary<BasicAttribute, BasicOption>();
        var specialResult = new Dictionary<SpecialAttribute, SpecialOption>();

        int total = count < 0 ? Random.Shared.Next(option.NumPick.Min, option.NumPick.Max + 1) : count;
        if (total == 0) {
            return new ItemStats.Option(statResult, specialResult);
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
            if (!AddResult(entry, statResult, specialResult)) {
                Log.Error("Failed to select random item option: {Entry}", entry); // Invalid entry
            }
        }

        return new ItemStats.Option(statResult, specialResult);

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
                    specialDict.Add(attribute, new SpecialOption(Random.Shared.Next(entry.Values.Value.Min, entry.Values.Value.Max + 1)));
                } else if (entry.Rates != null) {
                    float delta = entry.Rates.Value.Max - entry.Rates.Value.Min;
                    specialDict.Add(attribute, new SpecialOption(Random.Shared.NextSingle() * delta + entry.Rates.Value.Min));
                }
                return true;
            }
            return false;
        }
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
