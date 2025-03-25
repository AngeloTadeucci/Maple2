﻿using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Model.State;
using Maple2.Server.Game.Packets;
using Maple2.Tools;
using Maple2.Tools.Collision;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;
using static Maple2.Server.Game.Model.ActorStateComponent.TaskState;
using Maple2.Server.Game.Model.Enum;
using Maple2.Server.Core.Packets;
using DotRecast.Detour.Crowd;
using Maple2.Server.Game.Model.ActorStateComponent;
using MovementState = Maple2.Server.Game.Model.ActorStateComponent.MovementState;

namespace Maple2.Server.Game.Model;

public class FieldNpc : Actor<Npc> {
    #region Control
    public bool SendControl;
    private long lastUpdate;

    private Vector3 velocity;
    private NpcState state;
    private short sequenceId;
    public override Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
    public override Vector3 Rotation {
        get => Transform.RotationAnglesDegrees;
        set {
            Transform.RotationAnglesDegrees = value;
            SendControl = true;
        }
    }
    public Vector3 Velocity {
        get => velocity;
        set {
            if (value == velocity) return;
            velocity = value;
            SendControl = true;
        }
    }
    public NpcState State {
        get => state;
        [MemberNotNull(nameof(state))]
        set {
            state = value;
            SendControl = true;
        }
    }
    public short SequenceId {
        get => sequenceId;
        set {
            if (value == sequenceId) return;
            sequenceId = value;
            SendControl = true;
        }
    }
    public short SequenceCounter { get; set; }
    #endregion

    public required Vector3 Origin { get; init; }

    public FieldMobSpawn? Owner { get; init; }
    public override IPrism Shape => new Prism(
        new Circle(new Vector2(Position.X, Position.Y), Value.Metadata.Property.Capsule.Radius),
        Position.Z,
        Value.Metadata.Property.Capsule.Height
    );

    public readonly AgentNavigation? Navigation;
    public readonly AnimationSequence IdleSequence;
    public readonly AnimationSequence? JumpSequence;
    public readonly AnimationSequence? WalkSequence;
    public readonly AnimationSequence? SpawnSequence;
    private readonly WeightedSet<string> defaultRoutines;
    public readonly AiState AiState;
    public readonly MovementState MovementState;
    public readonly BattleState BattleState;
    public readonly TaskState TaskState;
    public readonly SkillMetadata?[] Skills;

    public int SpawnPointId = 0;

    public MS2PatrolData? Patrol { get; private set; }
    private int currentWaypointIndex;

    private bool hasBeenBattling = false;
    private NpcTask? idleTask;
    private long idleTaskLimitTick = 0;

    public readonly Dictionary<string, int> AiExtraData = new();

    public FieldNpc(FieldManager field, int objectId, DtCrowdAgent? agent, Npc npc, string aiPath, string spawnAnimation = "", string? patrolDataUUID = null) : base(field, objectId, npc, npc.Metadata.Model.Name, field.NpcMetadata) {
        IdleSequence = npc.Animations.GetValueOrDefault("Idle_A") ?? new AnimationSequence(string.Empty, -1, 1f, null);
        JumpSequence = npc.Animations.GetValueOrDefault("Jump_A") ?? npc.Animations.GetValueOrDefault("Jump_B");
        WalkSequence = npc.Animations.GetValueOrDefault("Walk_A");
        SpawnSequence = npc.Animations.GetValueOrDefault(spawnAnimation);
        defaultRoutines = new WeightedSet<string>();
        foreach (NpcAction action in Value.Metadata.Action.Actions) {
            defaultRoutines.Add(action.Name, action.Probability);
        }

        if (agent is not null) {
            Navigation = Field.Navigation.ForAgent(this, agent);

            if (patrolDataUUID is not null) {
                Patrol = field.Entities.Patrols.FirstOrDefault(x => x.Uuid == patrolDataUUID);
            }
        }
        MovementState = new MovementState(this);
        BattleState = new BattleState(this);
        TaskState = new TaskState(this);
        State = new NpcState();
        SequenceId = -1;
        SequenceCounter = 1;
        AiState = new AiState(this, aiPath);

        Skills = new SkillMetadata[Value.Metadata.Skill.Entries.Length];

        for (int i = 0; i < Skills.Length; ++i) {
            var entry = Value.Metadata.Skill.Entries[i];
            Field.SkillMetadata.TryGet(entry.Id, entry.Level, out Skills[i]);
        }
    }

    protected override void Dispose(bool disposing) { }

