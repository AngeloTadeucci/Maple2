using Maple2.Model.Enum;
using Maple2.Model.Game;

namespace Maple2.Server.Core.Formulas;

public static class LimitBreak {
    private const long MESO_COST_BASE = 1171000;
    private static readonly float[] MESO_COST_MULTIPLIER = [1.0f, 1.666f, 2.332f, 3.585f, 5.319f, 7.053f, 13.722f, 24.431f, 35.147f, 40.032f];

    private const ItemTag INGREDIENT_TAG_1 = ItemTag.ChaosOnix;
    private const ItemTag INGREDIENT_TAG_2 = ItemTag.Onix;
    private const ItemTag INGREDIENT_TAG_3 = ItemTag.PrismShard;
    private const ItemTag INGREDIENT_TAG_4 = ItemTag.PrismStone;

    private const int INGREDIENT_TAG_1_COST_BASE = 54;
    private const int INGREDIENT_TAG_2_COST_BASE = 3510;
    private const int INGREDIENT_TAG_3_COST_BASE = 64;
    private const int INGREDIENT_TAG_4_COST_BASE = 4;

    private static readonly float[] INGREDIENT_1_COST_MULTIPLIER = [1.0f, 1.37f, 1.74f, 2.93f, 4.78f, 6.63f, 11.13f, 17.80f, 24.46f, 27.46f];
    private static readonly float[] INGREDIENT_2_COST_MULTIPLIER = [1.0f, 1.37f, 1.74f, 2.93f, 4.78f, 6.63f, 11.13f, 17.80f, 24.46f, 27.46f];
    private static readonly float[] INGREDIENT_3_COST_MULTIPLIER = [1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f, 10.0f];
    private static readonly float[] INGREDIENT_4_COST_MULTIPLIER = [1.0f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f, 10.0f];

    public static long MesoCost(int limitBreakLevel) {
        int index = limitBreakLevel / 10;

        index = Math.Min(index, MESO_COST_MULTIPLIER.Length - 1);
        return (long) Math.Round(MESO_COST_BASE * MESO_COST_MULTIPLIER[index] / 100.0) * 100;
    }

    public static List<IngredientInfo> GetCatalysts(int limitBreakLevel) {
        List<IngredientInfo> costs = [];
        int index = limitBreakLevel / 10;
        index = Math.Min(index, INGREDIENT_1_COST_MULTIPLIER.Length - 1);

        costs.Add(new IngredientInfo(INGREDIENT_TAG_1, (int) (INGREDIENT_TAG_1_COST_BASE * INGREDIENT_1_COST_MULTIPLIER[index])));
        costs.Add(new IngredientInfo(INGREDIENT_TAG_2, (int) (INGREDIENT_TAG_2_COST_BASE * INGREDIENT_2_COST_MULTIPLIER[index])));
        costs.Add(new IngredientInfo(INGREDIENT_TAG_3, (int) (INGREDIENT_TAG_3_COST_BASE * INGREDIENT_3_COST_MULTIPLIER[index])));
        costs.Add(new IngredientInfo(INGREDIENT_TAG_4, (int) (INGREDIENT_TAG_4_COST_BASE * INGREDIENT_4_COST_MULTIPLIER[index])));

        return costs;
    }

}
