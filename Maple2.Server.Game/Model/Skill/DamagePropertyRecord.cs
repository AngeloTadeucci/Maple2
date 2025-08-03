using Maple2.Model.Enum;

namespace Maple2.Server.Game.Model.Skill;

public class DamagePropertyRecord {
    public bool CanCrit { get; init; } = true;
    public Element Element { get; init; } = Element.None;
    public int SkillId { get; init; } = 0;
    public int SkillGroup { get; init; } = 0;
    public RangeType RangeType { get; init; } = RangeType.None;
    public AttackType AttackType { get; init; }
    public CompulsionType[] CompulsionTypes { get; init; } = [];
    public float Rate { get; init; } = 0;
    public long Value { get; init; } = 0;
    public bool SuperArmorBreak { get; set; }
}