    protected virtual void Remove(int delay) => Field.RemoveNpc(ObjectId, delay);

    private List<string> debugMessages = new List<string>();
    private bool playersListeningToDebug = false; // controls whether messages should log
    private long nextDebugPacket = 0;

    public override void Update(long tickCount) {
        if (IsDead) return;

        base.Update(tickCount);

        // controls whether currently logged messages should print
        bool playersListeningToDebugNow = false;

        foreach ((int objectId, FieldPlayer player) in Field.Players) {
            if (player.DebugAi) {
                playersListeningToDebugNow = true;

                break;
            }
        }

        bool isSpawning = MovementState.State == ActorState.Spawn || MovementState.State == ActorState.Regen;

        if (!isSpawning) {
            BattleState.Update(tickCount);
            AiState.Update(tickCount);
            DoIdleBehavior(tickCount);
        }

        MovementState.Update(tickCount);

        if (!isSpawning) {
            TaskState.Update(tickCount);
        }

        bool sentDebugPacket = false;

        if (tickCount >= nextDebugPacket && playersListeningToDebugNow && debugMessages.Count > 0) {
            sentDebugPacket = true;

            Field.BroadcastAiMessage(CinematicPacket.BalloonTalk(false, ObjectId, String.Join("", debugMessages.ToArray()), 2500, 0));
        }

        if (sentDebugPacket || tickCount >= nextDebugPacket) {
            debugMessages.Clear();
        }

        playersListeningToDebug = playersListeningToDebugNow;

        if (SendControl) {
            SequenceCounter++;
            Field.BroadcastNpcControl(this);
            SendControl = false;
        }
        lastUpdate = tickCount;
    }

    private void DoIdleBehavior(long tickCount) {
        hasBeenBattling |= BattleState.InBattle;

        if (BattleState.InBattle) {
            idleTask?.Cancel();
            idleTask = null;

            return;
        }

        if (hasBeenBattling && idleTask is null) {
            Vector3 spawnPoint = Navigation?.GetRandomPatrolPoint() ?? Origin;

            idleTask = MovementState.TryMoveTo(spawnPoint, false);
            hasBeenBattling = false;
        }

        if (idleTask is MovementState.NpcStandbyTask && idleTaskLimitTick == 0) {
            idleTaskLimitTick = tickCount + 1000;
        }

        bool hitLimit = idleTaskLimitTick != 0 && tickCount >= idleTaskLimitTick;

        if (!hasBeenBattling && (idleTask is null || idleTask.IsDone || hitLimit)) {
            idleTaskLimitTick = 0;

            idleTask = NextRoutine(tickCount);
        }
    }

    public override void KeyframeEvent(string keyName) {
        MovementState.KeyframeEvent(keyName);
    }

    private NpcTask? NextRoutine(long tickCount) {
        if (Patrol?.WayPoints.Count > 0 && Navigation is not null) {
            return NextWaypoint();
        }

        string routineName = defaultRoutines.Get();
        if (!Value.Animations.TryGetValue(routineName, out AnimationSequence? sequence)) {
            Logger.Error("Invalid routine: {Routine} for npc {NpcId}", routineName, Value.Metadata.Id);

            return MovementState.TryStandby(null, true);
        }

        switch (routineName) {
            case { } when routineName.Contains("Idle_"):
                return MovementState.TryStandby(null, true, sequence.Name);
            case { } when routineName.Contains("Bore_"):
                return MovementState.TryEmote(sequence.Name, true);
            case { } when routineName.StartsWith("Walk_"): {
                    return MovementState.TryMoveTo(Navigation?.GetRandomPatrolPoint() ?? Position, false, sequence.Name);
                }
            case { } when routineName.StartsWith("Run_"):
                return MovementState.TryMoveTo(Navigation?.GetRandomPatrolPoint() ?? Position, false, sequence.Name);
            case { }:
                if (!Value.Animations.TryGetValue(routineName, out AnimationSequence? animationSequence)) {
                    break;
                }
                return MovementState.TryEmote(animationSequence.Name, SpawnSequence is not null);
        }

        Logger.Warning("Unhandled routine: {Routine} for npc {NpcId}", routineName, Value.Metadata.Id);

        return MovementState.TryStandby(null, true);
    }

