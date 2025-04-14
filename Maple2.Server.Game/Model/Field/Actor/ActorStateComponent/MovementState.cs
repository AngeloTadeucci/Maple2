using System.Numerics;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model.Enum;
using Maple2.Server.Game.Model.Skill;
using Maple2.Tools.Extensions;
using static Maple2.Server.Game.Model.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.ActorStateComponent;

public partial class MovementState {
    private readonly FieldNpc actor;

    public ActorState State { get; private set; } = ActorState.None;
    public float Speed { get; private set; }
    public Vector3 Velocity { get; private set; }
    private AnimationSequenceMetadata? stateSequence;
    #region LastTickData
    private float lastSpeed;
    private Vector3 lastVelocity;
    private ActorState lastState;
    private Vector3 lastPosition;
    private Vector3 lastFacing;
    private SkillRecord? lastCastSkill;
    #endregion
    private bool hasIdleA;
    private long lastTick;
    private long lastControlTick;
    private float speedOverride;
    private float baseSpeed;
    private readonly float aniSpeed;

    #region Emote
    private NpcTask? emoteActionTask;
    #endregion

    public MovementState(FieldNpc actor) {
        this.actor = actor;

        hasIdleA = actor.Animation.RigMetadata?.Sequences?.ContainsKey("Idle_A") ?? false;
        aniSpeed = actor.Value.Metadata.Model.AniSpeed;

        SetState(ActorState.Spawn);

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
            Speed = speed,
        };
    }

    public NpcTask TryMoveTo(Vector3 position, bool isBattle, string sequence = "", float speed = 0, bool lookAt = false) {
        walkTask?.Cancel();

        NpcTaskPriority priority = isBattle ? NpcTaskPriority.BattleWalk : NpcTaskPriority.IdleAction;

        return new NpcMoveToTask(actor.TaskState, priority, this) {
            Position = position,
            Sequence = sequence,
            Speed = speed,
            LookAt = lookAt,
        };
    }

    public NpcTask TryMoveTargetDistance(IActor target, float distance, bool isBattle, string sequence = "", float speed = 0) {
        walkTask?.Cancel();

        NpcTaskPriority priority = isBattle ? NpcTaskPriority.BattleWalk : NpcTaskPriority.IdleAction;

        return new NpcMoveTargetDistanceTask(actor.TaskState, priority, this, target) {
            Distance = distance,
            Sequence = sequence,
            Speed = speed,
        };
    }

    public NpcTask TryStandby(IActor? target, bool isIdle, string sequence = "") {
        NpcTaskPriority priority = isIdle ? NpcTaskPriority.IdleAction : NpcTaskPriority.BattleStandby;

        return new NpcStandbyTask(actor.TaskState, this, sequence, priority, isIdle);
    }

    public NpcTask TryEmote(string sequenceName, bool isIdle, float duration = -1f) {
        NpcTaskPriority priority = isIdle ? NpcTaskPriority.Emote : NpcTaskPriority.BattleStandby;
        return new NpcEmoteTask(actor.TaskState, this, sequenceName, priority, isIdle, duration);
    }

    public NpcTask TryTalk() {
        return new NpcTalkTask(actor.TaskState, this, NpcTaskPriority.Interrupt);
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

    public NpcTask CleanupPatrolData(FieldPlayer player) {
        return new NpcCleanupPatrolDataTask(player, actor.TaskState, this);
    }

    private void SetState(ActorState state) {
        if (actor.IsDead) {
            return;
        }

        State = state;
    }

    private void Idle(string sequence = "") {
        bool setAttackIdle = false;

        if (sequence == string.Empty) {
            setAttackIdle = actor.BattleState.InBattle;
            sequence = setAttackIdle ? "Attack_Idle_A" : "Idle_A";
        }

        SetState(ActorState.Idle);

        if (hasIdleA) {
            if (actor.Animation.TryPlaySequence(sequence, aniSpeed, AnimationType.Misc)) {
                stateSequence = actor.Animation.PlayingSequence;
            } else if (setAttackIdle && actor.Animation.TryPlaySequence("Idle_A", aniSpeed, AnimationType.Misc)) {
                stateSequence = actor.Animation.PlayingSequence;
            }
        }
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
            case ActorState.Idle:
            case ActorState.Emotion:
            case ActorState.EmotionIdle:
                StateEmoteEvent(keyName);
                break;
            default:
                break;
        }
    }

    public void Update(long tickCount) {
        if (actor.Animation.PlayingSequence != stateSequence) {
            Idle();
        }

        Velocity = new Vector3(0, 0, 0);

        if (actor.IsDead) {
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
                if (actor.Animation.TryPlaySequence("Regen_A", aniSpeed, AnimationType.Misc)) {
                    SetState(ActorState.Regen);

                    stateSequence = actor.Animation.PlayingSequence;
                } else {
                    Idle();
                }
                break;
            case ActorState.PcSkill:
                StateSkillCastUpdate(tickCount, tickDelta);
                break;
            case ActorState.Idle:
            case ActorState.Emotion:
            case ActorState.EmotionIdle:
                EmoteStateUpdate(tickCount, tickDelta);
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

        actor.SendControl |= Math.Abs(Speed - lastSpeed) > 0.01f;
        actor.SendControl |= Velocity != lastVelocity;
        actor.SendControl |= State != lastState;
        actor.SendControl |= actor.Position != lastPosition;
        actor.SendControl |= actor.Transform.FrontAxis != lastFacing;
        actor.SendControl |= castSkill != lastCastSkill;

        lastSpeed = Speed;
        lastVelocity = Velocity;
        lastState = State;
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
                    ActorState.Talk => true,
                    _ => false,
                };
            case ActorState.Walk:
                return state switch {
                    ActorState.Idle => true,
                    ActorState.Walk => true,
                    ActorState.PcSkill => true,
                    ActorState.Emotion => true,
                    ActorState.EmotionIdle => true,
                    ActorState.Talk => true,
                    _ => false,
                };
            case ActorState.PcSkill:
                return state switch {
                    ActorState.Idle => true,
                    ActorState.PcSkill => true,
                    _ => false,
                };
            case ActorState.Emotion:
                return state switch {
                    ActorState.Idle => true,
                    ActorState.Walk => true,
                    ActorState.EmotionIdle => true,
                    ActorState.Talk => true,
                    _ => false,
                };
            case ActorState.EmotionIdle:
                return state switch {
                    ActorState.Idle => true,
                    ActorState.Walk => true,
                    ActorState.Emotion => true,
                    ActorState.Talk => true,
                    _ => false,
                };
            case ActorState.Talk:
                return state switch {
                    ActorState.Idle => true,
                    ActorState.Talk => true,
                    _ => false,
                };
            default:
                return false;
        }
    }
}
