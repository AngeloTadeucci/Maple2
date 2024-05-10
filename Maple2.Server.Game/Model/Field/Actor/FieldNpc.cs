using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PathEngine;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model.Routine;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Model.State;
using Maple2.Server.Game.Packets;
using Maple2.Tools;
using Maple2.Tools.Collision;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Model.Field.Actor.ActorState;

namespace Maple2.Server.Game.Model;

public class FieldNpc : Actor<Npc> {
    #region Control
    public bool SendControl;
    private long lastUpdate;

    private Vector3 rotation;
    private Vector3 velocity;
    private NpcState state;
    private short sequenceId;
    public override Vector3 Position { get; set; }
    public override Vector3 Rotation {
        get => rotation;
        set {
            if (value == rotation) return;
            rotation = value;
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
    private readonly WeightedSet<string> defaultRoutines;
    public readonly AiState AiState;
    private NpcRoutine CurrentRoutine { get; set; }

    // Used for trigger spawn tracking.
    public int SpawnPointId = -1;

    public override Stats Stats { get; }
    public int TargetId = 0;
    private readonly MS2PatrolData? patrolData;
    private int currentWaypointIndex = 0;

    public FieldNpc(FieldManager field, int objectId, Agent? agent, Npc npc, string? patrolDataUUID = null) : base(field, objectId, npc) {
        IdleSequence = npc.Animations.GetValueOrDefault("Idle_A") ?? new AnimationSequence(-1, 1f, null);
        JumpSequence = npc.Animations.GetValueOrDefault("Jump_A") ?? npc.Animations.GetValueOrDefault("Jump_B");
        WalkSequence = npc.Animations.GetValueOrDefault("Walk_A");
        defaultRoutines = new WeightedSet<string>();
        foreach (NpcAction action in Value.Metadata.Action.Actions) {
            defaultRoutines.Add(action.Name, action.Probability);
        }

        if (agent is not null) {
            Navigation = Field.Navigation.ForAgent(this, agent);

            if (patrolDataUUID is not null) {
                patrolData = field.Entities.Patrols.FirstOrDefault(x => x.Uuid == patrolDataUUID);
            }
        }
        CurrentRoutine = new WaitRoutine(this, -1, 1f);
        Stats = new Stats(npc.Metadata.Stat);
        AiState = new AiState(this);

        State = new NpcState();
        SequenceId = -1;
        SequenceCounter = 1;

        AiState.SetAi(npc.Metadata.AiPath);
    }

    protected override void Dispose(bool disposing) {
        Navigation?.Dispose();
    }

    protected virtual void Remove(int delay) => Field.RemoveNpc(ObjectId, delay);

    private List<string> debugMessages = new List<string>();
    private bool playersListeningToDebug = false; // controls whether messages should log

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

        AiState.Update(tickCount);

        if (playersListeningToDebugNow && debugMessages.Count > 0) {
            Field.BroadcastAiMessage(CinematicPacket.BalloonTalk(true, ObjectId, String.Join("", debugMessages.ToArray()), 2500, 0));
        }

        debugMessages.Clear();
        playersListeningToDebug = playersListeningToDebugNow;

        NpcRoutine.Result result = CurrentRoutine.Update(TimeSpan.FromMilliseconds(tickCount - lastUpdate));
        if (result is NpcRoutine.Result.Success or NpcRoutine.Result.Failure) {
            CurrentRoutine = CurrentRoutine.NextRoutine?.Invoke() ?? NextRoutine();
        }

        if (SendControl) {
            SequenceCounter++;
            Field.Broadcast(NpcControlPacket.Control(this));
            SendControl = false;
        }
        lastUpdate = tickCount;
    }

    private NpcRoutine NextRoutine() {
        if (patrolData is not null && Navigation is not null && patrolData.WayPoints.Count > 0) {
            MS2WayPoint waypoint = patrolData.WayPoints[currentWaypointIndex];

            if (!string.IsNullOrEmpty(waypoint.ArriveAnimation) && CurrentRoutine is not AnimateRoutine) {
                if (Value.Animations.TryGetValue(waypoint.ArriveAnimation, out AnimationSequence? arriveSequence)) {
                    return new AnimateRoutine(this, arriveSequence);
                }
            }

            currentWaypointIndex++;

            if (currentWaypointIndex >= patrolData.WayPoints.Count) {
                currentWaypointIndex = 0;
            }

            waypoint = patrolData.WayPoints[currentWaypointIndex];

            if (Navigation.PathTo(waypoint.Position)) {
                if (Value.Animations.TryGetValue(waypoint.ApproachAnimation, out AnimationSequence? patrolSequence)) {
                    if (waypoint.ApproachAnimation.StartsWith("Walk_")) {
                        return MoveRoutine.Walk(this, patrolSequence.Id);
                    } else if (waypoint.ApproachAnimation.StartsWith("Run_")) {
                        return MoveRoutine.Run(this, patrolSequence.Id);
                    }
                }
                if (WalkSequence is not null) {
                    return MoveRoutine.Walk(this, WalkSequence.Id);
                }

                // Log.Logger.Warning("No walk sequence found for npc {NpcId} in patrol {PatrolId}", Value.Metadata.Id, patrolData.Uuid);
                return new WaitRoutine(this, IdleSequence.Id, 1f);
            } else {
                // Log.Logger.Warning("Failed to path to waypoint index({WaypointIndex}) coord {Coord} for npc {NpcId} in patrol {PatrolId}", currentWaypointIndex, waypoint.Position, Value.Metadata.Name, patrolData.Uuid);
                return new WaitRoutine(this, IdleSequence.Id, 1f);
            }
        }


        string routineName = defaultRoutines.Get();
        if (!Value.Animations.TryGetValue(routineName, out AnimationSequence? sequence)) {
            Logger.Error("Invalid routine: {Routine} for npc {NpcId}", routineName, Value.Metadata.Id);
            return new WaitRoutine(this, IdleSequence.Id, 1f);
        }

        switch (routineName) {
            case { } when routineName.Contains("Idle_"):
                return new WaitRoutine(this, sequence.Id, sequence.Time);
            case { } when routineName.Contains("Bore_"):
                return new WaitRoutine(this, sequence.Id, sequence.Time);
            case { } when routineName.StartsWith("Walk_"): {
                    if (Navigation is not null && Navigation.RandomPatrol()) {
                        return MoveRoutine.Walk(this, sequence.Id);
                    }
                    return new WaitRoutine(this, IdleSequence.Id, sequence.Time);
                }
            case { } when routineName.StartsWith("Run_"):
                if (Field.TryGetPlayer(TargetId, out FieldPlayer? target) && Navigation is not null && Navigation.PathTo(target.Position)) {
                    return MoveRoutine.Run(this, sequence.Id);
                }
                if (Navigation is not null && Navigation.RandomPatrol()) {
                    return MoveRoutine.Run(this, sequence.Id);
                }
                return new WaitRoutine(this, IdleSequence.Id, sequence.Time);
            case { }:
                // Check if routine is an animation
                if (!Value.Animations.TryGetValue(routineName, out AnimationSequence? animationSequence)) {
                    break;
                }
                return new AnimateRoutine(this, animationSequence);
        }

        Logger.Warning("Unhandled routine: {Routine} for npc {NpcId}", routineName, Value.Metadata.Id);
        return new WaitRoutine(this, sequence.Id, 1f);
    }

    protected override void OnDeath() {
        Owner?.Despawn(ObjectId);
        CurrentRoutine.OnCompleted();
        SendControl = false;

        HandleDamageDealers();

        Remove(delay: (int) (Value.Metadata.Dead.Time * 1000));
    }

    public virtual void Animate(string sequenceName, float duration = -1f) {
        if (!Value.Animations.TryGetValue(sequenceName, out AnimationSequence? sequence)) {
            Logger.Error("Invalid sequence: {Sequence} for npc {NpcId}", sequenceName, Value.Metadata.Id);
            return;
        }

        CurrentRoutine = new AnimateRoutine(this, sequence, duration);
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

    public override SkillRecord? CastSkill(int id, short level, long uid = 0) {
        SkillRecord? cast = base.CastSkill(id, level, uid);

        if (cast is null) {
            return null;
        }

        cast.ServerTick = (int) Field.FieldTick;

        return cast;
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
            player.Session.ConditionUpdate(ConditionType.npc, codeLong: Value.Id);
            foreach (string tag in Value.Metadata.Basic.MainTags) {
                player.Session.ConditionUpdate(ConditionType.npc_race, codeString: tag);
            }
        }
    }

    public void SendDebugAiInfo(GameSession requester) {
        string message = $"{ObjectId}";
        message += "\n" + (AiState.AiMetadata?.Name ?? "[No AI]");
        if (this is FieldPet pet) {
            if (Field.TryGetPlayer(pet.OwnerId, out FieldPlayer? player)) {
                message += "\nOwner: " + player.Value.Character.Name;
            }
        }
        requester.Send(CinematicPacket.BalloonTalk(true, ObjectId, message, 2500, 0));
    }

    public void AppendDebugMessage(string message) {
        if (!playersListeningToDebug) {
            return;
        }

        if (debugMessages.Count > 0 && debugMessages.Last().Last() != '\n') {
            debugMessages.Add("\n");
        }

        debugMessages.Add(message);
    }
}
