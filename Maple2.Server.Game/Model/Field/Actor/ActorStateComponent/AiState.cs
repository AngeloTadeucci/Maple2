using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Tools;
using Serilog;
using Serilog.Core;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using static Maple2.Model.Metadata.AiMetadata;
using static Maple2.Server.Game.Model.Field.Actor.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;

public class AiState {
    protected readonly ILogger Logger = Log.ForContext<AiState>();
    private readonly FieldNpc actor;
    public AiMetadata? AiMetadata { get; private set; }
    private Node? battle;
    private Node? battleEnd;

    private List<StackEntry> aiStack = new List<StackEntry>();
    private NpcTask? currentTask = null;
    private long currentLimitTick = 0;

    private AiMetadata? lastEvaluated;
    private DecisionTreeType currentTree;

    private Dictionary<string, long> coolTimes = new();
    private List<Node> reservedNodes = new List<Node>();

    private enum DecisionTreeType {
        None,
        Battle,
        BattleEnd
    }

    public AiState(FieldNpc actor) {
        this.actor = actor;
    }

    public bool IsProcessingNodes() {
        return aiStack.Count > 0;
    }

    // TODO: revisit later & refactor into constructor if no AI is found that needs this kind of context switching
    [MemberNotNullWhen(true, "AiMetadata")]
    public bool SetAi(string name) {
        if (name == string.Empty) {
            AiMetadata = null;

            return false;
        }

        if (name == "AI_DefaultNew.xml") {
            name = "AI_Default.xml";
        }

        AiMetadata? metadata;

        if (!actor.Field.AiMetadata.TryGet(name, out metadata)) {
            return false;
        }

        AiMetadata = metadata;

        if (AiMetadata.Battle.Length != 0) {
            battle = new Node("battle", AiMetadata.Battle);
        }

        if (AiMetadata.BattleEnd.Length != 0) {
            battleEnd = new Node("battle", AiMetadata.BattleEnd);
        }

        if (AiMetadata.Reserved.Length != 0) {
            reservedNodes = AiMetadata.Reserved.Cast<Node>().ToList();
        }

        return true;
    }

    public bool IsWaitingOnTask(long tickCount) {
        if (currentTask is null) {
            return false;
        }

        if (currentLimitTick != 0 && tickCount >= currentLimitTick) {
            return false;
        }

        return !currentTask.IsDone;
    }

