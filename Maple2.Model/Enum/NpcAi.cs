namespace Maple2.Model.Enum;

public enum AiConditionTargetState {
    GrabTarget,
    HoldMe,
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
    MasterHp,
    HitDamage,
    LinkHp,
}

public enum NodeSummonMaster {
    Master,
    Slave,
    None,
}

public enum NodeAiTarget {
    DefaultTarget,
    Hostile,
    Friendly,
}

public enum NodeTargetType {
    Rand,
    Near,
    Far,
    Mid,
    NearAssociated,
    RankAssociated,
    HasAdditional,
    RandAssociated,
    GrabbedUser,
    Random = Rand,
}

public enum NodeJumpType {
    JumpA = 1,
    JumpB = 2,
}

public enum NodeRideType {
    Slave,
}

public enum NodeBuffType {
    Add,
    Remove,
}

public enum NodePopupType : byte {
    Talk,
    CutIn,
}
