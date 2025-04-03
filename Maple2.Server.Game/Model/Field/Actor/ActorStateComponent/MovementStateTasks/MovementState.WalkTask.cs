using Maple2.Model.Enum;
using Maple2.Server.Game.Model.Enum;
using System.Numerics;
using static Maple2.Server.Game.Model.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.ActorStateComponent;
public partial class MovementState {

    public class NpcMoveDirectionTask : NpcTask {
        private readonly MovementState movement;

        public Vector3 Direction { get; init; }
        public string Sequence { get; init; } = string.Empty;
        public float Speed { get; init; }
        public override bool CancelOnInterrupt => Priority == NpcTaskPriority.IdleAction;

        public NpcMoveDirectionTask(TaskState taskState, NpcTaskPriority priority, MovementState movement) : base(taskState, priority) {
            this.movement = movement;
        }

        protected override void TaskResumed() {
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
        private readonly MovementState movement;
        public Vector3 Position { get; init; }
        public string Sequence { get; init; } = "";
        public float Speed { get; init; }
        public bool LookAt { get; init; }
        public override bool CancelOnInterrupt => Priority == NpcTaskPriority.IdleAction;

        public NpcMoveToTask(TaskState taskState, NpcTaskPriority priority, MovementState movement) : base(taskState, priority) {
            this.movement = movement;
        }

        protected override void TaskResumed() {
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
        private readonly MovementState movement;
        public IActor Target { get; init; }
        public float Distance { get; init; }
        public string Sequence { get; init; } = string.Empty;
        public float Speed { get; init; }
        public override bool CancelOnInterrupt => Priority == NpcTaskPriority.IdleAction;

        public NpcMoveTargetDistanceTask(TaskState taskState, NpcTaskPriority priority, MovementState movement, IActor target) : base(taskState, priority) {
            this.movement = movement;
            Target = target;
        }

        protected override void TaskResumed() {
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
            foundPath = actor.Navigation.PathAwayFrom(target.Position, (int) distance);
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
}
