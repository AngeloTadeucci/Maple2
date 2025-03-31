using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Server.Core.Formulas;

public static class Enchant {
    private static float[] LV_50_ENCHANT_ONYX_MULTIPLIER = [1, 1, 1, 1.37106f, 1.37106f, 1.742f, 2.1116f, 2.482f, 2.853f, 3.25f, 3.718f, 3.718f, 3.718f, 3.718f, 3.718f];
    private static float[] LV_70_ENCHANT_ONYX_MULTIPLIER = [1, 1, 1, 1.37106f, 1.37106f, 1.742f, 2.1116f, 2.482f, 2.853f, 5.707f, 5.707f, 8.561f, 8.561f, 11.415f, 11.415f];
    private static float[] ONYX_RARITY_MULTIPLIER = [0, 1, 1.237f, 1.5548f, 1.9216f, 2.3115f, 2.7794f];

    private static float[] LV_50_ENCHANT_CRYSTAL_FRAGMENT_MULTIPLIER = [1, 1, 1, 1.4f, 1.4f, 1.7f, 2.1f, 2.5f, 2.9f, 3.2f, 3.7f, 3.7f, 3.7f, 3.7f, 3.7f];
    private static float[] LV_70_ENCHANT_CRYSTAL_FRAGMENT_MULTIPLIER = [1, 1, 1, 1.375f, 1.375f, 1.75f, 2.125f, 2.4375f, 2.8125f, 7.5f, 7.5f, 11.25f, 11.25f, 15f, 15f];
    private static float[] CRYSTAL_FRAGMENT_RARITY_MULTIPLIER = [0, 1, 1.1897f, 1.49482f, 1.9216f, 2.4f, 3];

    private static float[] LV_50_ENCHANT_CHAOS_ONYX_MULTIPLIER = [1, 1, 1, 1.37106f, 1.37106f, 1.742f, 2.1116f, 2.482f, 2.853f, 3.25f, 3.718f, 3.718f, 3.718f, 3.718f, 3.718f];
    private static float[] LV_70_ENCHANT_CHAOS_ONYX_MULTIPLIER = [1, 1, 1, 1.333f, 1.333f, 2.25f, 2.25f, 2.25f, 2.25f, 8f, 8f, 12f, 12f, 16f, 16f];
    private static float[] CHAOS_ONYX_RARITY_MULTIPLIER = [0, 0, 1.237f, 1.5548f, 1.9216f, 2.3115f, 2.7794f];

    public static List<IngredientInfo> GetEnchantCost(Item item) {
        List<IngredientInfo> costs = [];
        IngredientInfo onyx = GetOnyxCost(item);
        if (onyx.Amount > 0) {
            costs.Add(onyx);
        }
        IngredientInfo chaosOnyx = GetChaosOnyxCost(item);
        if (chaosOnyx.Amount > 0) {
            costs.Add(chaosOnyx);
        }
        IngredientInfo crystalFragment = GetCrystalFragmentCost(item);
        if (crystalFragment.Amount > 0) {
            costs.Add(crystalFragment);
        }

        return costs;
    }
    private static IngredientInfo GetOnyxCost(Item item) {
        int itemLevel = item.Metadata.Limit.Level;
        double cost = 0;
        if (itemLevel is >= 20 and <= 50) {
            cost = 0.00011614 * Math.Pow(itemLevel, 4)
                   - 0.01078 * Math.Pow(itemLevel, 3)
                   + 0.46993 * Math.Pow(itemLevel, 2)
                   - 8.19353 * itemLevel
                   + 64.48147;
            cost = cost * ONYX_RARITY_MULTIPLIER[item.Rarity];
            cost = Math.Max(1, Math.Floor(cost));
        } else {
            switch (item.Rarity) {
                case 1:
                case 2:
                case 3:
                    // not supported?
                    break;
                case 4:
                    cost = 636;
                    break;
                case 5:
                    if (itemLevel < 70) {
                        cost = (507.0 / 14.0) * (itemLevel - 56) + 765;
                        break;
                    }
                    cost = 1272;
                    break;
                case 6:
                    cost = 1908; // may not be accurate but this is all the data I have
                    break;
            }
        }

        cost = ItemTypeMultiplier(item.Type, cost);

        int enchantLevel = item.Enchant?.Enchants ?? 0;
        if (itemLevel <= 50) {
            cost *= LV_50_ENCHANT_ONYX_MULTIPLIER[enchantLevel];
        } else {
            cost *= LV_70_ENCHANT_ONYX_MULTIPLIER[enchantLevel];
        }

        return new IngredientInfo(ItemTag.Onix, (int) Math.Round(cost));
    }

