using Maple2.Model.Enum;
using Maple2.Server.Game.Model.Enum;
using Maple2.Server.Game.Model.Skill;
using System.Numerics;
using static Maple2.Server.Game.Model.Field.Actor.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;

public partial class MovementState {

    public class NpcMoveDirectionTask : NpcTask {
        private MovementState movement;

        public Vector3 Direction { get; init; }
        public string Sequence { get; init; } = string.Empty;
        public float Speed { get; init; }
        override public bool CancelOnInterrupt { get => Priority == NpcTaskPriority.IdleAction; }

        public NpcMoveDirectionTask(TaskState taskState, NpcTaskPriority priority, MovementState movement) : base(taskState, priority) {
            this.movement = movement;
        }

        override protected void TaskResumed() {
            movement.MoveDirection(this, Direction, Sequence, Speed);
        }

        protected override void TaskPaused() {
            movement.Idle();
        }

        protected override void TaskFinished(bool isCompleted) {
            movement.walkTask = null;
            movement.Idle();
        }
    }

    private void MoveDirection(NpcTask task, Vector3 direction, string sequence, float speed) {
        if (!CanTransitionToState(ActorState.Walk)) {
            task.Cancel();

            return;
        }

        walkDirection = Vector3.Normalize(direction);
        walkType = WalkType.Direction;
        walkTask = task;

        actor.Transform.LookTo(walkDirection);

        UpdateMoveSpeed(speed);
        StartWalking(sequence, task);
    }

    public class NpcMoveToTask : NpcTask {
        private MovementState movement;
        public Vector3 Position { get; init; }
        public string Sequence { get; init; } = "";
        public float Speed { get; init; }
        public bool LookAt { get; init; }
        override public bool CancelOnInterrupt { get => Priority == NpcTaskPriority.IdleAction; }

        public NpcMoveToTask(TaskState taskState, NpcTaskPriority priority, MovementState movement) : base(taskState, priority) {
            this.movement = movement;
        }

        override protected void TaskResumed() {
            movement.MoveTo(this, Position, Sequence, Speed, LookAt);
        }

        protected override void TaskPaused() {
            movement.Idle();
        }

        protected override void TaskFinished(bool isCompleted) {
            movement.walkTask = null;
            movement.Idle();
        }
    }

    private void MoveTo(NpcTask task, Vector3 position, string sequence, float speed, bool lookAt) {
        if (!CanTransitionToState(ActorState.Walk)) {
            task.Cancel();

            return;
        }

        if (actor.Navigation is null) {
            task.Cancel();

            return;
        }

        actor.Navigation.UpdatePosition();

        actor.AppendDebugMessage($"> Pathing to position\n");

        if (!actor.Navigation.PathTo(position)) {
            task.Cancel();

            return;
        }

        walkTargetPosition = position;
        walkType = WalkType.MoveTo;
        walkLookWhenDone = lookAt;
        walkTask = task;

        UpdateMoveSpeed(speed);
        StartWalking(sequence, task);
    }

    public class NpcMoveTargetDistanceTask : NpcTask {
        private MovementState movement;
        public IActor Target { get; init; }
        public float Distance { get; init; }
        public string Sequence { get; init; } = string.Empty;
        public float Speed { get; init; }
        override public bool CancelOnInterrupt { get => Priority == NpcTaskPriority.IdleAction; }

        public NpcMoveTargetDistanceTask(TaskState taskState, NpcTaskPriority priority, MovementState movement, IActor target) : base(taskState, priority) {
            this.movement = movement;
            Target = target;
        }

        override protected void TaskResumed() {
            movement.MoveTargetDistance(this, Target, Distance, Sequence, Speed);
        }

        protected override void TaskPaused() {
            movement.Idle();
        }

        protected override void TaskFinished(bool isCompleted) {
            movement.walkTask = null;
            movement.Idle();
        }
    }

    private void MoveTargetDistance(NpcTask task, IActor target, float distance, string sequence, float speed) {
        if (!CanTransitionToState(ActorState.Walk)) {
            task.Cancel();

            return;
        }

        if (actor.Navigation is null) {
            task.Cancel();

            return;
        }

        float currentDistance = (actor.Position - target.Position).LengthSquared();
        bool foundPath = false;
        WalkType type = WalkType.ToTarget;
        float fromDistance = Math.Max(0, distance - 10);
        float toDistance = distance + 10;

        actor.Navigation.UpdatePosition();

        if (currentDistance < fromDistance * fromDistance) {
            actor.AppendDebugMessage($"> Pathing away target\n");
            foundPath = actor.Navigation.PathAway(target.Position, (int) distance);
            type = WalkType.FromTarget;
        } else if (currentDistance > toDistance * toDistance) {
            actor.AppendDebugMessage($"> Pathing to target\n");
            foundPath = actor.Navigation.PathTo(target.Position);
        }

        if (!foundPath) {
            task.Cancel();

            return;
        }

        walkTargetPosition = target.Position;
        walkTargetDistance = distance;
        walkType = type;
        walkLookWhenDone = true;
        walkTask = task;

        UpdateMoveSpeed(speed);
        StartWalking(sequence, task);
    }

