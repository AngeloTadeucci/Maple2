using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Packets;
using Maple2.Tools;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using static Community.CsharpSqlite.Sqlite3;
using static Maple2.Model.Metadata.AiMetadata;

namespace Maple2.Server.Game.Model.Field.Actor.ActorState;

public class AiState {
    private readonly FieldNpc actor;
    public AiMetadata? AiMetadata { get; private set; }
    private Node? battle;

    private List<StackEntry> aiStack = new List<StackEntry>();

    private long nextUpdate = 0;
    private AiMetadata? lastEvaluated;

    public AiState(FieldNpc actor) {
        this.actor = actor;
    }

    // TODO: revisit later & refactor into constructor if no AI is found that needs this kind of context switching
    [MemberNotNullWhen(true, "AiMetadata")]
    public bool SetAi(string name) {
        if (name == string.Empty) {
            AiMetadata = null;

            return false;
        }

        AiMetadata? metadata;

        if (!actor.Field.AiMetadata.TryGet(name, out metadata)) {
            return false;
        }

        AiMetadata = metadata;

        battle = new Node("battle", AiMetadata.Battle);

        return true;
    }

    public void Update(long tickCount) {
        if (nextUpdate == 0) {
            nextUpdate = tickCount + 1000;
        }

        if (nextUpdate > tickCount) {
            return;
        }
        if (AiMetadata is null || lastEvaluated != AiMetadata) {
            if (actor.Value.Metadata.AiPath != "") {
                actor.AppendDebugMessage("Missing AI\n");
                actor.AppendDebugMessage(actor.Value.Metadata.AiPath);
            }

            aiStack.Clear();

            return;
        }

        lastEvaluated = AiMetadata;

        if (aiStack.Count == 0) {
            if (AiMetadata.Battle.Length == 0) {
                return;
            }

            if (battle is not null) {
                Push(battle);
            }
        }

        while (aiStack.Count > 0) {
            StackEntry entry = aiStack.Last();

            int index = entry.Index;
            int last = aiStack.Count - 1;

            if (index >= entry.Node.Entries.Length) {
                aiStack.RemoveAt(last);

                continue;
            }

            int nextIndex = entry.LockIndex ? entry.Node.Entries.Length : index + 1;

            aiStack[last] = new StackEntry() { Node = entry.Node, Index = nextIndex, LockIndex = entry.LockIndex };

            Process(entry.Node.Entries[index]);

            break;
        }

        // arbitrary delay to process 1 actionable node at a time to get something up and running.
        // almost certainly wrong, and probably depends on node properties & arbitrary task durations, and probably a value in the server constants.
        nextUpdate = tickCount + 1000;
    }

    private struct StackEntry {
        public Node Node;
        public int Index;
        public bool LockIndex;
    }

    void Push(Node node) {
        aiStack.Add(new StackEntry() { Node = node });
    }

    void Push(AiPreset aiPreset) {
        actor.AppendDebugMessage($"> Preset: '{aiPreset.Name}'\n");

        AiPresetDefinition? definition = AiMetadata?.AiPresets.First(value => value.Name == aiPreset.Name);

        if (definition is null) {
            return;
        }

        Push(definition);
    }

    void Push(Entry entry) {
        Push((dynamic) entry);
    }

    private bool CanEvaluateNode(Entry entry) {
        if (entry is not Node node) {
            return true;
        }

        // TODO: check cooltime

        return true;
    }

    private void Process(Entry entry) {
        if (entry is Node) {
            actor.AppendDebugMessage($"> Node: {entry.Name}\n");

            ProcessNode((dynamic) entry);

            return;
        }

    }

    private void ProcessNode(TraceNode node) {
        if (!actor.Field.TryGetActor(actor.TargetId, out IActor? target)) {
            return;
        }

        actor.Navigation?.PathTo(target.Position);
    }

    private void ProcessNode(SkillNode node) {
        //new SkillRecord()
        //actor.Field?.Broadcast(SkillPacket.Use());
        int skillId = 0;
        short skillLevel = 0;

        NpcMetadataSkill[] skills = actor.Value.Metadata.Property.Skills;

        // idx starts at 1
        if (node.Idx > skills.Length) {
            actor.AppendDebugMessage($"{actor.Value.Metadata.Name}[{actor.Value.Id}]\n");
            actor.AppendDebugMessage($"{ AiMetadata!.Name}\n");
            actor.AppendDebugMessage($"Invalid Skill Idx {node.Idx}\n");

            return;
        }

        skillId = skills[node.Idx - 1].Id;
        skillLevel = (short)skills[node.Idx - 1].Level;

        SkillRecord? cast = actor.CastSkill(skillId, skillLevel);

        if (cast is null) {
            actor.AppendDebugMessage($"Failed cast {skillId}[{skillLevel}]\n");

            return;
        }
    }

    private void ProcessNode(TeleportNode node) {
        actor.Position = node.Pos;
    }

    private void ProcessNode(StandbyNode node) {
        actor.Navigation?.PathTo(actor.Position); // stop moving
    }

    private void ProcessNode(SetDataNode node) {

    }

    private void ProcessNode(TargetNode node) {

    }

    private void ProcessNode(SayNode node) {
        actor.Field.Broadcast(CinematicPacket.BalloonTalk(true, actor.ObjectId, node.Message, node.DurationTick, node.DelayTick));

        actor.AppendDebugMessage($"Say '{node.Message}'");
    }

