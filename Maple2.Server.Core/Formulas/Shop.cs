using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;

namespace Maple2.Server.Core.Formulas;

public static class Shop {
    // Hardcoded values used for items with level >= 57
    private static readonly int[] PRICES = [1541, 1926, 2465, 9256, 11339, 13653];

    public static long SellPrice(ItemMetadata metadata, ItemType type, int rarity) {
        if (metadata.Limit.Level >= 57) {
            return PRICES[rarity - 1];
        }

        if (metadata.Property.CustomSellPrices[rarity - 1] > 0) {
            if (rarity < 4 && (type.IsArmor || type.IsWeapon)) {
                return (long) Math.Floor(metadata.Property.CustomSellPrices[rarity - 1] * 0.333);
            }
            return metadata.Property.CustomSellPrices[rarity - 1];
        }

        if (metadata.Limit.Level < 57 && rarity < 4 && (type.IsArmor || type.IsWeapon)) {
            return (long) Math.Floor(metadata.Property.SellPrices[rarity - 1] * 0.333);
        }

        return metadata.Property.SellPrices[rarity - 1];
    }

    public static (ShopCurrencyType CurrencyType, int Cost) ExcessRestockCost(ShopCurrencyType excessCurrencyType, int restockCount) {
        return restockCount switch {
            > 0 and <= 5 => (Constant.InitialTierExcessRestockCurrency, 50000),
            6 => (excessCurrencyType, 10),
            7 => (excessCurrencyType, 20),
            8 => (excessCurrencyType, 40),
            9 => (excessCurrencyType, 60),
            10 => (excessCurrencyType, 80),
            > 10 and <= 15 => (excessCurrencyType, 100),
            _ => (excessCurrencyType, 150),
        };
    }
}
