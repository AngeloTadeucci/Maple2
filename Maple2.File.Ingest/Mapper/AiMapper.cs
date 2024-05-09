using M2dXmlGenerator;
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
        parser = new AiParser(xmlReader);
    }

    protected override IEnumerable<AiMetadata> Map() {
        foreach ((string name, NpcAi data) in parser.Parse()) {
            List<AiMetadata.Condition> reserved = new List<AiMetadata.Condition>();
            List<AiMetadata.Entry> battle = new List<AiMetadata.Entry>();
            List<AiMetadata.Entry> battleEnd = new List<AiMetadata.Entry>();
            List<AiMetadata.AiPresetDefinition> aiPresets = new List<AiMetadata.AiPresetDefinition>();

            foreach (Entry entry in data.Reserved) {
                if (entry is not ConditionEntry node) {
                    continue;
                }

                if (node is FeatureCondition feature && !FeatureLocaleFilter.FeatureEnabled(feature.feature)) {
                    continue;
                }

                reserved.Add(MapCondition(node));
            }
            
            foreach (Entry entry in data.Battle) {
                MapEntry(battle, entry);
            }

            foreach (Entry entry in data.BattleEnd) {
                MapEntry(battle, entry);
            }

            foreach (Entry node in data.AiPresets) {
                var childNodes = new List<AiMetadata.Entry>();

                foreach (Entry entry in node.Entries) {
                    MapEntry(childNodes, entry);
                }

                aiPresets.Add(new AiMetadata.AiPresetDefinition(
                    Name: node.name,
                    Entries:
                    childNodes.ToArray()
                ));
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

    AiMetadata.Condition MapCondition(ConditionEntry node) {
        var childNodes = new List<AiMetadata.Entry>();

        foreach (Entry entry in node.Entries) {
            MapEntry(childNodes, entry);
        }

        switch(node) {
            case DistanceOverCondition distanceOver:
                return new AiMetadata.DistanceOverCondition(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Value: distanceOver.value
                );
            case CombatTimeCondition combatTime:
                return new AiMetadata.CombatTimeCondition(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    BattleTimeBegin: combatTime.battleTimeBegin,
                    BattleTimeLoop: combatTime.battleTimeLoop,
                    BattleTimeEnd: combatTime.battleTimeEnd
                );
            case DistanceLessCondition distanceLess:
                return new AiMetadata.DistanceLessCondition(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Value: distanceLess.value
                );
            case SkillRangeCondition skillRange:
                return new AiMetadata.SkillRangeCondition(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    SkillIdx: skillRange.skillIdx,
                    SkillLev: skillRange.skillLev,
                    IsKeepBattle: skillRange.isKeepBattle
                );
            case ExtraDataCondition extraData:
                return new AiMetadata.ExtraDataCondition(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Key: extraData.key,
                    Value: extraData.value,
                    Op: (AiConditionOp) extraData.op,
                    IsKeepBattle: extraData.isKeepBattle
                );
            case SlaveCountCondition SlaveCount: // these are different enough to warrant having their own nodes. blame nexon
                return new AiMetadata.SlaveCountCondition(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Count: SlaveCount.count,
                    UseSummonGroup: SlaveCount.useSummonGroup,
                    SummonGroup: SlaveCount.summonGroup
                );
            case HpOverCondition hpOver:
                return new AiMetadata.HpOverCondition(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Value: hpOver.value
                );
            case StateCondition state:
                return new AiMetadata.StateCondition(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    TargetState: (AiConditionTargetState) state.targetState
                );
            case AdditionalCondition additional:
                return new AiMetadata.AdditionalCondition(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Id: additional.id,
                    Level: additional.level,
                    OverlapCount: additional.overlapCount,
                    IsTarget: additional.isTarget
                );
            case HpLessCondition hpLess:
                return new AiMetadata.HpLessCondition(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Value: hpLess.value
                );
            case FeatureCondition feature: // feature was converted to TrueCondition
            case TrueCondition trueNode:
                if (node.name == "feature") {
                    Console.WriteLine("AI feature condition node is being convered to a true node");
                }
                return new AiMetadata.TrueCondition(
                    Name: node.name,
                    Entries: childNodes.ToArray()
                );
            default:
                throw new NotImplementedException("unknown AI condition name: " + node.name);
        }
    }

    void MapEntry(List<AiMetadata.Entry> entries, Entry entry) {
        if (entry is NodeEntry node) {
            entries.Add(MapNode(node));

            return;
        }

        if (entry is AiPresetEntry aiPreset) {
            entries.Add(new AiMetadata.AiPreset(
                Name: aiPreset.name
            ));

            return;
        }

        throw new NotImplementedException($"unknown entry type {entry.GetType().Name}");
    }

    AiMetadata.Node MapNode(NodeEntry node) {
        var childNodes = new List<AiMetadata.Entry>();
        var childConditions = new List<AiMetadata.Condition>();

        foreach (Entry entry in node.Entries) {
            if (entry is ConditionEntry) {
                continue;
            }

            MapEntry(childNodes, entry);
        }

        foreach (Entry entry in node.Entries) {
            if (entry is not ConditionEntry child) {
                continue;
            }

            if (child is FeatureCondition feature && !FeatureLocaleFilter.FeatureEnabled(feature.feature)) {
                continue;
            }

            childConditions.Add(MapCondition(child));
        }

        switch(node) {
            case TraceNode trace:
                return new AiMetadata.TraceNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Limit: trace.limit,
                    SkillIdx: trace.skillIdx,
                    Animation: trace.animation,
                    Speed: trace.speed,
                    Till: trace.till,
                    InitialCooltime: trace.initialCooltime,
                    Cooltime: trace.cooltime,
                    IsKeepBattle: trace.isKeepBattle
                );
            case SkillNode skill:
                return new AiMetadata.SkillNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Idx: skill.idx,
                    Level: skill.level,
                    Prob: skill.prob,
                    Sequence: skill.sequence,
                    FacePos: skill.facePos,
                    FaceTarget: skill.faceTarget,
                    FaceTargetTick: skill.faceTargetTick,
                    InitialCooltime: skill.initialCooltime,
                    Cooltime: skill.cooltime,
                    Limit: skill.limit,
                    IsKeepBattle: skill.isKeepBattle
                );
            case TeleportNode teleport:
                return new AiMetadata.TeleportNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Pos: teleport.pos,
                    Prob: teleport.prob,
                    FacePos: teleport.facePos,
                    FaceTarget: teleport.faceTarget,
                    InitialCooltime: teleport.initialCooltime,
                    Cooltime: teleport.cooltime,
                    IsKeepBattle: teleport.isKeepBattle
                );
            case StandbyNode standby:
                return new AiMetadata.StandbyNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Limit: standby.limit,
                    Prob: standby.prob,
                    Animation: standby.animation,
                    FacePos: standby.facePos,
                    FaceTarget: standby.faceTarget,
                    InitialCooltime: standby.initialCooltime,
                    Cooltime: standby.cooltime,
                    IsKeepBattle: standby.isKeepBattle
                );
            case SetDataNode setData:
                return new AiMetadata.SetDataNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Key: setData.key,
                    Value: setData.value,
                    Cooltime: setData.cooltime
                );
            case TargetNode target:
                return new AiMetadata.TargetNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Type: (NodeTargetType) target.type,
                    Prob: target.prob,
                    Rank: target.rank,
                    AdditionalId: target.additionalId,
                    AdditionalLevel: target.additionalLevel,
                    From: target.from,
                    To: target.to,
                    Center: target.center,
                    Target: (NodeAiTarget) target.target,
                    NoChangeWhenNoTarget: target.noChangeWhenNoTarget,
                    InitialCooltime: target.initialCooltime,
                    Cooltime: target.cooltime,
                    IsKeepBattle: target.isKeepBattle
                );
            case SayNode say:
                return new AiMetadata.SayNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Message: say.message,
                    Prob: say.prob,
                    DurationTick: say.durationTick,
                    DelayTick: say.delayTick,
                    InitialCooltime: say.initialCooltime,
                    Cooltime: say.cooltime,
                    IsKeepBattle: say.isKeepBattle
                );
            case SetValueNode setValue:
                return new AiMetadata.SetValueNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Key: setValue.key,
                    Value: setValue.value,
                    InitialCooltime: setValue.initialCooltime,
                    Cooltime: setValue.cooltime,
                    IsModify: setValue.isModify,
                    IsKeepBattle: setValue.isKeepBattle
                );
            case ConditionsNode conditions:
                return new AiMetadata.ConditionsNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Conditions: childConditions.ToArray(),
                    InitialCooltime: conditions.initialCooltime,
                    Cooltime: conditions.cooltime,
                    IsKeepBattle: conditions.isKeepBattle
                );
            case JumpNode jump:
                return new AiMetadata.JumpNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Pos: jump.pos,
                    Speed: jump.speed,
                    HeightMultiplier: jump.heightMultiplier,
                    Type: (NodeJumpType) jump.type,
                    Cooltime: jump.cooltime,
                    IsKeepBattle: jump.isKeepBattle
                );
            case SelectNode select:
                return new AiMetadata.SelectNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Prob: select.prob,
                    useNpcProb: select.useNpcProb
                );
            case MoveNode move:
                return new AiMetadata.MoveNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Destination: move.destination,
                    Prob: move.prob,
                    Animation: move.animation,
                    Limit: move.limit,
                    Speed: move.speed,
                    FaceTarget: move.faceTarget,
                    InitialCooltime: move.initialCooltime,
                    Cooltime: move.cooltime,
                    IsKeepBattle: move.isKeepBattle
                );
            case SummonNode summon:
                return new AiMetadata.SummonNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    NpcId: summon.npcId,
                    NpcCountMax: summon.npcCountMax,
                    NpcCount: summon.npcCount,
                    DelayTick: summon.delayTick,
                    LifeTime: summon.lifeTime,
                    SummonRot: summon.summonRot,
                    SummonPos: summon.summonPos,
                    SummonPosOffset: summon.summonPosOffset,
                    SummonTargetOffset: summon.summonTargetOffset,
                    SummonRadius: summon.summonRadius,
                    Group: summon.group,
                    Master: (NodeSummonMaster) summon.master,
                    Option: Array.ConvertAll(summon.option, value => (NodeSummonOption)value),
                    Cooltime: summon.cooltime,
                    IsKeepBattle: summon.isKeepBattle
                );
            case TriggerSetUserValueNode triggerSetUserValue:
                return new AiMetadata.TriggerSetUserValueNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    TriggerID: triggerSetUserValue.triggerID,
                    Key: triggerSetUserValue.key,
                    Value: triggerSetUserValue.value,
                    Cooltime: triggerSetUserValue.cooltime,
                    IsKeepBattle: triggerSetUserValue.isKeepBattle
                );
            case RideNode ride:
                return new AiMetadata.RideNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Type: (NodeRideType) ride.type,
                    IsRideOff: ride.isRideOff,
                    RideNpcIDs: ride.rideNpcIDs
                );
            case SetSlaveValueNode setSlaveValue:
                return new AiMetadata.SetSlaveValueNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Key: setSlaveValue.key,
                Value: setSlaveValue.value,
                    IsRandom: setSlaveValue.isRandom,
                    Cooltime: setSlaveValue.cooltime,
                    IsModify: setSlaveValue.isModify,
                    IsKeepBattle: setSlaveValue.isKeepBattle
                );
            case SetMasterValueNode setMasterValue:
                return new AiMetadata.SetMasterValueNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Key: setMasterValue.key,
                    Value: setMasterValue.value,
                    IsRandom: setMasterValue.isRandom,
                    Cooltime: setMasterValue.cooltime,
                    IsModify: setMasterValue.isModify,
                    IsKeepBattle: setMasterValue.isKeepBattle
                );
            case RunawayNode runaway:
                return new AiMetadata.RunawayNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Animation: runaway.animation,
                    SkillIdx: runaway.skillIdx,
                    Till: runaway.till,
                    Limit: runaway.limit,
                    FacePos: runaway.facePos,
                    InitialCooltime: runaway.initialCooltime,
                    Cooltime: runaway.cooltime
                );
            case MinimumHpNode minimumHp:
                return new AiMetadata.MinimumHpNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    HpPercent: minimumHp.hpPercent
                );
            case BuffNode buff:
                return new AiMetadata.BuffNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Id: buff.id,
                    Type: (NodeBuffType) buff.type,
                    Level: buff.level,
                    Prob: buff.prob,
                    InitialCooltime: buff.initialCooltime,
                    Cooltime: buff.cooltime,
                    IsTarget: buff.isTarget,
                    IsKeepBattle: buff.isKeepBattle
                );
            case TargetEffectNode targetEffect:
                return new AiMetadata.TargetEffectNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    EffectName: targetEffect.effectName
                );
            case ShowVibrateNode showVibrate:
                return new AiMetadata.ShowVibrateNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    GroupId: showVibrate.groupID
                );
            case SidePopupNode sidePopup:
                return new AiMetadata.SidePopupNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Type: (NodePopupType) sidePopup.type,
                    Illust: sidePopup.illust,
                    Duration: sidePopup.duration,
                    Script: sidePopup.script,
                    Sound: sidePopup.sound,
                    Voice: sidePopup.voice
                );
            case SetValueRangeTargetNode setValueRangeTarget:
                return new AiMetadata.SetValueRangeTargetNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Key: setValueRangeTarget.key,
                    Value: setValueRangeTarget.value,
                    Height: setValueRangeTarget.height,
                    Radius: setValueRangeTarget.radius,
                    Cooltime: setValueRangeTarget.cooltime,
                    IsModify: setValueRangeTarget.isModify,
                    IsKeepBattle: setValueRangeTarget.isKeepBattle
                );
            case AnnounceNode announce:
                return new AiMetadata.AnnounceNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Message: announce.message,
                    DurationTick: announce.durationTick,
                    Cooltime: announce.cooltime
                );
            case ModifyRoomTimeNode modifyRoomTime:
                return new AiMetadata.ModifyRoomTimeNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    TimeTick: modifyRoomTime.timeTick,
                    IsShowEffect: modifyRoomTime.isShowEffect
                );
            case HideVibrateAllNode hideVibrateAll:
                return new AiMetadata.HideVibrateAllNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    IsKeepBattle: hideVibrateAll.isKeepBattle
                );
            case TriggerModifyUserValueNode triggerModifyUserValue:
                return new AiMetadata.TriggerModifyUserValueNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    TriggerID: triggerModifyUserValue.triggerID,
                    Key: triggerModifyUserValue.key,
                    Value: triggerModifyUserValue.value
                );
            case RemoveSlavesNode removeSlaves:
                return new AiMetadata.RemoveSlavesNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    IsKeepBattle: removeSlaves.isKeepBattle
                );
            case CreateRandomRoomNode createRandomRoom:
                return new AiMetadata.CreateRandomRoomNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    RandomRoomId: createRandomRoom.randomRoomID,
                    PortalDuration: createRandomRoom.portalDuration
                );
            case CreateInteractObjectNode createInteractObject:
                return new AiMetadata.CreateInteractObjectNode(
                    Name: node.name,
                    Entries: childNodes.ToArray(),
                    Normal: createInteractObject.normal,
                    InteractID: createInteractObject.interactID,
                    LifeTime: createInteractObject.lifeTime,
                    KfmName: createInteractObject.kfmName,
                    Reactable: createInteractObject.reactable
                );
            case RemoveNode removeMe:
                return new AiMetadata.RemoveMeNode(
                    Name: node.name,
                    Entries: childNodes.ToArray()
                );
            case SuicideNode Suicide:
                return new AiMetadata.SuicideNode(
                    Name: node.name,
                    Entries: childNodes.ToArray()
                );
            default:
                throw new NotImplementedException("unknown AI node name: " + node.name);
        }
    }
}
