namespace Maple2.Model.Enum;

public enum BuffType {
    None = 0,
    Buff = 1,
    Debuff = 2,
    Debuff2 = 3, // Also a debuff?
}

[Flags]
public enum BuffSubType {
    None = 0,
    Buff = 1, // Used for lots of random things
    Status = 2,
    Damage = 4,
    Motion = 8,
    Recovery = 16,
    Consumable = 32, // Healing, Souvenir, ???
    PcBang = 64,
    Fishing = 128,
    Guild = 256,
    Lapenta = 512,
    Prestige = 1024,
}

public enum BuffCategory {
    None = 0,
    Unknown1 = 1,
    Unknown2 = 2,
    Unknown4 = 4,
    EnemyDot = 6,
    Stunned = 7, // ?
    Slow = 8,
    BossResistance = 9,
    Unknown99 = 99,
    MonsterStunned = 1007, // ?
    Unknown2001 = 2001,
}

public enum BuffEventType {
    None = 0,
    AutoFish = 1,
    SafeRiding = 2,
    AmphibiousRide = 3,
    AutoPerform = 4,
}

public enum BuffKeepCondition {
    TimerDuration = 0, // ?
    SkillDuration = 1, // ?
    TimerDurationTrackCooldown = 5, // ?
    UnlimitedDuration = 99,
}

public enum BuffResetCondition {
    ResetEndTick = 0,
    PersistEndTick = 1, // end tick does not reset
    Reset2 = 2, // behaves the same as Reset ??
    Replace = 3, // Removes old buff and adds a new
}

public enum BuffDotCondition {
    Default = 0, // Maybe activate on enable?
    OnInterval = 1,
    Stack = 2,
}

[Flags]
public enum BuffFlag {
    None = 0,
    UpdateBuff = 1,
    UpdateShield = 2,
}

public enum InvokeEffectType : byte {
    ReduceCooldown = 1,
    IncreaseSkillDamage = 2,
    IncreaseEffectDuration = 3,
    IncreaseDotDamage = 5,
    // 20 (90050324)
    // 21 (90050324) // Triggered from 10200081 (Adrenaline Rush)
    IncreaseEvasionDebuff = 23,
    IncreaseCritEvasionDebuff = 26,
    // 34 (90050853) // Adds 9% weapon attack to the triggered effect of Explosive Panic or Explosive Fervor cannons.
    // 35 (90050854) // Triggered from 90050852 - Increases weapon attack by 4.5% when still for at least 1.5 sec."
    // 38 (90050806) // Adds 2% physical damage to the triggered effect of the Barbaric Extreme Longsword.
    // 40 (90050827) // Adds 5.2% magic damage to the triggered effect of the Holy Extreme Scepter.
    ReduceSpiritCost = 56,
    // 57 (90050351) // Triggered from 10500061 (Sharp Eyes) -
    IncreaseHealing = 58,
}

public enum BuffCompulsionEventType : byte {
    None = 0,
    CritChanceOverride = 1,
    EvasionChanceOverride = 2,
    BlockChance = 3,
}
