using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.Server.Game.Model.Skill;

public class DotDamageRecord {
    public readonly IActor Caster;
    public readonly IActor Target;
    public int ProcCount { get; init; }

    public readonly DamageType Type;
    public readonly int HpAmount;
    public readonly int SpAmount;
    public readonly int EpAmount;
    public readonly int RecoverHp;

    public DotDamageRecord(IActor caster, IActor target, AdditionalEffectMetadataDot.DotDamage dotDamage) {
        Caster = caster;
        Target = target;

        int hpAmount = dotDamage.HpValue;
        if (!dotDamage.IsConstDamage) {
            hpAmount += (int) (dotDamage.Rate * Math.Max(Caster.Stats[BasicAttribute.PhysicalAtk].Current, Caster.Stats[BasicAttribute.MagicalAtk].Current));
            hpAmount += (int) (dotDamage.DamageByTargetMaxHp * Target.Stats[BasicAttribute.Health].Total);
        }
        if (dotDamage.NotKill) {
            hpAmount = Math.Min(hpAmount, (int) (Target.Stats[BasicAttribute.Health].Current - 1));
        }

        Type = DamageType.Normal;
        HpAmount = -hpAmount;
        SpAmount = -dotDamage.SpValue;
        EpAmount = -dotDamage.EpValue;
        RecoverHp = (int) (dotDamage.RecoverHpByDamage * HpAmount);
    }
}
