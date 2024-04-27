namespace Maple2.Model.Enum;

public enum InvokeEffectType : byte {
    ReduceCooldown = 1,
    IncreaseSkillDamage = 2,
    IncreaseEffectDuration = 3,
    IncreaseDotDamage = 5,
    // 20 (90050324)
    // 21 (90050324)
    IncreaseEvasionDebuff = 23,
    IncreaseCritEvasionDebuff = 26,
    // 34 (90050853)
    // 35 (90050854)
    // 38 (90050806)
    // 40 (90050827)
    ReduceSpiritCost = 56, // flame wave 0 spirit cost? value=0, rate=2 on 10300250. subtract rate from cost (10500201, rate=20%)
    // 57 (90050351)
    IncreaseHealing = 58,
}
