using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml.AI;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using System.Numerics;

namespace Maple2.File.Ingest.Mapper;

public class AiMapper : TypeMapper<AiMetadata> {
    private readonly AiParser parser;

    public AiMapper(M2dReader xmlReader) {
        parser = new(xmlReader);
    }

    protected override IEnumerable<AiMetadata> Map() {
        foreach ((string name, NpcAi data) in parser.Parse()) {
            List<AiMetadata.Condition> reserved = [];
            List<AiMetadata.Node> battle = [];
            List<AiMetadata.Node> battleEnd = [];
            List<AiMetadata.AiPresetDefinition> aiPresets = [];

            foreach (Condition node in data.reserved?.condition ?? new List<Condition>()) {
                reserved.Add(MapCondition(node));
            }

            foreach (Node node in data.battle.node) {
                battle.Add(MapNode(node));
            }

            foreach (Node node in data.battleEnd?.node ?? []) {
                battleEnd.Add(MapNode(node));
            }

            foreach (AiPreset node in data.aiPresets?.aiPreset ?? []) {
                var childNodes = new List<AiMetadata.Node>();
                var childAiPresets = new List<AiMetadata.AiPreset>();

                foreach (Node child in node.node) {
                    childNodes.Add(MapNode(child));
                }

                foreach (AiPreset child in node.aiPreset) {
                    childAiPresets.Add(new AiMetadata.AiPreset(child.name));
                }

                aiPresets.Add(new AiMetadata.AiPresetDefinition(Name: node.name, Nodes: childNodes.ToArray(), AiPresets: childAiPresets.ToArray()));
            }

            yield return new AiMetadata(
                Name: name,
                Reserved: reserved.ToArray(),
                Battle: battle.ToArray(),
                BattleEnd: battleEnd.ToArray(),
                AiPresets: aiPresets.ToArray()
            );
        }
    }

    AiMetadata.Condition MapCondition(Condition node) {
        var childNodes = new List<AiMetadata.Node>();
        var childAiPresets = new List<AiMetadata.AiPreset>();

        foreach (Node child in node.node) {
            childNodes.Add(MapNode(child));
        }

        foreach (AiPreset child in node.aiPreset) {
            childAiPresets.Add(new AiMetadata.AiPreset(child.name));
        }

        switch(node.name) {
            case "distanceOver":
                return new AiMetadata.DistanceOverCondition(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Value: node.value
                );
            case "combatTime":
                return new AiMetadata.CombatTimeCondition(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    BattleTimeBegin: node.battleTimeBegin,
                    BattleTimeLoop: node.battleTimeLoop,
                    BattleTimeEnd: node.battleTimeEnd
                );
            case "distanceLess":
                return new AiMetadata.DistanceLessCondition(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Value: node.value
                );
            case "skillRange":
                return new AiMetadata.SkillRangeCondition(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    SkillIdx: node.skillIdx,
                    SkillLev: node.skillLev,
                    IsKeepBattle: node.isKeepBattle
                );
            case "extraData":
                return new AiMetadata.ExtraDataCondition(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Key: node.key,
                    Value: node.value,
                    Op: (AiConditionOp)node.op,
                    IsKeepBattle: node.isKeepBattle
                );
            case "SlaveCount": // these are different enough to warrant having their own nodes. blame nexon
                return new AiMetadata.SlaveCountCondition(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Count: node.count,
                    UseSummonGroup: node.useSummonGroup,
                    SummonGroup: node.summonGroup
                );
            case "hpOver":
                return new AiMetadata.HpOverCondition(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Value: node.value
                );
            case "state":
                return new AiMetadata.StateCondition(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    TargetState: (AiConditionTargetState)node.targetState
                );
            case "additional":
                return new AiMetadata.AdditionalCondition(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Id: node.id,
                    Level: node.level,
                    OverlapCount: node.overlapCount,
                    IsTarget: node.isTarget
                );
            case "hpLess":
                return new AiMetadata.HpLessCondition(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Value: node.value
                );
            case "DistanceLess":
                return new AiMetadata.DistanceLessCondition(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Value: node.value
                );
            case "slaveCount": // these are different enough to warrant having their own nodes. blame nexon
                return new AiMetadata.SlaveCountOpCondition(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    SlaveCount: node.slaveCount,
                    SlaveCountOp: (AiConditionOp) node.slaveCountOp
                );
            case "feature": // feature was converted to TrueCondition
            case "true":
                if (node.name == "feature") {
                    Console.WriteLine("AI feature condition node is being convered to a true node");
                }
                return new AiMetadata.TrueCondition(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray()
                );
            default:
                throw new NotImplementedException("unknown AI condition name: " + node.name);
        }
    }