    public void Update(long tickCount) {
        if (IsWaitingOnTask(tickCount)) {
            return;
        }

        currentTask?.Cancel();

        currentTask = null;
        currentLimitTick = 0;

        if (actor.IsDead) {
            return;
        }

        if (AiMetadata is null) {
            if (actor.Value.Metadata.AiPath != "") {
                actor.AppendDebugMessage("Missing AI\n");
                actor.AppendDebugMessage(actor.Value.Metadata.AiPath + "\n");
            }

            aiStack.Clear();
            currentTree = DecisionTreeType.None;

            return;
        }

        bool isInBattle = actor.BattleState.InBattle;

        if (!isInBattle) {
            if (currentTree == DecisionTreeType.Battle) {
                aiStack.Clear();

                currentTree = battleEnd is not null ? DecisionTreeType.BattleEnd : DecisionTreeType.None;
            } else if (currentTree == DecisionTreeType.BattleEnd && aiStack.Count == 0) {
                currentTree = DecisionTreeType.None;
            }

            return;
        } else if (currentTree == DecisionTreeType.BattleEnd) {
            aiStack.Clear();

            currentTree = DecisionTreeType.None;
        }

        if (isInBattle && battle is not null) {
            currentTree = DecisionTreeType.Battle;
        }

        if (lastEvaluated != AiMetadata) {
            aiStack.Clear();
        }

        lastEvaluated = AiMetadata;

        foreach (var node in reservedNodes.ToList()) {
            if (ProcessCondition((dynamic) node)) {
                aiStack.Clear();
                reservedNodes.Remove(node);
                Push(node);
            }
        }

        if (aiStack.Count == 0) {
            if (currentTree == DecisionTreeType.Battle) {
                Push(battle!);
            } else if (currentTree == DecisionTreeType.BattleEnd) {
                Push(battleEnd!);
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

            if (IsWaitingOnTask(tickCount)) {
                break;
            }
        }
    }

    private void SetNodeTask(NpcTask? task, long limit = 0) {
        currentTask = task;
        currentLimitTick = limit == 0 ? 0 : actor.Field.FieldTick + limit;
    }

    private struct StackEntry {
        public Node Node;
        public int Index;
        public bool LockIndex;
    }

    private void Push(Node node) {
        aiStack.Add(new StackEntry() { Node = node });
    }

    private void Push(AiPreset aiPreset) {
        actor.AppendDebugMessage($"> Preset: '{aiPreset.Name}'\n");

        AiPresetDefinition? definition = AiMetadata?.AiPresets.First(value => value.Name == aiPreset.Name);

        if (definition is null) {
            return;
        }

        Push(definition);
    }

    private SkillMetadata? TryGetSkill(int idx, short levelOverride = 0) {
        // idx starts at 1
        if (idx < 1 || idx > actor.Skills.Length) {
            actor.AppendDebugMessage($"{actor.Value.Metadata.Name}[{actor.Value.Id}]\n");
            actor.AppendDebugMessage($"{AiMetadata!.Name}\n");
            actor.AppendDebugMessage($"Invalid Skill Idx {idx}\n");

            Logger.Warning($"Missing skillIdx {idx} in {actor.Value.Metadata.Name}[{actor.Value.Id}] script '{AiMetadata!.Name}'. Xml.m2d/m2h might be out of date. Check the MS2 Hub resources!");

            return null;
        }

        SkillMetadata? skill = actor.Skills[idx - 1];

        if (skill is not null && levelOverride != 0) {
            actor.Field.SkillMetadata.TryGet(skill.Id, levelOverride, out skill);
        }

        if (skill is null) {
            actor.AppendDebugMessage($"{actor.Value.Metadata.Name}[{actor.Value.Id}]\n");
            actor.AppendDebugMessage($"{AiMetadata!.Name}\n");
            actor.AppendDebugMessage($"Missing Skill Idx {idx}\n");

            Logger.Warning($"Missing skillIdx {idx} in {actor.Value.Metadata.Name}[{actor.Value.Id}] script '{AiMetadata!.Name}'. Xml.m2d/m2h might be out of date. Check the MS2 Hub resources!");
        }

        return skill;
    }

    private void Push(Entry entry) {
        Push((dynamic) entry);
    }

    private bool CanEvaluateNode(Entry entry) {
        if (entry is not Node node) {
            return true;
        }

        bool hasTicked = coolTimes.TryGetValue(GetNodeKey(entry), out long lastTick);

        Type parent = entry.GetType();
        long? initialCooltime = parent.GetProperty("InitialCooltime")?.GetValue(entry, null) as long?;
        if (initialCooltime.HasValue && !hasTicked) {
            if (initialCooltime.Value > 0) {
                coolTimes[GetNodeKey(entry)] = actor.Field.FieldTick + initialCooltime.Value;
                return false;
            }
        }

        if (!hasTicked) {
            coolTimes[GetNodeKey(entry)] = 0;
            return true;
        }


        long? cooltime = parent.GetProperty("Cooltime")?.GetValue(entry, null) as long?;

        if (cooltime.HasValue) {
            return lastTick + cooltime.Value < actor.Field.FieldTick;
        }

        return lastTick < actor.Field.FieldTick;
    }

    private bool PerformOperation(AiConditionOp op, int value, int currentValue) {
        switch (op) {
            case AiConditionOp.Equal:
                return value == currentValue;
            case AiConditionOp.Greater:
                return value < currentValue;
            case AiConditionOp.Less:
                return value > currentValue;
            case AiConditionOp.GreaterEqual:
                return value <= currentValue;
            case AiConditionOp.LessEqual:
                return value >= currentValue;
            default:
                throw new ArgumentOutOfRangeException(nameof(op), op, null);
        }
    }
    private string GetNodeKey(Entry entry) {
        return $"{entry.Name}_{entry.GetHashCode()}";
    }

    private void Process(Entry entry) {
        if (entry is Node node) {
            actor.AppendDebugMessage($"> Node: {entry.Name}\n");

            ProcessNode((dynamic) entry);

            if (node.Entries.Length > 0 && aiStack.Last().Node is not SelectNode && aiStack.Last().Node != node && CanEvaluateNode(node.Entries[0])) {
                Push(node);
            }

            coolTimes[GetNodeKey(entry)] = actor.Field.FieldTick;
            return;
        }

        if (entry is AiPreset) {
            actor.AppendDebugMessage($"> AiPreset: {entry.Name}\n");
            Push(entry);
        }
    }

    private void ProcessNode(TraceNode node) {
        if (!actor.Field.TryGetActor(actor.BattleState.TargetId, out IActor? target)) {
            return;
        }

        float distance = 0;

        if (node.SkillIdx != 0) {
            SkillMetadata? skill = TryGetSkill(node.SkillIdx);

            if (skill is null) {
                return;
            }

            distance = skill.Data.Detect.Distance; // naive impl, might need to revisit
        }

        float speed = 1;

        if (node.Speed != 0) {
            speed = node.Speed;
        }

        NpcTask task = actor.MovementState.TryMoveTargetDistance(actor.BattleState.Target!, distance, true, node.Animation, speed);

        SetNodeTask(task, node.Limit);
    }

    private void ProcessNode(SkillNode node) {
        SkillMetadata? skill = TryGetSkill(node.Idx);

        if (skill is null) {
            return;
        }

        NpcTask task = actor.CastAiSkill(skill.Id, skill.Level, node.FaceTarget, node.FacePos);

        if (node.IsKeepBattle) {
            actor.BattleState.KeepBattle = true;
        }

        SetNodeTask(task, node.Limit);
    }

    private void ProcessNode(TeleportNode node) {
        actor.Position = node.Pos;

        Vector3 facePos = actor.BattleState.Target?.Position ?? new Vector3(0, 0, 0);

        if (node.Pos != new Vector3(0, 0, 0)) {
            facePos = node.Pos;
        }

        actor.Transform.LookTo(Vector3.Normalize(facePos - actor.Position));
    }

    private void ProcessNode(StandbyNode node) {
        NpcTask task = actor.MovementState.TryStandby(null, false);

        if (node.IsKeepBattle) {
            actor.BattleState.KeepBattle = true;
        }

        SetNodeTask(task, node.Limit);
    }

    private void ProcessNode(SetDataNode node) {
        actor.AiExtraData[node.Key] = node.Value;
    }

    private void ProcessNode(TargetNode node) {
        actor.BattleState.TargetNode = node;
    }

    private void ProcessNode(SayNode node) {
        actor.Field.Broadcast(CinematicPacket.BalloonTalk(true, actor.ObjectId, node.Message, node.DurationTick, node.DelayTick));

        actor.AppendDebugMessage($"Say '{node.Message}'\n");
    }

    private void ProcessNode(SetValueNode node) {
        // Assumed that we increment the current by the value if isModify is true
        if (node.IsModify) {
            if (actor.AiExtraData.TryGetValue(node.Key, out int oldValue)) {
                actor.AiExtraData[node.Key] = oldValue + node.Value;
                return;
            }
        }
        actor.AiExtraData[node.Key] = node.Value;
    }

    private void ProcessNode(ConditionsNode node) {
        Condition? passed = null;

        foreach (Condition condition in node.Conditions) {
            if (ProcessCondition((dynamic) condition)) {
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
        var weightedEntries = new WeightedSet<(Entry, int)>();

        for (int i = 0; i < node.Prob.Length; ++i) {
            if (!CanEvaluateNode(node.Entries[i])) {
                continue;
            }

            weightedEntries.Add((node.Entries[i], i), node.Prob[i]);
        }

        if (weightedEntries.Count == 0) {
            return;
        }

        (Entry entry, int index) selected = weightedEntries.Get();

        aiStack[aiStack.Count - 1] = new StackEntry { Node = node, Index = selected.index, LockIndex = true };

        Push(selected.entry);
        Process(selected.entry);
    }

    private void ProcessNode(MoveNode node) {
        NpcTask task = actor.MovementState.TryMoveTo(node.Destination, true, node.Animation, node.Speed);

        SetNodeTask(task, node.Limit);
    }

    private void ProcessNode(SummonNode node) {

    }

    private void ProcessNode(TriggerSetUserValueNode node) {
        actor.Field.UserValues[node.Key] = node.Value;
    }

    private void ProcessNode(RideNode node) {

    }

    private void ProcessNode(SetSlaveValueNode node) {

    }

    private void ProcessNode(SetMasterValueNode node) {

    }

    private void ProcessNode(RunawayNode node) {
        if (!actor.Field.TryGetActor(actor.BattleState.TargetId, out IActor? target)) {
            return;
        }

        float distance = float.MaxValue;

        if (node.SkillIdx != 0) {
            SkillMetadata? skill = TryGetSkill(node.SkillIdx);

            if (skill is null) {
                return;
            }

            distance = skill.Data.Detect.Distance; // naive impl, might need to revisit
        }

        NpcTask task = actor.MovementState.TryMoveTargetDistance(actor.BattleState.Target!, distance, true, node.Animation, 1);

        SetNodeTask(task, node.Limit);
    }

    private void ProcessNode(MinimumHpNode node) {
        Stat health = actor.Stats[BasicAttribute.Health];
        float currentHpPercent = ((float) health.Current / (float) health.Total) * 100f;

        if (currentHpPercent > node.HpPercent) {
            return;
        }

        // Heal back to node.HpPercent
        long newHp = (long) ((node.HpPercent / 100) * health.Total);
        actor.Stats[BasicAttribute.Health].Current = Math.Clamp(newHp, 0, health.Total);
    }

    private void ProcessNode(BuffNode node) {
        IActor target = actor;

        if (node.IsTarget) {
            IActor? newTarget = null;

            if (!actor.Field.TryGetActor(actor.BattleState.TargetId, out newTarget)) {
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
        actor.Field.Broadcast(NpcNoticePacket.TargetEffect(actor.BattleState.TargetId, node.EffectName));
    }

    private void ProcessNode(ShowVibrateNode node) {

    }

    private void ProcessNode(SidePopupNode node) {
        actor.Field.Broadcast(NpcNoticePacket.SidePopup(node.Type, node.Duration, node.Illust, node.Voice, node.Script, node.Sound));
    }

    private void ProcessNode(SetValueRangeTargetNode node) {
        int range = node.Radius;
        int height = node.Height;

        if (range == 0) {
            range = 10;
        }

        if (height == 0) {
            height = 10;
        }

        foreach (FieldNpc npc in actor.Field.EnumerateNpcs()) {
            if (Vector2.Distance(new Vector2(npc.Position.X, npc.Position.Y), new Vector2(actor.Position.X, actor.Position.Y)) > range) {
                continue;
            }

            if (npc.Position.Z < actor.Position.Z - height) {
                continue;
            }

            if (npc.Position.Z > actor.Position.Z + height) {
                continue;
            }

            if (node.IsModify) {
                if (npc.AiExtraData.TryGetValue(node.Key, out int oldValue)) {
                    actor.AiExtraData[node.Key] = oldValue + node.Value;
                    continue;
                }
            }
            actor.AiExtraData[node.Key] = node.Value;
        }
    }

    private void ProcessNode(AnnounceNode node) {
        actor.Field.Broadcast(NpcNoticePacket.Announce(node.Message, node.DurationTick));
    }

    private void ProcessNode(ModifyRoomTimeNode node) {

    }

    private void ProcessNode(HideVibrateAllNode node) {

    }

    private void ProcessNode(TriggerModifyUserValueNode node) {
        actor.Field.UserValues[node.Key] = node.Value;
    }

    private void ProcessNode(RemoveSlavesNode node) {

    }

    private void ProcessNode(CreateRandomRoomNode node) {

    }

    private void ProcessNode(CreateInteractObjectNode node) {

    }

    private void ProcessNode(RemoveMeNode node) {
        actor.Field.RemoveNpc(actor.ObjectId);
    }

    private void ProcessNode(SuicideNode node) {
        actor.Stats[BasicAttribute.Health].Current = 0;
    }


    private bool ProcessCondition(DistanceOverCondition node) {
        if (actor.BattleState.Target is null) {
            return false;
        }

        float targetDistance = (actor.BattleState.Target.Position - actor.Position).LengthSquared();

        return node.Value * node.Value > (int) targetDistance;
    }

    private bool ProcessCondition(CombatTimeCondition node) {
        return false;
    }

    private bool ProcessCondition(DistanceLessCondition node) {
        if (actor.BattleState.Target is null) {
            return false;
        }

        float targetDistance = (actor.BattleState.Target.Position - actor.Position).LengthSquared();

        return node.Value * node.Value < (int) targetDistance;
    }

    private bool ProcessCondition(SkillRangeCondition node) {
        SkillMetadata? skill = TryGetSkill(node.SkillIdx, node.SkillLev);

        if (skill is null) {
            return false;
        }

        if (actor.BattleState.Target is null) {
            return false;
        }

        float targetDistance = (actor.BattleState.Target.Position - actor.Position).LengthSquared();
        // +10 to account for the npc moving to skill range distance & allowing for it to attack slightly outside
        float detectDistance = skill.Data.Detect.Distance + 10;

        return detectDistance * detectDistance >= targetDistance; // naive, need to implement more sophisticated collision detection
    }

    private bool ProcessCondition(ExtraDataCondition node) {
        bool complete = PerformOperation(node.Op, node.Value, actor.AiExtraData.GetValueOrDefault(node.Key, 0));
        return complete;
    }

    private bool ProcessCondition(SlaveCountCondition node) {
        return false;
    }

    private bool ProcessCondition(SlaveCountOpCondition node) {
        return false;
    }

    private bool ProcessCondition(HpOverCondition node) {
        Stat health = actor.Stats[BasicAttribute.Health];

        return node.Value <= ((float) health.Current / (float) health.Total) * 100f;
    }

    private bool ProcessCondition(StateCondition node) {
        if (actor.BattleState.Target is null) {
            return false;
        }

        ActorState state = ActorState.None;

        if (actor.BattleState.Target is FieldPlayer player) {
            state = player.State;
        }

        if (actor.BattleState.Target is FieldNpc npc) {
            state = npc.MovementState.State;
        }

        return node.TargetState switch {
            AiConditionTargetState.GrabTarget => state == ActorState.GrabTarget,
            AiConditionTargetState.HoldMe => state == ActorState.Hold,
            _ => false
        };
    }

    private bool ProcessCondition(AdditionalCondition node) {
        if (actor.BattleState.Target is null && node.IsTarget) {
            return false;
        }
        if (node.IsTarget) {
            return actor.BattleState.Target!.Buffs.HasBuff(node.Id, node.Level, node.OverlapCount);
        }

        return actor.Buffs.HasBuff(node.Id, node.Level, node.OverlapCount);
    }

    private bool ProcessCondition(HpLessCondition node) {
        Stat health = actor.Stats[BasicAttribute.Health];

        return node.Value >= ((float) health.Current / (float) health.Total) * 100f;
    }

    private bool ProcessCondition(TrueCondition node) {
        return true;
    }
}
