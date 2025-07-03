namespace Maple2.Server.Core.Formulas;

public static class ItemMerge {

    public static int CostMultiplier(int rarity) {
        return rarity switch {
            1 or 2 or 3 => 1,
            4 => 2,
            5 => 4,
            6 => 6,
            _ => 1,
        };
    }
}
