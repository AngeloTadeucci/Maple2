using Maple2.Model.Enum;

namespace Maple2.Server.Core.Formulas;

public static class BaseStat {
    private const short SPLIT_LEVEL = 50;
    private const double RES_DIVISOR = 99.0;

    public static long Strength(JobCode job, short level) {
        long result = job switch {
            JobCode.Newbie => 7,
            JobCode.Knight => 8,
            JobCode.Berserker => 8,
            JobCode.Wizard => 1,
            JobCode.Priest => 1,
            JobCode.Archer => 6,
            JobCode.HeavyGunner => 2,
            JobCode.Thief => 6,
            JobCode.Assassin => 2,
            JobCode.RuneBlader => 8,
            JobCode.Striker => 6,
            JobCode.SoulBinder => 1,
            _ => 1,
        };

        short max = Math.Min(level, SPLIT_LEVEL);
        for (short i = 1; i < max; i++) {
            result += job switch {
                JobCode.Newbie => 7,
                JobCode.Knight => 7,
                JobCode.Berserker => 7,
                JobCode.Wizard => i % 3 == 0 ? 1 : 0,
                JobCode.Priest => i % 3 == 0 ? 1 : 0,
                JobCode.Archer => 1,
                JobCode.HeavyGunner => i % 2 == 1 ? 1 : 0,
                JobCode.Thief => 1,
                JobCode.Assassin => i % 2 == 1 ? 1 : 0,
                JobCode.RuneBlader => 7,
                JobCode.Striker => 1,
                JobCode.SoulBinder => i % 3 == 0 ? 1 : 0,
                _ => 0,
            };
        }
        for (short i = SPLIT_LEVEL; i < level; i++) {
            result += job switch {
                JobCode.Newbie => 1,
                JobCode.Knight => 1,
                JobCode.Berserker => 1,
                JobCode.Archer => i % 3 == 0 ? 1 : 0,
                JobCode.Thief => i % 3 == 0 ? 1 : 0,
                JobCode.RuneBlader => 1,
                JobCode.Striker => i % 3 == 0 ? 1 : 0,
                _ => 0,
            };
        }

        return result;
    }

    public static long Dexterity(JobCode job, short level) {
        long result = job switch {
            JobCode.Newbie => 6,
            JobCode.Knight => 6,
            JobCode.Berserker => 6,
            JobCode.Wizard => 1,
            JobCode.Priest => 1,
            JobCode.Archer => 8,
            JobCode.HeavyGunner => 8,
            JobCode.Thief => 2,
            JobCode.Assassin => 6,
            JobCode.RuneBlader => 6,
            JobCode.Striker => 8,
            JobCode.SoulBinder => 1,
            _ => 1,
        };

        short max = Math.Min(level, SPLIT_LEVEL);
        for (short i = 1; i < max; i++) {
            result += job switch {
                JobCode.Newbie => i % 3 != 2 ? 1 : 0,
                JobCode.Knight => 1,
                JobCode.Berserker => 1,
                JobCode.Wizard => i % 3 == 1 ? 1 : 0,
                JobCode.Priest => i % 3 == 1 ? 1 : 0,
                JobCode.Archer => 7,
                JobCode.HeavyGunner => 7,
                JobCode.Thief => i % 2 == 1 ? 1 : 0,
                JobCode.Assassin => 1,
                JobCode.RuneBlader => 1,
                JobCode.Striker => 7,
                JobCode.SoulBinder => i % 3 == 1 ? 1 : 0,
                _ => 0,
            };
        }
        for (short i = SPLIT_LEVEL; i < level; i++) {
            result += job switch {
                JobCode.Knight => i % 3 == 0 ? 1 : 0,
                JobCode.Berserker => i % 3 == 0 ? 1 : 0,
                JobCode.Archer => 1,
                JobCode.HeavyGunner => 1,
                JobCode.Assassin => i % 3 == 0 ? 1 : 0,
                JobCode.RuneBlader => i % 3 == 0 ? 1 : 0,
                JobCode.Striker => 1,
                _ => 0,
            };
        }

        return result;
    }

