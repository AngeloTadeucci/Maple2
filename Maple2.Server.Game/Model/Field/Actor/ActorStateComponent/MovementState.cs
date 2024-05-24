using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model.Enum;
using System.Numerics;
using Maple2.Model.Game;
using Maple2.Tools.Extensions;
using Maple2.Server.Game.Model.Skill;
using static Maple2.Server.Game.Model.Field.Actor.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;

public partial class MovementState {
    private readonly FieldNpc actor;

    public ActorState State { get; private set; }
    public ActorSubState SubState { get; private set; }
    public float Speed { get; private set; }
    public Vector3 Velocity { get; private set; }
    private AnimationSequence? stateSequence;
    #region LastTickData
    private float lastSpeed;
    private Vector3 lastVelocity;
    private ActorState lastState;
    private ActorSubState lastSubstate;
    private Vector3 lastPosition;
    private Vector3 lastFacing;
    private SkillRecord? lastCastSkill = null;
    #endregion
    private bool hasIdleA;
    private long lastTick = 0;
    private float speedOverride = 0;
    private float baseSpeed = 0;
    private readonly float aniSpeed = 1;

    #region WalkProperties
    private enum WalkType {
        None,
        Direction,
        MoveTo,
        ToTarget,
        FromTarget
    }
    private Vector3 walkDirection;
    private Vector3 walkTargetPosition;
    private float walkTargetDistance;
    private WalkType walkType = WalkType.None;
    private (Vector3 start, Vector3 end) walkSegment;
    private bool walkSegmentSet;
    private bool walkLookWhenDone = false;
    private AnimationSequence? walkSequence = null;
    private float walkSpeed;
    private NpcTask? walkTask = null;
    #endregion

    #region CastSkill
    private SkillRecord? castSkill = null;
    private NpcTask? castTask = null;
    #endregion

    #region Emote
    private NpcTask? emoteActionTask = null;
    #endregion

    #region Debugging
    private class DebugMarker {
        public bool Spawned;
        public Item ItemData;
        public FieldItem Item;
        public long NextUpdate = 0;
        public int ObjectId = 0;

        public DebugMarker(FieldItem item, Item itemData) {
            Item = item;
            ItemData = itemData;
        }
    }
    private DebugMarker debugNpc;
    private DebugMarker debugTarget;
    private DebugMarker debugAgent;
    #endregion

    public MovementState(FieldNpc actor) {
        this.actor = actor;

        State = ActorState.None;
        SubState = ActorSubState.None;
        hasIdleA = actor.AnimationState.RigMetadata?.Sequences?.ContainsKey("Idle_A") ?? false;
        aniSpeed = actor.Value.Metadata.Model.AniSpeed;

        SetState(ActorState.Spawn, ActorSubState.Idle_Idle);

        debugNpc = InitDebugMarker(30000071, 2);
        debugTarget = InitDebugMarker(50300014, 6);
        debugAgent = InitDebugMarker(30000084, 3);
    }

    private void UpdateMoveSpeed(float speed) {
        Stat moveSpeed = actor.Stats[BasicAttribute.MovementSpeed];

        speedOverride = speed;
        Speed = speed == 0 ? (float) moveSpeed.Current / 100 : speed;
        Speed *= baseSpeed;
    }

    private void StartWalking(string sequence, NpcTask task) {
        sequence = sequence == "" ? "Run_A" : sequence;
        walkSegmentSet = false;
        walkSpeed = Speed;

        emoteActionTask?.Cancel();

        bool isWalking = sequence.StartsWith("Walk_");

        baseSpeed = isWalking ? actor.Value.Metadata.Action.WalkSpeed : actor.Value.Metadata.Action.RunSpeed;

        if (actor.AnimationState.PlayingSequence?.Name == sequence || actor.AnimationState.TryPlaySequence(sequence, aniSpeed * Speed, AnimationType.Misc)) {
            stateSequence = actor.AnimationState.PlayingSequence;
            walkSequence = stateSequence;
            walkTask = task;

            SetState(ActorState.Walk, isWalking ? ActorSubState.Walk_Walking : ActorSubState.Walk_Running);
        } else {
            task.Cancel();

            Idle();
        }
    }

    public bool IsMovingToTarget() {
        return State == ActorState.Walk && walkType switch {
            WalkType.MoveTo => true,
            WalkType.FromTarget => true,
            WalkType.ToTarget => true,
            _ => false
        };
    }

