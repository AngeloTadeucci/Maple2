using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using Maple2.Model.Enum;
using Maple2.Model.Game.Event;

namespace Maple2.Model.Metadata;

public record AiMetadata(
    string Name,
    AiMetadata.Condition[] Reserved,
    AiMetadata.Entry[] Battle,
    AiMetadata.Entry[] BattleEnd,
    AiMetadata.AiPresetDefinition[] AiPresets) {

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "!")]
    [JsonDerivedType(typeof(AiPreset), typeDiscriminator: nameof(AiPreset))]
    [JsonDerivedType(typeof(Node), typeDiscriminator: nameof(Node))]
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
    [JsonDerivedType(typeof(AiPresetDefinition), typeDiscriminator: nameof(AiPresetDefinition))]
    public record Entry(
        string Name);

    public record Node(
        string Name,
        Entry[] Entries) : Entry(Name) {
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
        Entry[] Entries) : Node(Name, Entries);

    public record AiPresetDefinition(
        string Name,
        Entry[] Entries) : Node(Name, Entries) {
    }

    public record AiPreset(
        string Name) : Entry(Name) {
    }

    #region Nodes
    public record TraceNode(
        string Name,
        Entry[] Entries,
        int Limit,
        int SkillIdx,
        string Animation,
        int Speed,
        int Till,
        long InitialCooltime,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Entries);

	public record SkillNode(
        string Name,
        Entry[] Entries,
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
        bool IsKeepBattle) : Node(Name, Entries);

	public record TeleportNode(
        string Name,
        Entry[] Entries,
        Vector3 Pos,
        int Prob,
        Vector3 FacePos,
        int FaceTarget,
        long InitialCooltime,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Entries);

	public record StandbyNode(
        string Name,
        Entry[] Entries,
        int Limit,
        int Prob,
        string Animation, // kfm anim name
        Vector3 FacePos,
        int FaceTarget,
        long InitialCooltime,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Entries);

	public record SetDataNode(
        string Name,
        Entry[] Entries,
        string Key,
        int Value,
        long Cooltime) : Node(Name, Entries);

	public record TargetNode(
        string Name,
        Entry[] Entries,
        NodeTargetType Type,
        int Prob,
        int Rank,
        int AdditionalId,
        short AdditionalLevel,
        int From,
        int To,
        Vector3 Center,
        NodeAiTarget Target,
        bool NoChangeWhenNoTarget,
        long InitialCooltime,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Entries);

	public record SayNode(
        string Name,
        Entry[] Entries,
        string Message,
        int Prob,
        int DurationTick,
        int DelayTick,
        long InitialCooltime,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Entries);

	public record SetValueNode(
        string Name,
        Entry[] Entries,
        string Key,
        int Value,
        long InitialCooltime,
        long Cooltime,
        bool IsModify,
        bool IsKeepBattle) : Node(Name, Entries);

	public record ConditionsNode(
        string Name,
        Entry[] Entries,
        Condition[] Conditions,
        long InitialCooltime,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Entries);

	public record JumpNode(
        string Name,
        Entry[] Entries,
        Vector3 Pos,
        int Speed,
        float HeightMultiplier,
        NodeJumpType Type,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Entries);

	public record SelectNode(
        string Name,
        Entry[] Entries,
        int[] Prob,
        bool useNpcProb) : Node(Name, Entries);

	public record MoveNode(
        string Name,
        Entry[] Entries,
        Vector3 Destination,
        int Prob,
        string Animation, // kfm anim name
        int Limit,
        int Speed,
        int FaceTarget,
        long InitialCooltime,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Entries);

	public record SummonNode(
        string Name,
        Entry[] Entries,
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
        NodeSummonMaster Master,
        NodeSummonOption[] Option,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Entries);

	public record TriggerSetUserValueNode(
        string Name,
        Entry[] Entries,
        int TriggerID,
        string Key,
        int Value,
        long Cooltime,
        bool IsKeepBattle) : Node(Name, Entries);

	public record RideNode(
        string Name,
        Entry[] Entries,
        NodeRideType Type,
        bool IsRideOff,
        int[] RideNpcIDs) : Node(Name, Entries);

	public record SetSlaveValueNode(
        string Name,
        Entry[] Entries,
        string Key,
        int Value,
        bool IsRandom,
        long Cooltime,
        bool IsModify,
        bool IsKeepBattle) : Node(Name, Entries);

	public record SetMasterValueNode(
        string Name,
        Entry[] Entries,
        string Key,
        int Value,
        bool IsRandom,
        long Cooltime,
        bool IsModify,
        bool IsKeepBattle) : Node(Name, Entries);

	public record RunawayNode(
        string Name,
        Entry[] Entries,
        string Animation, // kfm anim name
        int SkillIdx,
        int Till,
        int Limit,
        Vector3 FacePos,
        long InitialCooltime,
        long Cooltime) : Node(Name, Entries);

	public record MinimumHpNode(
        string Name,
        Entry[] Entries,
        float HpPercent) : Node(Name, Entries);

	public record BuffNode(
        string Name,
        Entry[] Entries,
        int Id,
        NodeBuffType Type,
        short Level,
        int Prob,
        long InitialCooltime,
        long Cooltime,
        bool IsTarget,
        bool IsKeepBattle) : Node(Name, Entries);

	public record TargetEffectNode(
        string Name,
        Entry[] Entries,
        string EffectName // xml effect
        ) : Node(Name, Entries);

	public record ShowVibrateNode(
        string Name,
        Entry[] Entries,
        int GroupId) : Node(Name, Entries);

	public record SidePopupNode(
        string Name,
        Entry[] Entries,
        NodePopupType Type,
        string Illust, // side popup asset name
        int Duration,
        string Script,
        string Sound, // sound asset name
        string Voice // voice asset path
        ) : Node(Name, Entries);

	public record SetValueRangeTargetNode(
        string Name,
        Entry[] Entries,
        string Key,
        int Value,
        int Height,
        int Radius,
        long Cooltime,
        bool IsModify,
        bool IsKeepBattle) : Node(Name, Entries);

	public record AnnounceNode(
        string Name,
        Entry[] Entries,
        string Message,
        int DurationTick,
        long Cooltime) : Node(Name, Entries);

	public record ModifyRoomTimeNode(
        string Name,
        Entry[] Entries,
        int TimeTick,
        bool IsShowEffect) : Node(Name, Entries);

    public record HideVibrateAllNode(
        string Name,
        Entry[] Entries,
        bool IsKeepBattle) : Node(Name, Entries);

    public record TriggerModifyUserValueNode(
        string Name,
        Entry[] Entries,
        int TriggerID,
        string Key,
        int Value) : Node(Name, Entries);

	public record RemoveSlavesNode(
        string Name,
        Entry[] Entries,
        bool IsKeepBattle) : Node(Name, Entries);

    public record CreateRandomRoomNode(
        string Name,
        Entry[] Entries,
        int RandomRoomId,
        int PortalDuration) : Node(Name, Entries);

    public record CreateInteractObjectNode(
        string Name,
        Entry[] Entries,
        string Normal, // kfm anim name
        int InteractID,
        int LifeTime,
        string KfmName,
        string Reactable // kfm anim name
        ) : Node(Name, Entries);

	public record RemoveMeNode(
        string Name,
        Entry[] Entries) : Node(Name, Entries);

    public record SuicideNode(
        string Name,
        Entry[] Entries) : Node(Name, Entries);
    #endregion

    #region Conditions
    public record DistanceOverCondition(
        string Name,
        Entry[] Entries,
        int Value) : Condition(Name, Entries);

	public record CombatTimeCondition(
        string Name,
        Entry[] Entries,
        long BattleTimeBegin,
        long BattleTimeLoop,
        long BattleTimeEnd) : Condition(Name, Entries);

	public record DistanceLessCondition(
        string Name,
        Entry[] Entries,
        int Value) : Condition(Name, Entries);

	public record SkillRangeCondition(
        string Name,
        Entry[] Entries,
        int SkillIdx,
        short SkillLev,
        bool IsKeepBattle) : Condition(Name, Entries);

	public record ExtraDataCondition(
        string Name,
        Entry[] Entries,
        string Key,
        int Value,
        AiConditionOp Op,
        bool IsKeepBattle) : Condition(Name, Entries);

	public record SlaveCountCondition(
        string Name,
        Entry[] Entries,
        int Count,
        bool UseSummonGroup,
        int SummonGroup) : Condition(Name, Entries);

    public record SlaveCountOpCondition(
        string Name,
        Entry[] Entries,
        int SlaveCount,
        AiConditionOp SlaveCountOp) : Condition(Name, Entries);

    public record HpOverCondition(
        string Name,
        Entry[] Entries,
        int Value) : Condition(Name, Entries);

	public record StateCondition(
        string Name,
        Entry[] Entries,
        AiConditionTargetState TargetState) : Condition(Name, Entries);

	public record AdditionalCondition(
        string Name,
        Entry[] Entries,
        int Id,
        short Level,
        int OverlapCount,
        bool IsTarget) : Condition(Name, Entries);

	public record HpLessCondition(
        string Name,
        Entry[] Entries,
        int Value) : Condition(Name, Entries);

	public record TrueCondition(
        string Name,
        Entry[] Entries) : Condition(Name, Entries);
    #endregion
}