    private NpcTask? NextWaypoint() {
        MS2WayPoint currentWaypoint = Patrol!.WayPoints[currentWaypointIndex];

        if (!string.IsNullOrEmpty(currentWaypoint.ArriveAnimation) && idleTask is not MovementState.NpcEmoteTask) {
            if (Value.Animations.TryGetValue(currentWaypoint.ArriveAnimation, out AnimationSequence? arriveSequence)) {
                return MovementState.TryEmote(arriveSequence.Name, false);
            }
        }

        NpcTask? approachTask = null;

        if (Navigation!.PathTo(currentWaypoint.Position)) {
            if (Value.Animations.TryGetValue(currentWaypoint.ApproachAnimation, out AnimationSequence? patrolSequence)) {
                approachTask = MovementState.TryMoveTo(currentWaypoint.Position, false, sequence: patrolSequence.Name);
            } else if (WalkSequence is not null) {
                approachTask = MovementState.TryMoveTo(currentWaypoint.Position, false, WalkSequence.Name);
            } else {
                Logger.Warning("No walk sequence found for npc {NpcId} in patrol {PatrolId}", Value.Metadata.Id, Patrol.Uuid);
            }
        } else {
            Logger.Warning("Failed to path to waypoint id({Id}) coord {Coord} for npc {NpcName} - {NpcId} in patrol {PatrolId}", currentWaypoint.Id, currentWaypoint.Position, Value.Metadata.Name, Value.Metadata.Id, Patrol.Uuid);
        }

        MS2WayPoint lastWaypoint = Patrol.WayPoints.Last();

        // if we're at the last waypoint and we're not looping, we're done
        if (currentWaypoint.Id == lastWaypoint.Id && !Patrol.IsLoop) {
            Patrol = null;

            return approachTask;
        }

        currentWaypointIndex = (currentWaypointIndex + 1) % Patrol.WayPoints.Count;

        if ((approachTask?.Status ?? NpcTaskStatus.Cancelled) == NpcTaskStatus.Cancelled) {
            Logger.Warning("Failed to path to waypoint id({Id}) coord {Coord} for npc {NpcName} - {NpcId} in patrol {PatrolId}", currentWaypoint.Id, currentWaypoint.Position, Value.Metadata.Name, Value.Metadata.Id, Patrol.Uuid);
            return MovementState.TryStandby(null, true);
        }

        return approachTask;
    }

    protected override void OnDeath() {
        Owner?.Despawn(ObjectId);
        SendControl = false;

        HandleDamageDealers();

        Remove(delay: (int) (Value.Metadata.Dead.Time * 1000));
    }

    public virtual void Animate(string sequenceName, float duration = -1f) {
        if (!Value.Animations.TryGetValue(sequenceName, out AnimationSequence? sequence)) {
            Logger.Error("Invalid sequence: {Sequence} for npc {NpcId}", sequenceName, Value.Metadata.Id);
            return;
        }

        bool isIdle = sequenceName.Contains("idle", StringComparison.OrdinalIgnoreCase);

        idleTask = MovementState.TryEmote(sequence.Name, isIdle, duration);
    }

    public void Talk() {
        if (SpawnSequence is not null) {
            return;
        }
        idleTask = MovementState.TryTalk();
    }

    public void StopTalk() {
        if (idleTask is MovementState.NpcTalkTask) {
            idleTask.Cancel();
        }
    }

    public void DropLoot(FieldPlayer firstPlayer) {
        NpcMetadataDropInfo dropInfo = Value.Metadata.DropInfo;

        ICollection<Item> itemDrops = new List<Item>();
        foreach (int globalDropId in dropInfo.GlobalDropBoxIds) {
            itemDrops = itemDrops.Concat(Field.ItemDrop.GetGlobalDropItems(globalDropId, Value.Metadata.Basic.Level)).ToList();
        }

        foreach (int individualDropId in dropInfo.IndividualDropBoxIds) {
            itemDrops = itemDrops.Concat(Field.ItemDrop.GetIndividualDropItems(firstPlayer.Session, Value.Metadata.Basic.Level, individualDropId)).ToList();
        }

        foreach (Item item in itemDrops) {
            float x = Random.Shared.Next((int) Position.X - Value.Metadata.DropInfo.DropDistanceRandom, (int) Position.X + Value.Metadata.DropInfo.DropDistanceRandom);
            float y = Random.Shared.Next((int) Position.Y - Value.Metadata.DropInfo.DropDistanceRandom, (int) Position.Y + Value.Metadata.DropInfo.DropDistanceRandom);
            var position = new Vector3(x, y, Position.Z);

            FieldItem fieldItem = Field.SpawnItem(this, position, Rotation, item, firstPlayer.Value.Character.Id);
            Field.Broadcast(FieldPacket.DropItem(fieldItem));
        }
    }


