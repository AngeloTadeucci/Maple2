using Maple2.File.IO;
using Maple2.File.Parser;
using Maple2.File.Parser.Xml.AI;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class AiMapper : TypeMapper<AiMetadata> {
    private readonly AiParser parser;

    public AiMapper(M2dReader xmlReader) {
        parser = new AiParser(xmlReader);
    }

    protected override IEnumerable<AiMetadata> Map() {
        foreach ((string name, NpcAi data) in parser.Parse()) {
            List<AiMetadata.Condition> reserved = new List<AiMetadata.Condition>();
            List<AiMetadata.Node> battle = new List<AiMetadata.Node>();
            List<AiMetadata.Node> battleEnd = new List<AiMetadata.Node>();
            List<AiMetadata.AiPresetDefinition> aiPresets = new List<AiMetadata.AiPresetDefinition>();

            foreach (Condition node in data.reserved?.condition ?? new List<Condition>()) {
                reserved.Add(MapCondition(node));
            }

            foreach (Node node in data.battle.node) {
                battle.Add(MapNode(node));
            }

            foreach (Node node in data.battleEnd?.node ?? new List<Node>()) {
                battleEnd.Add(MapNode(node));
            }

            foreach (AiPreset node in data.aiPresets?.aiPreset ?? new List<AiPreset>()) {
                List<AiMetadata.Node> childNodes = new List<AiMetadata.Node>();
                List<AiMetadata.AiPreset> childAiPresets = new List<AiMetadata.AiPreset>();

                foreach (Node child in node.node) {
                    childNodes.Add(MapNode(child));
                }

                foreach (AiPreset child in node.aiPreset) {
                    childAiPresets.Add(new AiMetadata.AiPreset(child.name));
                }

                aiPresets.Add(new AiMetadata.AiPresetDefinition(node.name, childNodes.ToArray(), childAiPresets.ToArray()));
            }

            yield return new AiMetadata(name, reserved.ToArray(), battle.ToArray(), battleEnd.ToArray(), aiPresets.ToArray());
        }
    }

    AiMetadata.Condition MapCondition(Condition node) {
        List<AiMetadata.Node> childNodes = new List<AiMetadata.Node>();
        List<AiMetadata.AiPreset> childAiPresets = new List<AiMetadata.AiPreset>();
        
        foreach (Node child in node.node) {
            childNodes.Add(MapNode(child));
        }

        foreach (AiPreset child in node.aiPreset) {
            childAiPresets.Add(new AiMetadata.AiPreset(child.name));
        }

        switch(node.name) {
            case "distanceOver":
                return new AiMetadata.DistanceOverCondition(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.value);
            case "combatTime":
                return new AiMetadata.CombatTimeCondition(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.battleTimeBegin, node.battleTimeLoop, node.battleTimeEnd);
            case "distanceLess":
                return new AiMetadata.DistanceLessCondition(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.value);
            case "skillRange":
                return new AiMetadata.SkillRangeCondition(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.skillIdx, node.skillLev, node.isKeepBattle);
            case "extraData":
                return new AiMetadata.ExtraDataCondition(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.key, node.value, (AiConditionOp)node.op, node.isKeepBattle);
            case "SlaveCount": // these are different enough to warrant having their own nodes. blame nexon
                return new AiMetadata.SlaveCountCondition(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.count, node.useSummonGroup, node.summonGroup);
            case "hpOver":
                return new AiMetadata.HpOverCondition(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.value);
            case "state":
                return new AiMetadata.StateCondition(node.name, childNodes.ToArray(), childAiPresets.ToArray(), (AiConditionTargetState)node.targetState);
            case "additional":
                return new AiMetadata.AdditionalCondition(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.id, node.level, node.overlapCount, node.isTarget);
            case "hpLess":
                return new AiMetadata.HpLessCondition(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.value);
            case "DistanceLess":
                return new AiMetadata.DistanceLessCondition(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.value);
            case "slaveCount": // these are different enough to warrant having their own nodes. blame nexon
                return new AiMetadata.SlaveCountOpCondition(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.slaveCount, (AiConditionOp) node.slaveCountOp);
            case "feature": // feature was converted to TrueCondition
            case "true":
                if (node.name == "feature") {
                    Console.WriteLine("AI feature condition node is being convered to a true node");
                }
                return new AiMetadata.TrueCondition(node.name, childNodes.ToArray(), childAiPresets.ToArray());
            default:
                throw new NotImplementedException("unknown AI condition name: " + node.name);
        }
    }

    AiMetadata.Node MapNode(Node node) {
        List<AiMetadata.Node> childNodes = new List<AiMetadata.Node>();
        List<AiMetadata.Condition> childConditions = new List<AiMetadata.Condition>();
        List<AiMetadata.AiPreset> childAiPresets = new List<AiMetadata.AiPreset>();

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
                return new AiMetadata.TraceNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.limit, node.skillIdx, node.animation, node.speed, node.till, node.initialCooltime, node.cooltime, node.isKeepBattle);
            case "skill":
                return new AiMetadata.SkillNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.idx, node.level, onlyProb, node.sequence, node.facePos, node.faceTarget, node.faceTargetTick, node.initialCooltime, node.cooltime, node.limit, node.isKeepBattle);
            case "teleport":
                return new AiMetadata.TeleportNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.pos, onlyProb, node.facePos, node.faceTarget, node.initialCooltime, node.cooltime, node.isKeepBattle);
            case "standby":
                return new AiMetadata.StandbyNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.limit, onlyProb, node.animation, node.facePos, node.faceTarget, node.initialCooltime, node.cooltime, node.isKeepBattle);
            case "setData":
                return new AiMetadata.SetDataNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.key, node.value, node.cooltime);
            case "target":
                NodeTargetType targetType = NodeTargetType.Random;
                Enum.TryParse(node.type, out targetType);
                return new AiMetadata.TargetNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), targetType, onlyProb, node.rank, node.additionalId, node.additionalLevel, node.from, node.to, node.center, (NodeAiTarget)node.target, node.noChangeWhenNoTarget, node.initialCooltime, node.cooltime, node.isKeepBattle);
            case "say":
                return new AiMetadata.SayNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.message, onlyProb, node.durationTick, node.delayTick, node.initialCooltime, node.cooltime, node.isKeepBattle);
            case "SetValue":
                return new AiMetadata.SetValueNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.key, node.value, node.initialCooltime, node.cooltime, node.isModify, node.isKeepBattle);
            case "conditions":
                return new AiMetadata.ConditionsNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), childConditions.ToArray(), node.initialCooltime, node.cooltime, node.isKeepBattle);
            case "jump":
                NodeJumpType jumpType = NodeJumpType.JumpA;
                Enum.TryParse(node.type, out jumpType);
                return new AiMetadata.JumpNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.pos, node.speed, node.heightMultiplier, jumpType, node.cooltime, node.isKeepBattle);
            case "select":
                return new AiMetadata.SelectNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.prob, node.useNpcProb);
            case "move":
                return new AiMetadata.MoveNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.destination, onlyProb, node.animation, node.limit, node.speed, node.faceTarget, node.initialCooltime, node.cooltime, node.isKeepBattle);
            case "summon":
                return new AiMetadata.SummonNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.npcId, node.npcCountMax, node.npcCount, node.delayTick, node.lifeTime, node.summonRot, node.summonPos, node.summonPosOffset, node.summonTargetOffset, node.summonRadius, node.group, (NodeSummonMaster)node.master, Array.ConvertAll(node.option, value => (NodeSummonOption)value), node.cooltime, node.isKeepBattle);
            case "TriggerSetUserValue":
                return new AiMetadata.TriggerSetUserValueNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.triggerID, node.key, node.value, node.cooltime, node.isKeepBattle);
            case "ride":
                NodeRideType rideType = NodeRideType.Slave;
                Enum.TryParse(node.type, out rideType);
                return new AiMetadata.RideNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), rideType, node.isRideOff, node.rideNpcIDs);
            case "SetSlaveValue":
                return new AiMetadata.SetSlaveValueNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.key, node.value, node.isRandom, node.cooltime, node.isModify, node.isKeepBattle);
            case "SetMasterValue":
                return new AiMetadata.SetMasterValueNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.key, node.value, node.isRandom, node.cooltime, node.isModify, node.isKeepBattle);
            case "runaway":
                return new AiMetadata.RunawayNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.animation, node.skillIdx, node.till, node.limit, node.facePos, node.initialCooltime, node.cooltime);
            case "MinimumHp":
                return new AiMetadata.MinimumHpNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.hpPercent);
            case "buff":
                NodeBuffType buffType = NodeBuffType.Add;
                Enum.TryParse(node.type, out buffType);
                return new AiMetadata.BuffNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.id, buffType, node.level, onlyProb, node.initialCooltime, node.cooltime, node.isTarget, node.isKeepBattle);
            case "TargetEffect":
                return new AiMetadata.TargetEffectNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.effectName);
            case "ShowVibrate":
                return new AiMetadata.ShowVibrateNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.groupID);
            case "sidePopup":
                NodePopupType popupType = NodePopupType.Talk;
                Enum.TryParse(node.type, out popupType);
                return new AiMetadata.SidePopupNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), popupType, node.illust, node.duration, node.script, node.sound, node.voice);
            case "SetValueRangeTarget":
                return new AiMetadata.SetValueRangeTargetNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.key, node.value, node.height, node.radius, node.cooltime, node.isModify, node.isKeepBattle);
            case "announce":
                return new AiMetadata.AnnounceNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.message, node.durationTick, node.cooltime);
            case "ModifyRoomTime":
                return new AiMetadata.ModifyRoomTimeNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.timeTick, node.isShowEffect);
            case "HideVibrateAll":
                return new AiMetadata.HideVibrateAllNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.isKeepBattle);
            case "TriggerModifyUserValue":
                return new AiMetadata.TriggerModifyUserValueNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.triggerID, node.key, node.value);
            case "Buff":
                return new AiMetadata.BuffNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.id, NodeBuffType.Add, node.level, 100, 0, 0, false, true);
            case "RemoveSlaves":
                return new AiMetadata.RemoveSlavesNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.isKeepBattle);
            case "CreateRandomRoom":
                return new AiMetadata.CreateRandomRoomNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.randomRoomID, node.portalDuration);
            case "CreateInteractObject":
                return new AiMetadata.CreateInteractObjectNode(node.name, childNodes.ToArray(), childAiPresets.ToArray(), node.normal, node.interactID, node.lifeTime, node.kfmName, node.reactable);
            case "RemoveMe":
                return new AiMetadata.RemoveMeNode(node.name, childNodes.ToArray(), childAiPresets.ToArray());
            case "Suicide":
                return new AiMetadata.SuicideNode(node.name, childNodes.ToArray(), childAiPresets.ToArray());
            default:
                throw new NotImplementedException("unknown AI node name: " + node.name);
        }
    }
}
