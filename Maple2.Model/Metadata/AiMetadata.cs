using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using Maple2.Model.Enum;
using Maple2.Model.Game.Event;

namespace Maple2.Model.Metadata;

public enum AiConditionTargetState {
    grabTarget,
    holdme
}

public enum AiConditionOp {
    Equal,
    Greater,
    Less,
    GreaterEqual,
    LessEqual,
}

public enum NodeSummonOption {
    none,
    masterHP,
    hitDamage,
    linkHP
}

public enum NodeSummonMaster {
    Master,
    Slave,
    None
}

public enum NodeAiTarget {
    defaultTarget,
    hostile,
    friendly
}

public enum NodeTargetType {
    rand,
    near,
    far,
    mid,
    nearAssociated,
    rankAssociate,
    hasadditional,
    randassociated,
    grabbeduser,
    random
}

public enum NodeJumpType {
    JumpA = 1,
    JumpB = 2
}

public enum NodeRideType {
    Slave
}

public enum NodeBuffType {
    remove,
    add
}

public enum NodePopupType {
    talk,
    cutin
}

public record AiMetadata(
    string Name,
    AiMetadata.Condition[] Reserved,
    AiMetadata.Node[] Battle,
    AiMetadata.Node[] BattleEnd,
    AiMetadata.AiPresetDefinition[] AiPresets) {

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "!")]
    [JsonDerivedType(typeof(TraceNode), typeDiscriminator: nameof(TraceNode))]
    [JsonDerivedType(typeof(SkillNode), typeDiscriminator: nameof(SkillNode))]
    [JsonDerivedType(typeof(TeleportNode), typeDiscriminator: nameof(TeleportNode))]
    [JsonDerivedType(typeof(StandbyNode), typeDiscriminator: nameof(StandbyNode))]
    [JsonDerivedType(typeof(SetDataNode), typeDiscriminator: nameof(SetDataNode))]
    [JsonDerivedType(typeof(TargetNode), typeDiscriminator: nameof(TargetNode))]
    [JsonDerivedType(typeof(SayNode), typeDiscriminator: nameof(SayNode))]
    [JsonDerivedType(typeof(SetValueNode), typeDiscriminator: nameof(SetValueNode))]
    [JsonDerivedType(typeof(ConditionsNode), typeDiscriminator: nameof(ConditionsNode))]
    [JsonDerivedType(typeof(JumpNode), typeDiscriminator: nameof(JumpNode))]
    [JsonDerivedType(typeof(SelectNode), typeDiscriminator: nameof(SelectNode))]
    [JsonDerivedType(typeof(MoveNode), typeDiscriminator: nameof(MoveNode))]
    [JsonDerivedType(typeof(SummonNode), typeDiscriminator: nameof(SummonNode))]
    [JsonDerivedType(typeof(HideVibrateAllNode), typeDiscriminator: nameof(HideVibrateAllNode))]
    [JsonDerivedType(typeof(TriggerSetUserValueNode), typeDiscriminator: nameof(TriggerSetUserValueNode))]
    [JsonDerivedType(typeof(RideNode), typeDiscriminator: nameof(RideNode))]
    [JsonDerivedType(typeof(SetSlaveValueNode), typeDiscriminator: nameof(SetSlaveValueNode))]
    [JsonDerivedType(typeof(SetMasterValueNode), typeDiscriminator: nameof(SetMasterValueNode))]
    [JsonDerivedType(typeof(RunawayNode), typeDiscriminator: nameof(RunawayNode))]
    [JsonDerivedType(typeof(MinimumHpNode), typeDiscriminator: nameof(MinimumHpNode))]
    [JsonDerivedType(typeof(BuffNode), typeDiscriminator: nameof(BuffNode))]
    [JsonDerivedType(typeof(TargetEffectNode), typeDiscriminator: nameof(TargetEffectNode))]
    [JsonDerivedType(typeof(ShowVibrateNode), typeDiscriminator: nameof(ShowVibrateNode))]
    [JsonDerivedType(typeof(SidePopupNode), typeDiscriminator: nameof(SidePopupNode))]
    [JsonDerivedType(typeof(SetValueRangeTargetNode), typeDiscriminator: nameof(SetValueRangeTargetNode))]
    [JsonDerivedType(typeof(AnnounceNode), typeDiscriminator: nameof(AnnounceNode))]
    [JsonDerivedType(typeof(ModifyRoomTimeNode), typeDiscriminator: nameof(ModifyRoomTimeNode))]
    [JsonDerivedType(typeof(TriggerModifyUserValueNode), typeDiscriminator: nameof(TriggerModifyUserValueNode))]
    [JsonDerivedType(typeof(RemoveSlavesNode), typeDiscriminator: nameof(RemoveSlavesNode))]
    [JsonDerivedType(typeof(CreateRandomRoomNode), typeDiscriminator: nameof(CreateRandomRoomNode))]
    [JsonDerivedType(typeof(CreateInteractObjectNode), typeDiscriminator: nameof(CreateInteractObjectNode))]
    [JsonDerivedType(typeof(RemoveMeNode), typeDiscriminator: nameof(RemoveMeNode))]
    [JsonDerivedType(typeof(SuicideNode), typeDiscriminator: nameof(SuicideNode))]
    public record Node(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets) {
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "!")]
    [JsonDerivedType(typeof(DistanceOverCondition), typeDiscriminator: nameof(DistanceOverCondition))]
    [JsonDerivedType(typeof(CombatTimeCondition), typeDiscriminator: nameof(CombatTimeCondition))]
    [JsonDerivedType(typeof(DistanceLessCondition), typeDiscriminator: nameof(DistanceLessCondition))]
    [JsonDerivedType(typeof(SkillRangeCondition), typeDiscriminator: nameof(SkillRangeCondition))]
    [JsonDerivedType(typeof(ExtraDataCondition), typeDiscriminator: nameof(ExtraDataCondition))]
    [JsonDerivedType(typeof(SlaveCountCondition), typeDiscriminator: nameof(SlaveCountCondition))]
    [JsonDerivedType(typeof(HpOverCondition), typeDiscriminator: nameof(HpOverCondition))]
    [JsonDerivedType(typeof(StateCondition), typeDiscriminator: nameof(StateCondition))]
    [JsonDerivedType(typeof(AdditionalCondition), typeDiscriminator: nameof(AdditionalCondition))]
    [JsonDerivedType(typeof(HpLessCondition), typeDiscriminator: nameof(HpLessCondition))]
    [JsonDerivedType(typeof(SlaveCountOpCondition), typeDiscriminator: nameof(SlaveCountOpCondition))]
    [JsonDerivedType(typeof(TrueCondition), typeDiscriminator: nameof(TrueCondition))]
    public record Condition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets) {
    }

    public record AiPresetDefinition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets) {
    }

    public record AiPreset(
        string Name) {
    }

    #region Nodes
    public record TraceNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int Limit,
        int SkillIdx,
        string Animation,
        int Speed,
        int Till,
        long InitialCooltime,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record SkillNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int Idx,
        short Level,
        int Prob,
        bool Sequence,
        Vector3 FacePos,
        int FaceTarget,
        int FaceTargetTick,
        long InitialCooltime,
        long Cooltime,
        int Limit,
        int Rob,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record TeleportNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        Vector3 Pos,
        int Prob,
        Vector3 FacePos,
        int FaceTarget,
        long InitialCooltime,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record StandbyNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int Limit,
        int Prob,
        string Animation, // kfm anim name
        Vector3 FacePos,
        int FaceTarget,
        long InitialCooltime,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record SetDataNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        string Key,
        int Value,
        long Cooltime) : Node(Name, Nodes, AiPresets);

	public record TargetNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        NodeTargetType Type,
        int Prob,
        int Rank,
        int AdditionalId,
        short AdditionalLevel,
        int From,
        int To,
        Vector3 Center,
        NodeAiTarget Target, // hostile, friendly
        bool NoChangeWhenNoTarget,
        long InitialCooltime,
        long Cooltime,
        int Rob,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record SayNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        string Message,
        int Prob,
        int DurationTick,
        int DelayTick,
        long InitialCooltime,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record SetValueNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        string Key,
        int Value,
        long InitialCooltime,
        long Cooltime,
        bool IsModify,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record ConditionsNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        Condition[] Conditions,
        long InitialCooltime,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record JumpNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        Vector3 Pos,
        int Speed,
        float HeightMultiplier,
        NodeJumpType Type,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record SelectNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int[] Prob,
        bool useNpcProb) : Node(Name, Nodes, AiPresets);

	public record MoveNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        Vector3 Destination,
        int Prob,
        string Animation, // kfm anim name
        int Limit,
        int Speed,
        int FaceTarget,
        long InitialCooltime,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record SummonNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int NpcId,
        int NpcCountMax,
        int NpcCount,
        int DelayTick,
        int LifeTime,
        Vector3 SummonRot,
        Vector3 SummonPos,
        Vector3 SummonPosOffset,
        Vector3 SummonTargetOffset,
        Vector3 SummonRadius,
        int Group,
        NodeSummonMaster Master, // Slave, None
        NodeSummonOption[] Option, // masterHP,hitDamage
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record TriggerSetUserValueNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int TriggerID,
        string Key,
        int Value,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record RideNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        NodeRideType Type,
        bool IsRideOff,
        int[] RideNpcIDs) : Node(Name, Nodes, AiPresets);

	public record SetSlaveValueNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        string Key,
        int Value,
        bool IsRandom,
        long Cooltime,
        bool IsModify,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record SetMasterValueNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        string Key,
        int Value,
        bool IsRandom,
        long Cooltime,
        bool IsModify,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record RunawayNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        string Animation, // kfm anim name
        int SkillIdx,
        int Till,
        int Limit,
        Vector3 FacePos,
        long InitialCooltime,
        long Cooltime) : Node(Name, Nodes, AiPresets);

	public record MinimumHpNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        float HpPercent) : Node(Name, Nodes, AiPresets);

	public record BuffNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int Id,
        NodeBuffType Type,
        short Level,
        int Prob,
        long InitialCooltime,
        long Cooltime,
        bool IsTarget,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record TargetEffectNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        string EffectName // xml effect
        ) : Node(Name, Nodes, AiPresets);

	public record ShowVibrateNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int GroupID) : Node(Name, Nodes, AiPresets);

	public record SidePopupNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        NodePopupType Type,
        string Illust, // side popup asset name
        int Duration,
        string Script,
        string Sound, // sound asset name
        string Voice // voice asset path
        ) : Node(Name, Nodes, AiPresets);

	public record SetValueRangeTargetNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        string Key,
        int Value,
        int Height,
        int Radius,
        long Cooltime,
        bool IsModify,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

	public record AnnounceNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        string Message,
        int DurationTick,
        long Cooltime) : Node(Name, Nodes, AiPresets);

	public record ModifyRoomTimeNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int TimeTick,
        bool IsShowEffect) : Node(Name, Nodes, AiPresets);
    public record HideVibrateAllNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

    public record TriggerModifyUserValueNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int TriggerID,
        string Key,
        int Value) : Node(Name, Nodes, AiPresets);

	public record RemoveSlavesNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        bool IsKeepBattle) : Node(Name, Nodes, AiPresets);

    public record CreateRandomRoomNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int randomRoomID,
        int portalDuration) : Node(Name, Nodes, AiPresets);

    public record CreateInteractObjectNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        string Normal, // kfm anim name
        int InteractID,
        int LifeTime,
        string KfmName,
        string Reactable // kfm anim name
        ) : Node(Name, Nodes, AiPresets);

	public record RemoveMeNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets) : Node(Name, Nodes, AiPresets);

    public record SuicideNode(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets) : Node(Name, Nodes, AiPresets);
    #endregion

    #region Conditions
    public record DistanceOverCondition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int Value) : Condition(Name, Nodes, AiPresets);

	public record CombatTimeCondition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int BattleTimeBegin,
        int BattleTimeLoop,
        int BattleTimeEnd) : Condition(Name, Nodes, AiPresets);

	public record DistanceLessCondition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int Value) : Condition(Name, Nodes, AiPresets);

	public record SkillRangeCondition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int SkillIdx,
        short SkillLev,
        bool IsKeepBattle) : Condition(Name, Nodes, AiPresets);

	public record ExtraDataCondition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        string Key,
        int Value,
        AiConditionOp Op,
        bool IsKeepBattle) : Condition(Name, Nodes, AiPresets);

	public record SlaveCountCondition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int Count,
        bool UseSummonGroup,
        int SummonGroup) : Condition(Name, Nodes, AiPresets);

    public record SlaveCountOpCondition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int SlaveCount,
        AiConditionOp SlaveCountOp) : Condition(Name, Nodes, AiPresets);

    public record HpOverCondition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int Value) : Condition(Name, Nodes, AiPresets);

	public record StateCondition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        AiConditionTargetState TargetState) : Condition(Name, Nodes, AiPresets);

	public record AdditionalCondition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int Id,
        short Level,
        int OverlapCount,
        bool IsTarget) : Condition(Name, Nodes, AiPresets);

	public record HpLessCondition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets,
        int Value) : Condition(Name, Nodes, AiPresets);

	public record TrueCondition(
        string Name,
        Node[] Nodes,
        AiPreset[] AiPresets) : Condition(Name, Nodes, AiPresets);
    #endregion
}
