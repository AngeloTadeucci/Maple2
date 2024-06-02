using Maple2.Model.Enum;

namespace Maple2.Server.Game.Model.Skill;

public class DamagePropertyRecord {
    public bool CanCrit { get; init; }
    public Element Element { get; init; }
    public int SkillId { get; init; }
    public int SkillGroup { get; init; }
    public RangeType RangeType { get; init; }
    public AttackType AttackType { get; init; }
    public CompulsionType[] CompulsionTypes { get; init; }
    public float Rate { get; init; }
    public long Value { get; init; }

    public DamagePropertyRecord() {
        CanCrit = true;
        Element = Element.None;
        SkillId = 0;
        SkillGroup = 0;
        RangeType = RangeType.None;
        CompulsionTypes = [];
        Rate = 0;
        Value = 0;
    }
}