    public NpcTask TryMoveDirection(Vector3 direction, bool isBattle, string sequence = "", float speed = 0) {
        walkTask?.Cancel();

        NpcTaskPriority priority = isBattle ? NpcTaskPriority.BattleWalk : NpcTaskPriority.IdleAction;

        return new NpcMoveDirectionTask(actor.TaskState, priority, this) {
            Direction = direction,
            Sequence = sequence,
            Speed = speed
        };
    }

    public NpcTask TryMoveTo(Vector3 position, bool isBattle, string sequence = "", float speed = 0, bool lookAt = false) {
        walkTask?.Cancel();

        NpcTaskPriority priority = isBattle ? NpcTaskPriority.BattleWalk : NpcTaskPriority.IdleAction;

        return new NpcMoveToTask(actor.TaskState, priority, this) {
            Position = position,
            Sequence = sequence,
            Speed = speed,
            LookAt = lookAt
        };
    }

    public NpcTask TryMoveTargetDistance(IActor target, float distance, bool isBattle, string sequence = "", float speed = 0) {
        walkTask?.Cancel();

        NpcTaskPriority priority = isBattle ? NpcTaskPriority.BattleWalk : NpcTaskPriority.IdleAction;

        return new NpcMoveTargetDistanceTask(actor.TaskState, priority, this, target) {
            Distance = distance,
            Sequence = sequence,
            Speed = speed
        };
    }

    public NpcTask TryStandby(IActor? target, string sequence = "") {
        return new NpcStandbyTask(actor.TaskState, this, sequence);
    }

    public NpcTask TryEmote(string sequenceName, bool isIdle) {
        return new NpcEmoteTask(actor.TaskState, this, sequenceName, isIdle);
    }

    //public bool TryJumpTo(Vector3 position, float height) {
    //
    //}
    //
    //public bool TryStun() {
    //
    //}
    //
    //public bool TryKnockback(Vector3 direction, float height) {
    //
    //}


    public NpcTask TryCastSkill(int id, short level, long uid) {
        walkTask?.Cancel();
        emoteActionTask?.Cancel();

        return new NpcSkillCastTask(actor.TaskState, this, id, level, uid);
    }

    private void SetState(ActorState state, ActorSubState subState) {
        if (State == ActorState.Dead) {
            return;
        }

        State = state;
        SubState = subState;
    }

    private void Idle(string sequence = "") {
        bool setAttackIdle = false;

        if (sequence == string.Empty) {
            setAttackIdle = actor.BattleState.InBattle;
            sequence = setAttackIdle ? "Attack_Idle_A" : "Idle_A";
        }

        SetState(ActorState.Idle, ActorSubState.Idle_Idle);

        if (hasIdleA) {
            if (actor.AnimationState.TryPlaySequence(sequence, aniSpeed, AnimationType.Misc)) {
                stateSequence = actor.AnimationState.PlayingSequence;
            } else if (setAttackIdle && actor.AnimationState.TryPlaySequence("Idle_A", aniSpeed, AnimationType.Misc)) {
                stateSequence = actor.AnimationState.PlayingSequence;
            }
        }
    }

    public void Died() {
        SetState(ActorState.Dead, ActorSubState.None);

        UpdateControl();

        Velocity = new Vector3(0, 0, 0);
    }

    public void StateRegenEvent(string keyName) {
        switch (keyName) {
            case "end":
                Idle();

                break;
            default:
                break;
        }
    }

    public void StateWalkEvent(string keyName) {
        switch (keyName) {
            case "end":
                if (State == ActorState.Walk) {
                    if (actor.AnimationState.TryPlaySequence(stateSequence!.Name, aniSpeed * Speed, AnimationType.Misc)) {
                        stateSequence = actor.AnimationState.PlayingSequence;
                    }

                    return;
                }
                walkTask?.Cancel();

                break;
            default:
                break;
        }
    }

    public void StateSkillEvent(string keyName) {
        switch (keyName) {
            case "end":
                castTask?.Completed();

                Idle();

                break;
            default:
                break;
        }
    }

    public void StateEmoteEvent(string keyName) {
        switch (keyName) {
            case "end":
                emoteActionTask?.Completed();

                Idle();

                break;
            default:
                break;
        }
    }

