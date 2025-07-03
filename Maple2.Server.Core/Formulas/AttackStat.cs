using Maple2.Model.Enum;

namespace Maple2.Server.Core.Formulas;

public static class AttackStat {
    private const float PRIMARY = 19.0f / 30.0f;
    private const float SECONDARY = 1.0f / 6.0f;

    public static long PhysicalAtk(JobCode jobCode, long str, long dex, long luk) {
        (float strCoefficient, float dexCoefficient, float lukCoefficient) = jobCode switch {
            JobCode.Newbie => (0.0f, 0.0f, 0.0f),
            JobCode.Knight => (PRIMARY, SECONDARY, 0.0f),
            JobCode.Berserker => (PRIMARY, SECONDARY, 0.0f),
            JobCode.Wizard => (0.5666f, SECONDARY, 0.0f),
            JobCode.Priest => (0.4721f, SECONDARY, 0.0f),
            JobCode.Archer => (SECONDARY, PRIMARY, 0.0f),
            JobCode.HeavyGunner => (0.0f, PRIMARY, SECONDARY),
            JobCode.Thief => (SECONDARY, 0.0f, PRIMARY),
            JobCode.Assassin => (0.0f, SECONDARY, PRIMARY),
            JobCode.RuneBlader => (PRIMARY, SECONDARY, 0.0f),
            JobCode.Striker => (SECONDARY, PRIMARY, 0.0f),
            JobCode.SoulBinder => (0.5666f, SECONDARY, 0.0f),
            _ => (0.0f, 0.0f, 0.0f),
        };

        return (long) (strCoefficient * str + dexCoefficient * dex + lukCoefficient * luk);
    }

    public static long MagicalAtk(JobCode jobCode, long @int) {
        float intCoefficient = jobCode switch {
            JobCode.Newbie => 0.0f,
            JobCode.Knight => PRIMARY,
            JobCode.Berserker => PRIMARY,
            JobCode.Wizard => 0.5666f,
            JobCode.Priest => 0.4721f,
            JobCode.Archer => PRIMARY,
            JobCode.HeavyGunner => PRIMARY,
            JobCode.Thief => PRIMARY,
            JobCode.Assassin => PRIMARY,
            JobCode.RuneBlader => 0.5666f,
            JobCode.Striker => PRIMARY,
            JobCode.SoulBinder => 0.5666f,
            _ => 0.0f,
        };

        return (long) (intCoefficient * @int);
    }
}