    public static long Intelligence(JobCode job, short level) {
        long result = job switch {
            JobCode.Newbie => 2,
            JobCode.Knight => 2,
            JobCode.Berserker => 1,
            JobCode.Wizard => 14,
            JobCode.Priest => 14,
            JobCode.Archer => 1,
            JobCode.HeavyGunner => 1,
            JobCode.Thief => 1,
            JobCode.Assassin => 1,
            JobCode.RuneBlader => 2,
            JobCode.Striker => 1,
            JobCode.SoulBinder => 14,
            _ => 1,
        };

        short max = Math.Min(level, SPLIT_LEVEL);
        for (short i = 1; i < max; i++) {
            result += job switch {
                JobCode.Newbie => i % 3 != 1 ? 1 : 0,
                JobCode.Knight => i % 2 == 0 ? 1 : 0,
                JobCode.Berserker => i % 2 == 1 ? 1 : 0,
                JobCode.Wizard => 8,
                JobCode.Priest => 8,
                JobCode.Archer => i % 2 == 0 ? 1 : 0,
                JobCode.HeavyGunner => i % 2 == 0 ? 1 : 0,
                JobCode.Thief => i % 2 == 0 ? 1 : 0,
                JobCode.Assassin => i % 2 == 0 ? 1 : 0,
                JobCode.RuneBlader => i % 2 == 0 ? 1 : 0,
                JobCode.Striker => i % 2 == 1 ? 1 : 0,
                JobCode.SoulBinder => 8,
                _ => 0,
            };
        }
        for (short i = SPLIT_LEVEL; i < level; i++) {
            result += job switch {
                JobCode.Wizard => 1,
                JobCode.Priest => 1,
                JobCode.SoulBinder => 1,
                _ => 0,
            };
        }

        return result;
    }

    public static long Luck(JobCode job, short level) {
        long result = job switch {
            JobCode.Newbie => 2,
            JobCode.Knight => 1,
            JobCode.Berserker => 2,
            JobCode.Wizard => 1,
            JobCode.Priest => 1,
            JobCode.Archer => 2,
            JobCode.HeavyGunner => 6,
            JobCode.Thief => 8,
            JobCode.Assassin => 8,
            JobCode.RuneBlader => 1,
            JobCode.Striker => 2,
            JobCode.SoulBinder => 1,
            _ => 1,
        };

        short max = Math.Min(level, SPLIT_LEVEL);
        for (short i = 1; i < max; i++) {
            result += job switch {
                JobCode.Newbie => i % 3 != 0 ? 1 : 0,
                JobCode.Knight => i % 2 == 1 ? 1 : 0,
                JobCode.Berserker => i % 2 == 0 ? 1 : 0,
                JobCode.Wizard => i % 3 == 2 ? 1 : 0,
                JobCode.Priest => i % 3 == 2 ? 1 : 0,
                JobCode.Archer => i % 2 == 1 ? 1 : 0,
                JobCode.HeavyGunner => 1,
                JobCode.Thief => 7,
                JobCode.Assassin => 7,
                JobCode.RuneBlader => i % 2 == 1 ? 1 : 0,
                JobCode.Striker => i % 2 == 0 ? 1 : 0,
                JobCode.SoulBinder => i % 3 == 2 ? 1 : 0,
                _ => 0,
            };
        }
        for (short i = SPLIT_LEVEL; i < level; i++) {
            result += job switch {
                JobCode.HeavyGunner => i % 3 == 0 ? 1 : 0,
                JobCode.Thief => 1,
                JobCode.Assassin => 1,
                _ => 0,
            };
        }

        return result;
    }

    public static long Health(JobCode job, short level) {
        double result = 50;
        short max = Math.Min(level, SPLIT_LEVEL);
        for (short i = 0; i < max; i++) {
            result += job switch {
                JobCode.Newbie => 66,
                JobCode.Knight => 72,
                JobCode.Berserker => 85,
                JobCode.Wizard => 60,
                JobCode.Priest => 66,
                JobCode.Archer => 61,
                JobCode.HeavyGunner => 67.5,
                JobCode.Thief => 65,
                JobCode.Assassin => 60,
                JobCode.RuneBlader => 69,
                JobCode.Striker => 69,
                JobCode.SoulBinder => 65,
                _ => 50,
            } * (Math.Atan(0.22 * i - 1.4) / Math.PI + 0.5);
        }

        for (short i = SPLIT_LEVEL; i < level; i++) {
            // ReSharper disable once PossibleLossOfFraction
            result += 11.5 + (i - SPLIT_LEVEL) / 10 * 0.5;
        }

        return (long) Math.Ceiling(result);
    }