    public void KeyframeEvent(string keyName) {
        switch (State) {
            case ActorState.Regen:
                StateRegenEvent(keyName);
                break;
            case ActorState.Walk:
                StateWalkEvent(keyName);
                break;
            case ActorState.PcSkill:
                StateSkillEvent(keyName);
                break;
            case ActorState.Emotion:
            case ActorState.EmotionIdle:
                StateEmoteEvent(keyName);
                break;
            default:
                break;
        }
    }

    private void StateWalkUpdate(long tickCount, long tickDelta) {
        if (actor.Navigation is null) {
            return;
        }

        UpdateMoveSpeed(speedOverride);

        float delta = (float) tickDelta / 1000;

        if (walkType == WalkType.Direction) {
            Vector3 newPosition = actor.Position + delta * Speed * walkDirection;
            int searchRadius = Math.Max((int) (delta * Speed * 1.1f), 10);

            actor.Navigation.UpdatePosition();
            actor.Position = actor.Navigation.FindClosestPoint(newPosition, searchRadius);

            Velocity = Speed * walkDirection;

            UpdateDebugMarker(actor.Position, debugNpc, tickCount);
            UpdateDebugMarker(actor.Navigation.GetAgentPosition(), debugAgent, tickCount);

            return;
        }

        Vector3 offset = walkSegmentSet ? walkSegment.end - actor.Position : new Vector3(0, 0, 0);
        float distanceSquared = offset.LengthSquared();
        float tickDistance = delta * Speed;

        if (!walkSegmentSet || distanceSquared < tickDistance * tickDistance) {
            if (walkSegmentSet) {
                actor.Position = walkSegment.end;

                tickDistance -= (float) Math.Sqrt(distanceSquared);
            }

            walkSegment = actor.Navigation.Advance(TimeSpan.FromSeconds(1), Speed, out walkSegmentSet);

            offset = walkSegmentSet ? walkSegment.end - actor.Position : new Vector3(0, 0, 0);
            distanceSquared = offset.LengthSquared();

            if (!walkSegmentSet || distanceSquared == 0) {
                Vector3 walkTargetOffset = walkTargetPosition - actor.Position;
                float walkTargetDistance = walkTargetOffset.LengthSquared();

                if (walkLookWhenDone && walkTargetDistance > 0) {
                    actor.Transform.LookTo(Vector3.Normalize(walkTargetOffset));
                }

                walkTask?.Cancel();

                return;
            } else {
                float distance = (float) Math.Sqrt(distanceSquared);

                walkDirection = (1 / distance) * offset;
                tickDistance = Math.Min(tickDistance, distance);

                actor.Transform.LookTo(walkDirection);
            }
        }

        Velocity = Speed * walkDirection;
        actor.Position += tickDistance * walkDirection;

        Vector3 targetOffset = walkTargetPosition - actor.Position;
        float targetDistance = targetOffset.LengthSquared();
        float travelDistance = Speed * delta;

        UpdateDebugMarker(actor.Position, debugNpc, tickCount);
        UpdateDebugMarker(walkTargetPosition, debugTarget, tickCount);
        UpdateDebugMarker(actor.Navigation.GetAgentPosition(), debugAgent, tickCount);

        bool reached;

        if (walkType == WalkType.MoveTo) {
            reached = targetDistance < travelDistance * travelDistance;
        } else if (walkType == WalkType.ToTarget) {
            reached = targetDistance < walkTargetDistance * walkTargetDistance;
        } else {
            reached = targetDistance >= walkTargetDistance * walkTargetDistance;
        }

        if (reached) {
            Velocity = new Vector3(0, 0, 0);

            if (walkLookWhenDone) {
                actor.Transform.LookTo(Vector3.Normalize(targetOffset));
            }

            walkTask?.Completed();
        }
    }

    public void Update(long tickCount) {
        if (actor.AnimationState.PlayingSequence != stateSequence) {
            Idle();
        }

        Velocity = new Vector3(0, 0, 0);

        if (actor.Stats[BasicAttribute.Health].Current == 0) {
            return;
        }

        long tickDelta = Math.Min(lastTick == 0 ? 0 : tickCount - lastTick, 20);

        RemoveDebugMarker(debugNpc, tickCount);
        RemoveDebugMarker(debugTarget, tickCount);
        RemoveDebugMarker(debugAgent, tickCount);

        switch (State) {
            case ActorState.Walk:
                StateWalkUpdate(tickCount, tickDelta);
                break;
            case ActorState.Spawn:
                if (actor.AnimationState.TryPlaySequence("Regen_A", aniSpeed, AnimationType.Misc)) {
                    SetState(ActorState.Regen, ActorSubState.Idle_Idle);

                    stateSequence = actor.AnimationState.PlayingSequence;

                } else {
                    Idle();
                }
                break;
            default:
                break;
        }

        lastTick = tickCount;

        UpdateControl();
    }