    private void ProcessNode(SetValueNode node) {

    }

    private void ProcessNode(ConditionsNode node) {
        Condition? passed = null;

        foreach (Condition condition in node.Conditions) {
            if (ProcessCondition((dynamic)condition)) {
                passed = condition;

                break;
            }
        }

        if (passed is null) {
            actor.AppendDebugMessage("Failed conditions\n");

            return;
        }

        actor.AppendDebugMessage($"+ Condition: '{passed.Name}'\n");

        Push(passed);
    }

    private void ProcessNode(JumpNode node) {

    }

    private void ProcessNode(SelectNode node) {
        var weightedEntries = new WeightedSet<int>();

        bool foundZeroProb = false;

        for (int i = 0; i < node.Prob.Length; ++i) {
            if (node.Prob[i] == 0) {
                foundZeroProb = true;

                continue;
            }

            if (!CanEvaluateNode(node.Entries[i])) {
                continue;
            }

            weightedEntries.Add(i, node.Prob[i]);
        }

        int selectedIndex = -1;

        if (weightedEntries.Count > 0) {
            int stackIndex = weightedEntries.Get();

            selectedIndex = stackIndex;
        }
        else if (foundZeroProb) {
            // only evaluate if prob=0 nodes are found and none of the weighted ones passed
            for (int i = 0; i < node.Prob.Length; ++i) {
                if (i >= node.Entries.Length) {
                    break;
                }

                if (node.Prob[i] == 0 && CanEvaluateNode(node.Entries[i])) {
                    selectedIndex = i;

                    break;
                }
            }
        }

        if (selectedIndex == -1) {
            return;
        }

        aiStack[aiStack.Count - 1] = new StackEntry { Node = node, Index = selectedIndex, LockIndex = true };

        Push(node.Entries[selectedIndex]);
        Process(node.Entries[selectedIndex]);
    }

    private void ProcessNode(MoveNode node) {
        actor.Navigation?.PathTo(node.Destination);
    }

    private void ProcessNode(SummonNode node) {

    }

    private void ProcessNode(TriggerSetUserValueNode node) {

    }

    private void ProcessNode(RideNode node) {

    }

    private void ProcessNode(SetSlaveValueNode node) {

    }

    private void ProcessNode(SetMasterValueNode node) {

    }

    private void ProcessNode(RunawayNode node) {
        if (!actor.Field.TryGetActor(actor.TargetId, out IActor? target)) {
            return;
        }

        Vector3 offset = actor.Position - target.Position;

        actor.Navigation?.PathTo(target.Position + (1000 / offset.Length()) * offset);
    }

    private void ProcessNode(MinimumHpNode node) {

    }

    private void ProcessNode(BuffNode node) {
        IActor target = actor;

        if (node.IsTarget) {
            IActor? newTarget = null;

            if (!actor.Field.TryGetActor(actor.TargetId, out newTarget)) {
                return;
            }

            target = newTarget;
        }
        
        if (node.Type == NodeBuffType.Remove) {
            target.Buffs.Remove(node.Id);

            return;
        }

        target.Buffs.AddBuff(actor, actor, node.Id, node.Level);
    }

    private void ProcessNode(TargetEffectNode node) {
        actor.Field.Broadcast(NpcNoticePacket.TargetEffect(actor.TargetId, node.EffectName));
    }

    private void ProcessNode(ShowVibrateNode node) {

    }

    private void ProcessNode(SidePopupNode node) {
        actor.Field.Broadcast(NpcNoticePacket.SidePopup(node.Type, node.Duration, node.Illust, node.Voice, node.Script, node.Sound));
    }

    private void ProcessNode(SetValueRangeTargetNode node) {

    }

    private void ProcessNode(AnnounceNode node) {
        actor.Field.Broadcast(NpcNoticePacket.Announce(node.Message, node.DurationTick));
    }

    private void ProcessNode(ModifyRoomTimeNode node) {

    }

    private void ProcessNode(HideVibrateAllNode node) {

    }

    private void ProcessNode(TriggerModifyUserValueNode node) {

    }

    private void ProcessNode(RemoveSlavesNode node) {

    }

    private void ProcessNode(CreateRandomRoomNode node) {

    }

    private void ProcessNode(CreateInteractObjectNode node) {

    }

    private void ProcessNode(RemoveMeNode node) {

    }

    private void ProcessNode(SuicideNode node) {

    }


    private bool ProcessCondition(DistanceOverCondition node) {
        return false;
    }

    private bool ProcessCondition(CombatTimeCondition node) {
        return false;
    }

    private bool ProcessCondition(DistanceLessCondition node) {
        return false;
    }

    private bool ProcessCondition(SkillRangeCondition node) {
        return false;
    }

    private bool ProcessCondition(ExtraDataCondition node) {
        return false;
    }

    private bool ProcessCondition(SlaveCountCondition node) {
        return false;
    }

    private bool ProcessCondition(SlaveCountOpCondition node) {
        return false;
    }

    private bool ProcessCondition(HpOverCondition node) {
        return false;
    }

    private bool ProcessCondition(StateCondition node) {
        return false;
    }

    private bool ProcessCondition(AdditionalCondition node) {
        return false;
    }

    private bool ProcessCondition(HpLessCondition node) {
        return false;
    }

    private bool ProcessCondition(TrueCondition node) {
        return true;
    }
}