    private static IngredientInfo GetCrystalFragmentCost(Item item) {
        int itemLevel = item.Metadata.Limit.Level;
        double cost = 0;
        if (itemLevel is >= 20 and <= 50) {
            cost = -0.0000049627 * Math.Pow(itemLevel, 4)
                          +  0.0006886    * Math.Pow(itemLevel, 3)
                          -  0.0285752    * Math.Pow(itemLevel, 2)
                          +  0.45096      * itemLevel
                          -  1.2688;

            cost = cost * CRYSTAL_FRAGMENT_RARITY_MULTIPLIER[item.Rarity];
            cost = Math.Max(1, Math.Floor(cost));
        } else {
            switch (item.Rarity) {
                case 1:
                case 2:
                case 3:
                    // not supported?
                    break;
                case 4:
                    cost = 16;
                    break;
                case 5:
                    if (itemLevel < 70) {
                        cost = 19;
                        break;
                    }
                    cost = 96;
                    break;
                case 6:
                    if (itemLevel < 70) {
                        cost = 24;
                        break;
                    }
                    cost = 144;
                    break;
            }
        }

        cost = ItemTypeMultiplier(item.Type, cost);
        int enchantLevel = item.Enchant?.Enchants ?? 0;
        if (itemLevel <= 50) {
            cost *= LV_50_ENCHANT_CRYSTAL_FRAGMENT_MULTIPLIER[enchantLevel];
        } else {
            cost *= LV_70_ENCHANT_CRYSTAL_FRAGMENT_MULTIPLIER[enchantLevel];
        }


        return new IngredientInfo(ItemTag.CrystalPiece, (int) cost);
    }

    private static IngredientInfo GetChaosOnyxCost(Item item) {
        int itemLevel = item.Metadata.Limit.Level;
        double cost = 0;
        if (itemLevel is >= 20 and <= 50) {
           cost = -0.001023 * Math.Pow(itemLevel, 2)
                +  0.11095  * itemLevel
                -  0.91968;

           cost = cost * CHAOS_ONYX_RARITY_MULTIPLIER[item.Rarity];
           cost = Math.Floor(cost);
        } else {
            switch (item.Rarity) {
                case 1:
                case 2:
                case 3:
                    break;
                case 4:
                    cost = 2;
                    break;
                case 5:
                    cost = (3.0 / 7.0) * (itemLevel - 56) + 2;
                    break;
                case 6:
                    cost = 12;
                    break;
            }
        }

        cost = ItemTypeMultiplier(item.Type, cost);
        int enchantLevel = item.Enchant?.Enchants ?? 0;
        if (itemLevel <= 50) {
            cost *= LV_50_ENCHANT_CHAOS_ONYX_MULTIPLIER[enchantLevel];
        } else {
            cost *= LV_70_ENCHANT_CHAOS_ONYX_MULTIPLIER[enchantLevel];
        }

        return new IngredientInfo(ItemTag.ChaosOnix, (int) cost);
    }

    private static double ItemTypeMultiplier(ItemType itemType, double cost) {
        //  Two hand weapon/ main hand weapon
        if (itemType.IsBludgeon ||
            itemType.IsLongsword ||
            itemType.IsGreatsword ||
            itemType.IsStaff ||
            itemType.IsScepter ||
            itemType.IsBow ||
            itemType.IsCannon ||
            itemType.IsBlade ||
            itemType.IsKnuckle ||
            itemType.IsOrb) {
            cost *= 2;
        } else if (itemType.IsDagger ||
                   itemType.IsThrowingStar) {
            cost *= 1;
        } else if (itemType.IsClothes ||
                   itemType.IsPants) {
            cost *= 1;
        } else if (itemType.IsOverall) {
            cost *= 2;
        } else if (itemType.IsGloves ||
                   itemType.IsShoes) {
            cost *= 0.1792;
        } else if (itemType.IsHat) {
            cost *= 0.66;
        }

        return cost;
    }
}
