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
    private long lastControlTick = 0;
    private float speedOverride = 0;
    private float baseSpeed = 0;
    private readonly float aniSpeed = 1;

    #region Emote
    private NpcTask? emoteActionTask = null;
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

    public NpcTask TryStandby(IActor? target, bool isIdle, string sequence = "") {
        NpcTaskPriority priority = isIdle ? NpcTaskPriority.IdleAction : NpcTaskPriority.BattleStandby;
        return new NpcStandbyTask(actor.TaskState, this, sequence, priority, isIdle);
    }

    public NpcTask TryEmote(string sequenceName, bool isIdle) {
        NpcTaskPriority priority = isIdle ? NpcTaskPriority.IdleAction : NpcTaskPriority.BattleStandby;
        return new NpcEmoteTask(actor.TaskState, this, sequenceName, priority, isIdle);
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


    public NpcTask TryCastSkill(int id, short level, int faceTarget, Vector3 facePos, long uid) {
        walkTask?.Cancel();
        emoteActionTask?.Cancel();

        return new NpcSkillCastTask(actor.TaskState, this, id, level, faceTarget, facePos, uid);
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

    public void Update(long tickCount) {
        if (actor.AnimationState.PlayingSequence != stateSequence) {
            Idle();
        }

        Velocity = new Vector3(0, 0, 0);

        if (actor.Stats.Values[BasicAttribute.Health].Current == 0) {
            SetState(ActorState.Dead, ActorSubState.None);

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
            case ActorState.PcSkill:
                StateSkillCastUpdate(tickCount, tickDelta);
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

        if (lastControlTick < actor.Field.FieldTick) {
            actor.SendControl = true;
            lastControlTick = actor.Field.FieldTick + Constant.MaxNpcControlDelay;
        }

        actor.SendControl |= Speed != lastSpeed;
        actor.SendControl |= Velocity != lastVelocity;
        actor.SendControl |= State != lastState;
        actor.SendControl |= SubState != lastSubstate;
        actor.SendControl |= actor.Position != lastPosition;
        actor.SendControl |= actor.Transform.FrontAxis != lastFacing;
        actor.SendControl |= castSkill != lastCastSkill;

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
}
