namespace Maple2.Model.Enum;

public enum AiConditionTargetState {
    GrabTarget,
    Holdme
}

public enum AiConditionOp {
    Equal,
    Greater,
    Less,
    GreaterEqual,
    LessEqual,
}

public enum NodeSummonOption {
    None,
    MasterHP,
    HitDamage,
    LinkHP
}

public enum NodeSummonMaster {
    Master,
    Slave,
    None
}

public enum NodeAiTarget {
    DefaultTarget,
    Hostile,
    Friendly
}

public enum NodeTargetType {
    Rand,
    Near,
    Far,
    Mid,
    NearAssociated,
    RankAssociate,
    Hasadditional,
    Randassociated,
    Grabbeduser,
    Random
}

public enum NodeJumpType {
    JumpA = 1,
    JumpB = 2
}

public enum NodeRideType {
    Slave
}

public enum NodeBuffType {
    Remove,
    Add
}

public enum NodePopupType {
    Talk,
    CutIn
}