    private void UpdateControl() {
        if (actor.Position.IsNearlyEqual(lastPosition, 1) && Velocity != new Vector3(0, 0, 0)) {
            Velocity = new Vector3(0, 0, 0);
        }

        actor.SendControl |= Speed != lastSpeed;
        actor.SendControl |= Velocity != lastVelocity;
        actor.SendControl |= State != lastState;
        actor.SendControl |= SubState != lastSubstate;
        actor.SendControl |= actor.Position != lastPosition;
        actor.SendControl |= actor.Transform.FrontAxis != lastFacing;
        actor.SendControl |= castSkill != lastCastSkill;

        actor.SendControl &= State != ActorState.PcSkill;

        lastSpeed = Speed;
        lastVelocity = Velocity;
        lastState = State;
        lastSubstate = SubState;
        lastPosition = actor.Position;
        lastFacing = actor.Transform.FrontAxis;
        lastCastSkill = castSkill;
    }

    private bool CanTransitionToState(ActorState state) {
        switch (State) {
            case ActorState.Idle:
                return state switch {
                    ActorState.Walk => true,
                    ActorState.PcSkill => true,
                    ActorState.Warp => true,
                    ActorState.Emotion => true,
                    ActorState.EmotionIdle => true,
                    _ => false
                };
            case ActorState.Walk:
                return state switch {
                    ActorState.Idle => true,
                    ActorState.Walk => true,
                    ActorState.PcSkill => true,
                    ActorState.Emotion => true,
                    ActorState.EmotionIdle => true,
                    _ => false
                };
            case ActorState.PcSkill:
                return state switch {
                    ActorState.Idle => true,
                    ActorState.PcSkill => true,
                    _ => false
                };
            case ActorState.Emotion:
                return state switch {
                    ActorState.Idle => true,
                    ActorState.Walk => true,
                    ActorState.EmotionIdle => true,
                    _ => false
                };
            case ActorState.EmotionIdle:
                return state switch {
                    ActorState.Idle => true,
                    ActorState.Walk => true,
                    ActorState.Emotion => true,
                    _ => false
                };
            default:
                return false;
        }
    }

    #region DebuggingImpl
    private DebugMarker InitDebugMarker(int itemId, int rarity) {
        actor.Field.ItemMetadata.TryGet(itemId, out ItemMetadata? itemData);

        if (itemData is null) {
            throw new InvalidDataException("bad item");
        }

        Item? item = actor.Field.ItemDrop.CreateItem(itemId, rarity);
        if (item == null) {
            throw new InvalidDataException("bad item");
        }

        FieldItem fieldItem = new FieldItem(actor.Field, 0, item) {
            FixedPosition = true,
            ReceiverId = -1,
            Type = DropType.Player
        };

        return new DebugMarker(fieldItem, item);
    }

    private void UpdateDebugMarker(Vector3 position, DebugMarker marker, long tickCount) {
        if (tickCount < marker.NextUpdate) {
            return;
        }

        marker.NextUpdate = tickCount + 50;

        if (marker.Spawned) {
            //actor.Field.Broadcast(FieldPacket.RemoveItem(marker.ObjectId));
        }

        marker.Spawned = true;
        //marker.ObjectId = actor.Field.RemoveMeNextLocalId();
        //
        //actor.Field.Broadcast(FieldPacket.DropDebugItem(marker.Item, marker.ObjectId, position, 1000000, 0, false));
    }

    private void RemoveDebugMarker(DebugMarker marker, long tickCount) {
        if (tickCount < marker.NextUpdate) {
            return;
        }

        if (!marker.Spawned) {
            return;
        }

        marker.NextUpdate = tickCount + 50;

        marker.Spawned = false;
        //actor.Field.Broadcast(FieldPacket.RemoveItem(marker.ObjectId));
        marker.ObjectId = 0;
    }
    #endregion
}