    AiMetadata.Node MapNode(Node node) {
        List<AiMetadata.Node> childNodes = [];
        List<AiMetadata.Condition> childConditions = [];
        List<AiMetadata.AiPreset> childAiPresets = [];

        foreach (Node child in node.node) {
            childNodes.Add(MapNode(child));
        }

        foreach (Condition child in node.condition) {
            childConditions.Add(MapCondition(child));
        }

        foreach (AiPreset child in node.aiPreset) {
            childAiPresets.Add(new AiMetadata.AiPreset(child.name));
        }

        int onlyProb = node.prob.Length > 0 ? node.prob[0] : 100;

        switch(node.name) {
            case "trace":
                return new AiMetadata.TraceNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Limit: node.limit,
                    SkillIdx: node.skillIdx,
                    Animation: node.animation,
                    Speed: node.speed,
                    Till: node.till,
                    InitialCooltime: node.initialCooltime,
                    Cooltime: node.cooltime,
                    IsKeepBattle: node.isKeepBattle
                );
            case "skill":
                return new AiMetadata.SkillNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Idx: node.idx,
                    Level: node.level,
                    Prob: onlyProb,
                    Sequence: node.sequence,
                    FacePos: node.facePos,
                    FaceTarget: node.faceTarget,
                    FaceTargetTick: node.faceTargetTick,
                    InitialCooltime: node.initialCooltime,
                    Cooltime: node.cooltime,
                    Limit: node.limit,
                    IsKeepBattle: node.isKeepBattle
                );
            case "teleport":
                return new AiMetadata.TeleportNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Pos: node.pos,
                    Prob: onlyProb,
                    FacePos: node.facePos,
                    FaceTarget: node.faceTarget,
                    InitialCooltime: node.initialCooltime,
                    Cooltime: node.cooltime,
                    IsKeepBattle: node.isKeepBattle
                );
            case "standby":
                return new AiMetadata.StandbyNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Limit: node.limit,
                    Prob: onlyProb,
                    Animation: node.animation,
                    FacePos: node.facePos,
                    FaceTarget: node.faceTarget,
                    InitialCooltime: node.initialCooltime,
                    Cooltime: node.cooltime,
                    IsKeepBattle: node.isKeepBattle
                );
            case "setData":
                return new AiMetadata.SetDataNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Key: node.key,
                    Value: node.value,
                    Cooltime: node.cooltime
                );
            case "target":
                NodeTargetType targetType = NodeTargetType.Random;
                Enum.TryParse(node.type, out targetType);
                return new AiMetadata.TargetNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Type: targetType,
                    Prob: onlyProb,
                    Rank: node.rank,
                    AdditionalId: node.additionalId,
                    AdditionalLevel: node.additionalLevel,
                    From: node.from,
                    To: node.to,
                    Center: node.center,
                    Target: (NodeAiTarget)node.target,
                    NoChangeWhenNoTarget: node.noChangeWhenNoTarget,
                    InitialCooltime: node.initialCooltime,
                    Cooltime: node.cooltime,
                    IsKeepBattle: node.isKeepBattle
                );
            case "say":
                return new AiMetadata.SayNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Message: node.message,
                    Prob: onlyProb,
                    DurationTick: node.durationTick,
                    DelayTick: node.delayTick,
                    InitialCooltime: node.initialCooltime,
                    Cooltime: node.cooltime,
                    IsKeepBattle: node.isKeepBattle
                );
            case "SetValue":
                return new AiMetadata.SetValueNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Key: node.key,
                    Value: node.value,
                    InitialCooltime: node.initialCooltime,
                    Cooltime: node.cooltime,
                    IsModify: node.isModify,
                    IsKeepBattle: node.isKeepBattle
                );
            case "conditions":
                return new AiMetadata.ConditionsNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Conditions: childConditions.ToArray(),
                    InitialCooltime: node.initialCooltime,
                    Cooltime: node.cooltime,
                    IsKeepBattle: node.isKeepBattle
                );
            case "jump":
                NodeJumpType jumpType = NodeJumpType.JumpA;
                Enum.TryParse(node.type, out jumpType);
                return new AiMetadata.JumpNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Pos: node.pos,
                    Speed: node.speed,
                    HeightMultiplier: node.heightMultiplier,
                    Type: jumpType,
                    Cooltime: node.cooltime,
                    IsKeepBattle: node.isKeepBattle
                );
            case "select":
                return new AiMetadata.SelectNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Prob: node.prob,
                    useNpcProb: node.useNpcProb
                );
            case "move":
                return new AiMetadata.MoveNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Destination: node.destination,
                    Prob: onlyProb,
                    Animation: node.animation,
                    Limit: node.limit,
                    Speed: node.speed,
                    FaceTarget: node.faceTarget,
                    InitialCooltime: node.initialCooltime,
                    Cooltime: node.cooltime,
                    IsKeepBattle: node.isKeepBattle
                );
            case "summon":
                return new AiMetadata.SummonNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    NpcId: node.npcId,
                    NpcCountMax: node.npcCountMax,
                    NpcCount: node.npcCount,
                    DelayTick: node.delayTick,
                    LifeTime: node.lifeTime,
                    SummonRot: node.summonRot,
                    SummonPos: node.summonPos,
                    SummonPosOffset: node.summonPosOffset,
                    SummonTargetOffset: node.summonTargetOffset,
                    SummonRadius: node.summonRadius,
                    Group: node.group,
                    Master: (NodeSummonMaster)node.master,
                    Option: Array.ConvertAll(node.option, value => (NodeSummonOption)value),
                    Cooltime: node.cooltime,
                    IsKeepBattle: node.isKeepBattle
                );
            case "TriggerSetUserValue":
                return new AiMetadata.TriggerSetUserValueNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    TriggerID: node.triggerID,
                    Key: node.key,
                    Value: node.value,
                    Cooltime: node.cooltime,
                    IsKeepBattle: node.isKeepBattle
                );
            case "ride":
                NodeRideType rideType = NodeRideType.Slave;
                Enum.TryParse(node.type, out rideType);
                return new AiMetadata.RideNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Type: rideType,
                    IsRideOff: node.isRideOff,
                    RideNpcIDs: node.rideNpcIDs
                );
            case "SetSlaveValue":
                return new AiMetadata.SetSlaveValueNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Key: node.key,
                    Value: node.value,
                    IsRandom: node.isRandom,
                    Cooltime: node.cooltime,
                    IsModify: node.isModify,
                    IsKeepBattle: node.isKeepBattle
                );
            case "SetMasterValue":
                return new AiMetadata.SetMasterValueNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Key: node.key,
                    Value: node.value,
                    IsRandom: node.isRandom,
                    Cooltime: node.cooltime,
                    IsModify: node.isModify,
                    IsKeepBattle: node.isKeepBattle
                );
            case "runaway":
                return new AiMetadata.RunawayNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Animation: node.animation,
                    SkillIdx: node.skillIdx,
                    Till: node.till,
                    Limit: node.limit,
                    FacePos: node.facePos,
                    InitialCooltime: node.initialCooltime,
                    Cooltime: node.cooltime
                );
            case "MinimumHp":
                return new AiMetadata.MinimumHpNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    HpPercent: node.hpPercent
                );
            case "buff":
                NodeBuffType buffType = NodeBuffType.Add;
                Enum.TryParse(node.type, out buffType);
                return new AiMetadata.BuffNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Id: node.id,
                    Type: buffType,
                    Level: node.level,
                    Prob: onlyProb,
                    InitialCooltime: node.initialCooltime,
                    Cooltime: node.cooltime,
                    IsTarget: node.isTarget,
                    IsKeepBattle: node.isKeepBattle
                );
            case "TargetEffect":
                return new AiMetadata.TargetEffectNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    EffectName: node.effectName
                );
            case "ShowVibrate":
                return new AiMetadata.ShowVibrateNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    GroupId: node.groupID
                );
            case "sidePopup":
                NodePopupType popupType = NodePopupType.Talk;
                Enum.TryParse(node.type, out popupType);
                return new AiMetadata.SidePopupNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Type: popupType,
                    Illust: node.illust,
                    Duration: node.duration,
                    Script: node.script,
                    Sound: node.sound,
                    Voice: node.voice
                );
            case "SetValueRangeTarget":
                return new AiMetadata.SetValueRangeTargetNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Key: node.key,
                    Value: node.value,
                    Height: node.height,
                    Radius: node.radius,
                    Cooltime: node.cooltime,
                    IsModify: node.isModify,
                    IsKeepBattle: node.isKeepBattle
                );
            case "announce":
                return new AiMetadata.AnnounceNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Message: node.message,
                    DurationTick: node.durationTick,
                    Cooltime: node.cooltime
                );
            case "ModifyRoomTime":
                return new AiMetadata.ModifyRoomTimeNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    TimeTick: node.timeTick,
                    IsShowEffect:node.isShowEffect
                );
            case "HideVibrateAll":
                return new AiMetadata.HideVibrateAllNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    IsKeepBattle: node.isKeepBattle
                );
            case "TriggerModifyUserValue":
                return new AiMetadata.TriggerModifyUserValueNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    TriggerID: node.triggerID,
                    Key: node.key,
                    Value: node.value
                );
            case "Buff":
                return new AiMetadata.BuffNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Id: node.id,
                    Type: NodeBuffType.Add,
                    Level: node.level,
                    Prob: 100,
                    InitialCooltime: 0,
                    Cooltime: 0,
                    IsTarget: false,
                    IsKeepBattle: true
                );
            case "RemoveSlaves":
                return new AiMetadata.RemoveSlavesNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    IsKeepBattle: node.isKeepBattle
                );
            case "CreateRandomRoom":
                return new AiMetadata.CreateRandomRoomNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    RandomRoomId: node.randomRoomID,
                    PortalDuration: node.portalDuration
                );
            case "CreateInteractObject":
                return new AiMetadata.CreateInteractObjectNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray(),
                    Normal: node.normal,
                    InteractID: node.interactID,
                    LifeTime: node.lifeTime,
                    KfmName: node.kfmName,
                    Reactable: node.reactable
                );
            case "RemoveMe":
                return new AiMetadata.RemoveMeNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray());
            case "Suicide":
                return new AiMetadata.SuicideNode(
                    Name: node.name,
                    Nodes: childNodes.ToArray(),
                    AiPresets: childAiPresets.ToArray()
                );
            default:
                throw new NotImplementedException("unknown AI node name: " + node.name);
        }
    }
}