    public class NpcStandbyTask : NpcTask {
        private MovementState movement;
        public IActor? Target { get; init; }
        public string Sequence { get; init; } = string.Empty;
        public bool IsIdle { get; init; }
        override public bool CancelOnInterrupt { get => true; }

        public NpcStandbyTask(TaskState taskState, MovementState movement, string sequence, NpcTaskPriority priority, bool isIdle) : base(taskState, priority) {
            this.movement = movement;
        }

        override protected void TaskResumed() {
            movement.Standby(this, Target, IsIdle, Sequence);
        }
    }

    private void Standby(NpcTask task, IActor? target, bool isIdle, string sequence) {
        Idle(sequence);

        if (target is null) {
            return;
        }

        actor.Transform.LookTo(Vector3.Normalize(target.Position - actor.Position));
    }

    public class NpcEmoteTask : NpcTask {
        private MovementState movement;
        public string Sequence { get; init; } = string.Empty;
        public bool IsIdle { get; init; }
        override public bool CancelOnInterrupt { get => true; }

        public NpcEmoteTask(TaskState taskState, MovementState movement, string sequence, NpcTaskPriority priority, bool isIdle) : base(taskState, priority) {
            this.movement = movement;
            Sequence = sequence;
            IsIdle = isIdle;
        }

        override protected void TaskResumed() {
            movement.Emote(this, Sequence, IsIdle);
        }
        protected override void TaskFinished(bool isCompleted) {
            movement.Idle();
        }
    }

    private void Emote(NpcTask task, string sequence, bool isIdle) {
        if (!CanTransitionToState(ActorState.Emotion)) {
            task.Cancel();

            return;
        }

        if (!actor.AnimationState.TryPlaySequence(sequence, 1, AnimationType.Misc)) {
            task.Cancel();

            return;
        }

        ActorSubState subState = ActorSubState.EmotionIdle_Idle;

        if (isIdle) {
            subState = sequence switch {
                "Bore_A" => ActorSubState.EmotionIdle_Bore_A,
                "Bore_B" => ActorSubState.EmotionIdle_Bore_B,
                "Bore_C" => ActorSubState.EmotionIdle_Bore_C,
                _ => ActorSubState.EmotionIdle_Idle
            };
        }

        emoteActionTask = task;
        stateSequence = actor.AnimationState.PlayingSequence;

        SetState(isIdle ? ActorState.Emotion : ActorState.EmotionIdle, subState);
    }

    class NpcSkillCastTask : NpcTask {
        private MovementState movement;

        public SkillRecord? Cast;
        public int SkillId { get; init; }
        public short SkillLevel { get; init; }
        public long SkillUid { get; init; }

        public NpcSkillCastTask(TaskState queue, MovementState movement, int id, short level, long uid) : base(queue, NpcTaskPriority.BattleAction) {
            this.movement = movement;
            SkillId = id;
            SkillLevel = level;
            SkillUid = uid;
        }

        override protected void TaskResumed() {
            movement.SkillCast(this, SkillId, SkillLevel, SkillUid);
        }

        protected override void TaskFinished(bool isCompleted) {
            movement.castSkill = null;
            movement.castTask = null;
            movement.Idle();
            movement.actor.AppendDebugMessage((isCompleted ? "Finished" : "Canceled") + " cast\n");
        }
    }

    private void SkillCast(NpcSkillCastTask task, int id, short level, long uid) {
        castTask?.Cancel();

        if (!CanTransitionToState(ActorState.PcSkill)) {
            task.Cancel();

            return;
        }

        Velocity = new Vector3(0, 0, 0);

        SkillRecord? cast = actor.CastSkill(id, level, uid);

        if (cast is null) {
            return;
        }

        if (!actor.AnimationState.TryPlaySequence(cast.Motion.MotionProperty.SequenceName, cast.Motion.MotionProperty.SequenceSpeed, AnimationType.Skill)) {
            task.Cancel();

            return;
        }

        castTask = task;
        castSkill = cast;
        task.Cast = cast;

        //if (faceTarget && actor.BattleState.Target is not null) {
        //    actor.Transform.LookTo(Vector3.Normalize(actor.BattleState.Target.Position - actor.Position));
        //}

        SetState(ActorState.PcSkill, ActorSubState.Skill_Default);

        stateSequence = actor.AnimationState.PlayingSequence;

        return;
    }
}
