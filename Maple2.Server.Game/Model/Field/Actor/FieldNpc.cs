using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PathEngine;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model.Routine;
using Maple2.Server.Game.Model.State;
using Maple2.Server.Game.Packets;
using Maple2.Tools;
using Maple2.Tools.Collision;

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

    public readonly AgentNavigation Navigation;
    public readonly AnimationSequence IdleSequence;
    public readonly AnimationSequence? JumpSequence;
    private readonly WeightedSet<string> defaultRoutines;
    private NpcRoutine CurrentRoutine { get; set; }

    // Used for trigger spawn tracking.
    public int SpawnPointId = -1;

    public override Stats Stats { get; }
    public int TargetId = 0;

    public FieldNpc(FieldManager field, int objectId, Agent agent, Npc npc) : base(field, objectId, npc) {
        IdleSequence = npc.Animations.GetValueOrDefault("Idle_A") ?? new AnimationSequence(-1, 1f);
        JumpSequence = npc.Animations.GetValueOrDefault("Jump_A") ?? npc.Animations.GetValueOrDefault("Jump_B");
        defaultRoutines = new WeightedSet<string>();
        foreach (NpcAction action in Value.Metadata.Action.Actions) {
            defaultRoutines.Add(action.Name, action.Probability);
        }

        Navigation = Field.Navigation.ForAgent(this, agent);
        CurrentRoutine = new WaitRoutine(this, -1, 1f);
        Stats = new Stats(npc.Metadata.Stat);

        State = new NpcState();
        SequenceId = -1;
        SequenceCounter = 1;
    }

    protected override void Dispose(bool disposing) {
        Navigation.Dispose();
    }

    protected virtual void Remove(int delay) => Field.RemoveNpc(ObjectId, delay);

    public override void Update(long tickCount) {
        if (IsDead) return;

        base.Update(tickCount);

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
                    if (Navigation.RandomPatrol()) {
                        return MoveRoutine.Walk(this, sequence.Id);
                    }
                    return new WaitRoutine(this, IdleSequence.Id, sequence.Time);
                }
            case { } when routineName.StartsWith("Run_"):
                if (Field.TryGetPlayer(TargetId, out FieldPlayer? target) && Navigation.PathTo(target.Position)) {
                    return MoveRoutine.Run(this, sequence.Id);
                }
                if (Navigation.RandomPatrol()) {
                    return MoveRoutine.Run(this, sequence.Id);
                }
                return new WaitRoutine(this, IdleSequence.Id, sequence.Time);
        }

        Logger.Warning("Unhandled routine: {Routine} for npc {NpcId}", routineName, Value.Metadata.Id);
        return new WaitRoutine(this, sequence.Id, 1f);
    }

    protected override void OnDeath() {
        Owner?.Despawn(ObjectId);
        CurrentRoutine.OnCompleted();
        SendControl = false;
        Remove(delay: (int) (Value.Metadata.Dead.Time * 1000));
    }
}