    public override SkillRecord? CastSkill(int id, short level, long uid = 0, byte motionPoint = 0) {
        if (!Field.SkillMetadata.TryGet(id, level, out SkillMetadata? metadata) || metadata.Data.Motions.Length <= motionPoint) {
            Logger.Error("Invalid skill use: {SkillId},{Level},{motionPoint}", id, level, motionPoint);
            return null;
        }

        if (uid == 0) {
            // The client derives the player's skill cast skillSn/uid using this formula so I'm using it here for mob casts for parity.
            uid = (long) ((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds * 100000) % (long) 1e14;
        }

        Field.Broadcast(NpcControlPacket.Control(this));

        var cast = base.CastSkill(id, level, uid, motionPoint);

        return cast;
    }

    public NpcTask CastAiSkill(int id, short level, int faceTarget, Vector3 facePos, long uid = 0) {
        return MovementState.TryCastSkill(id, level, faceTarget, facePos, uid);
    }

    // mob drops, exp, etc.
    private void HandleDamageDealers() {
        // TODO: Fix drop loot. Right now we're getting the first player in damage dealers as the receiver of the loot.
        // How it should work is the person who instigated the first attack on the mob gets tagged. As long as the mob is in aggro, it stays on them, regardless if aggro changes.
        // If the mob stops aggro to everyone, it resets this and heals/removes all damage records.
        // Boss drop loot is different. They drop for everyone who did damage to them.

        if (!Field.TryGetPlayer(DamageDealers.FirstOrDefault().Key, out FieldPlayer? firstPlayer)) {
            return;
        }

        foreach (KeyValuePair<int, DamageRecordTarget> damageDealer in DamageDealers) {
            if (!Field.TryGetPlayer(damageDealer.Key, out FieldPlayer? player)) {
                continue;
            }

            DropLoot(firstPlayer);
            GiveExp(player);

            player.Session.ConditionUpdate(ConditionType.npc, codeLong: Value.Id, targetLong: Field.MapId);
            foreach (string tag in Value.Metadata.Basic.MainTags) {
                player.Session.ConditionUpdate(ConditionType.npc_race, codeString: tag);
            }
        }
    }

    private void GiveExp(FieldPlayer player) {
        // 0 means no exp
        if (Value.Metadata.Basic.CustomExp == 0) {
            return;
        }

        if (Value.Metadata.Basic.CustomExp == -1) {
            // TODO: this is temporary. We need to know how to split exp between players.
            player.Session.Exp.AddMobExp(Value.Metadata.Basic.Level);
            return;
        }

        player.Session.Exp.AddExp(Value.Metadata.Basic.CustomExp);
    }

    public void SendDebugAiInfo(GameSession requester) {
        string message = $"{ObjectId}";
        message += "\n" + (AiState.AiMetadata?.Name ?? "[No AI]");
        if (this is FieldPet pet) {
            if (Field.TryGetPlayer(pet.OwnerId, out FieldPlayer? player)) {
                message += "\nOwner: " + player.Value.Character.Name;
            }
        }
        requester.Send(CinematicPacket.BalloonTalk(false, ObjectId, message, 2500, 0));
    }

    public void AppendDebugMessage(string message, bool sanitize = false) {
        if (!playersListeningToDebug) {
            return;
        }

        if (sanitize) {
            message = message.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        if (debugMessages.Count > 0 && debugMessages.Last().Last() != '\n') {
            debugMessages.Add("\n");
        }

        debugMessages.Add(message);

        if (debugMessages.Last().Last() != '\n') {
            Field.BroadcastAiMessage(NoticePacket.Message($"{ObjectId}: {message}"));
        } else {
            string trimmedMessage = message.Substring(0, Math.Max(0, message.Length - 1));

            Field.BroadcastAiMessage(NoticePacket.Message($"{ObjectId}: {trimmedMessage}"));
        }
    }

    public void SetPatrolData(MS2PatrolData newPatrolData) {
        Patrol = newPatrolData;
        currentWaypointIndex = 0;
    }

    public void ClearPatrolData() {
        if (Patrol is null) {
            return;
        }

        MS2WayPoint currentWaypoint = Patrol.WayPoints[currentWaypointIndex];

        // make sure we're at the last checkpoint in the list
        if (currentWaypoint.Id != Patrol.WayPoints.Last().Id) {
            return;
        }

        // Clear patrol data
        Patrol = null;
        currentWaypointIndex = 0;
    }
}
