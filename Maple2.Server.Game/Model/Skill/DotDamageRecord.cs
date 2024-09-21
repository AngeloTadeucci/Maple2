﻿using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Formulas;
using Maple2.Server.Game.Util;

namespace Maple2.Server.Game.Model.Skill;

public class DotDamageRecord {
    public readonly IActor Caster;
    public readonly IActor Target;

    public readonly DamagePropertyRecord Properties;
    public int ProcCount { get; init; }

    public readonly DamageType Type;
    public readonly int HpAmount;
    public readonly int SpAmount;
    public readonly int EpAmount;
    public readonly int RecoverHp;

    public DotDamageRecord(IActor caster, IActor target, AdditionalEffectMetadataDot.DotDamage dotDamage) {
        Caster = caster;
        Target = target;
        Properties = new DamagePropertyRecord {
            CanCrit = dotDamage.UseGrade,
            Element = dotDamage.Element,
            AttackType = dotDamage.Type,
            Rate = dotDamage.Rate,
        };
        Type = DamageType.Normal;

        int hpAmount = dotDamage.HpValue;
        if (!dotDamage.IsConstDamage) {
            (DamageType type, long amount) = DamageCalculator.CalculateDamage(caster, target, Properties);
            Type = type;
            hpAmount += (int) amount;
            hpAmount += (int) (dotDamage.DamageByTargetMaxHp * Target.Stats.Values[BasicAttribute.Health].Total);
        }
        if (dotDamage.NotKill) {
            hpAmount = Math.Min(hpAmount, (int) (Target.Stats.Values[BasicAttribute.Health].Current - 1));
        }

        HpAmount = -hpAmount;
        SpAmount = -dotDamage.SpValue;
        EpAmount = -dotDamage.EpValue;
        RecoverHp = (int) (dotDamage.RecoverHpByDamage * HpAmount);
    }
}