    public static long Accuracy(JobCode job, short level) => 82;

    public static long Evasion(JobCode job, short level) {
        return job switch {
            JobCode.Newbie => 70,
            JobCode.Knight => 70,
            JobCode.Berserker => 72,
            JobCode.Wizard => 70,
            JobCode.Priest => 70,
            JobCode.Archer => 77,
            JobCode.HeavyGunner => 77,
            JobCode.Thief => 80,
            JobCode.Assassin => 77,
            JobCode.RuneBlader => 77,
            JobCode.Striker => 76,
            JobCode.SoulBinder => 76,
            _ => 70,
        };
    }

    public static long CriticalRate(JobCode job, short level) {
        return job switch {
            JobCode.Newbie => 35,
            JobCode.Knight => 45,
            JobCode.Berserker => 47,
            JobCode.Wizard => 40,
            JobCode.Priest => 45,
            JobCode.Archer => 55,
            JobCode.HeavyGunner => 52,
            JobCode.Thief => 50,
            JobCode.Assassin => 53,
            JobCode.RuneBlader => 46,
            JobCode.Striker => 48,
            JobCode.SoulBinder => 48,
            _ => 35,
        };
    }

    public static long CriticalDamage(JobCode job, short level) => 125;

    public static long CriticalEvasion(JobCode job, short level) => 50;

    public static long Defense(JobCode job, short level) => level;

    // TODO: This is not actually accurate
    public static long PhysicalRes(JobCode job, short level) {
        return (long) Math.Ceiling(job switch {
            JobCode.Newbie => 15.0 * (level / RES_DIVISOR),
            JobCode.Knight => 55.0 * (level / RES_DIVISOR),
            JobCode.Berserker => 55.0 * (level / RES_DIVISOR),
            JobCode.Wizard => 15.0 * (level / RES_DIVISOR),
            JobCode.Priest => 15.0 * (level / RES_DIVISOR),
            JobCode.Archer => 35.0 * (level / RES_DIVISOR),
            JobCode.HeavyGunner => 50.0 * (level / RES_DIVISOR),
            JobCode.Thief => 15.0 * (level / RES_DIVISOR),
            JobCode.Assassin => 15.0 * (level / RES_DIVISOR),
            JobCode.RuneBlader => 55.0 * (level / RES_DIVISOR),
            JobCode.Striker => 55.0 * (level / RES_DIVISOR),
            JobCode.SoulBinder => 15.0 * (level / RES_DIVISOR),
            _ => 25.0 * (level / RES_DIVISOR),
        });
    }

    // TODO: This is not actually accurate
    public static long MagicalRes(JobCode job, short level) {
        return (long) Math.Ceiling(job switch {
            JobCode.Newbie => 15.0 * (level / RES_DIVISOR),
            JobCode.Knight => 15.0 * (level / RES_DIVISOR),
            JobCode.Berserker => 15.0 * (level / RES_DIVISOR),
            JobCode.Wizard => 50.0 * (level / RES_DIVISOR),
            JobCode.Priest => 55.0 * (level / RES_DIVISOR),
            JobCode.Archer => 15.0 * (level / RES_DIVISOR),
            JobCode.HeavyGunner => 15.0 * (level / RES_DIVISOR),
            JobCode.Thief => 15.0 * (level / RES_DIVISOR),
            JobCode.Assassin => 15.0 * (level / RES_DIVISOR),
            JobCode.RuneBlader => 5.0 * (level / RES_DIVISOR),
            JobCode.Striker => 15.0 * (level / RES_DIVISOR),
            JobCode.SoulBinder => 50.0 * (level / RES_DIVISOR),
            _ => 25.0 * (level / RES_DIVISOR),
        });
    }
}
