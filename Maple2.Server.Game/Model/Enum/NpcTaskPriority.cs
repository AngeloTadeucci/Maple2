namespace Maple2.Server.Game.Model.Enum;

public enum NpcTaskPriority {
    None,
    Cleanup,
    IdleAction, // wander, patrol, bore emote
    BattleStandby,
    BattleWalk, // trace/runaway/move
    BattleAction, // skill cast, jump
    AutoLoot, // pet auto loot
    Stun,
    Interrupt, // push, pull, stagger

    Count
}
