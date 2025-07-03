namespace Maple2.Server.Core.Formulas;

public static class Damage {
    public static double CalculateResistance(double targetPiercingResistance, double casterPiercingMultiplier) {
        return (1500.0 - Math.Max(0, targetPiercingResistance - 1500 * casterPiercingMultiplier)) / 1500;
    }

    /// <summary>
    /// Checks hit rate for evading
    /// </summary>
    public static bool Evade(float targetAccuracyResistance, long targetEvasionTotal, long targetDexeterityTotal, long casterAccuracyTotal, float casterEvasionResistance, long casterLuckTotal) {
        // Unconfirmed
        double casterAccuracy = casterAccuracyTotal * (1 - targetAccuracyResistance);
        double targetEvasion = targetEvasionTotal * (1 - casterEvasionResistance);

        double hitRate = (casterAccuracy - 10) / (targetEvasion + (0.8 * casterAccuracy)) * 2 + Math.Min(0.05, targetDexeterityTotal / (targetDexeterityTotal + 50) * 0.05)
               - Math.Min(0.05, casterLuckTotal / (casterLuckTotal + 50) * 0.05);

        if (Random.Shared.NextDouble() > hitRate) {
            return true;
        }

        return false;
    }

    public static bool TriggerCompulsionEvent(float totalCompulsionRate) {
        if (totalCompulsionRate >= 1 || (totalCompulsionRate > 0 && Random.Shared.NextDouble() <= totalCompulsionRate)) {
            return true;
        }

        return false;
    }
}
