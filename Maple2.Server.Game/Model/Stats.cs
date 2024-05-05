using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Formulas;

namespace Maple2.Server.Game.Model;

public class Stats {
    public const int TOTAL = 35;

    private readonly Dictionary<BasicAttribute, Stat> values;

    public int GearScore => 12345;

    public Stats(JobCode jobCode, short level) {
        values = new Dictionary<BasicAttribute, Stat>();
        Reset(jobCode, level);
    }

    public Stats(NpcMetadataStat npcStats) {
        values = new Dictionary<BasicAttribute, Stat>();
        foreach ((BasicAttribute attribute, long value) in npcStats.Stats) {
            this[attribute].AddBase(value);
        }
    }

    public void Reset(JobCode jobCode, short level) {
        values.Clear();

        this[BasicAttribute.Strength].AddBase(BaseStat.Strength(jobCode, level));
        this[BasicAttribute.Dexterity].AddBase(BaseStat.Dexterity(jobCode, level));
        this[BasicAttribute.Intelligence].AddBase(BaseStat.Intelligence(jobCode, level));
        this[BasicAttribute.Luck].AddBase(BaseStat.Luck(jobCode, level));
        this[BasicAttribute.Health].AddBase(BaseStat.Health(jobCode, level));
        this[BasicAttribute.HpRegen] = new Stat(10);
        this[BasicAttribute.HpRegenInterval] = new Stat(3000);
        this[BasicAttribute.Spirit] = new Stat(100);
        this[BasicAttribute.SpRegen] = new Stat(10);
        this[BasicAttribute.SpRegenInterval] = new Stat(1000);
        this[BasicAttribute.Stamina] = new Stat(120);
        this[BasicAttribute.StaminaRegen] = new Stat(10);
        this[BasicAttribute.StaminaRegenInterval] = new Stat(500);
        this[BasicAttribute.AttackSpeed] = new Stat(100);
        this[BasicAttribute.MovementSpeed] = new Stat(100);
        this[BasicAttribute.Accuracy].AddBase(BaseStat.Accuracy(jobCode, level));
        this[BasicAttribute.Evasion].AddBase(BaseStat.Evasion(jobCode, level));
        this[BasicAttribute.CriticalRate].AddBase(BaseStat.CriticalRate(jobCode, level));
        this[BasicAttribute.CriticalDamage].AddBase(BaseStat.CriticalDamage(jobCode, level));
        this[BasicAttribute.CriticalEvasion].AddBase(BaseStat.CriticalEvasion(jobCode, level));
        this[BasicAttribute.Defense].AddBase(BaseStat.Defense(jobCode, level));
        this[BasicAttribute.JumpHeight] = new Stat(100);
        this[BasicAttribute.PhysicalRes].AddBase(BaseStat.PhysicalRes(jobCode, level));
        this[BasicAttribute.MagicalRes].AddBase(BaseStat.MagicalRes(jobCode, level));
        this[BasicAttribute.MountSpeed] = new Stat(100);

        this[BasicAttribute.PhysicalAtk].AddBase(
            AttackStat.PhysicalAtk(jobCode, this[BasicAttribute.Strength].Base, this[BasicAttribute.Dexterity].Base, this[BasicAttribute.Luck].Base));
        this[BasicAttribute.MagicalAtk].AddBase(AttackStat.MagicalAtk(jobCode, this[BasicAttribute.Intelligence].Base));

#if DEBUG
        this[BasicAttribute.AttackSpeed].AddTotal(40);
        this[BasicAttribute.MovementSpeed].AddTotal(40);
        this[BasicAttribute.JumpHeight].AddTotal(40);
        this[BasicAttribute.MountSpeed].AddTotal(60);
#endif
    }

    public Stat this[BasicAttribute attribute] {
        get {
            if (!values.ContainsKey(attribute)) {
                values[attribute] = new Stat(0, 0, 0);
            }

            return values[attribute];
        }
        set => values[attribute] = value;
    }
}

public sealed class Stat {
    public const int TOTAL = 3;

    public long Total { get; set; }
    public long Base { get; set; }
    public long Current { get; set; }

    public Stat(long value) : this(value, value, value) { }

    public Stat(long total, long @base, long current) {
        Total = total;
        Base = @base;
        Current = current;
    }

    public void AddBase(long amount) {
        Total += amount;
        Base += amount;
        Current += amount;
    }

    public void AddTotal(long amount) {
        Total += amount;
        Current += amount;
    }

    public void Add(long amount) {
        Current = Math.Clamp(Current + amount, 0, Total);
    }

    public long this[int i] {
        get {
            return i switch {
                0 => Total,
                1 => Base,
                _ => Current,
            };
        }
    }

    public override string ToString() => $"<{Total}|{Base}|{Current}>";
}
