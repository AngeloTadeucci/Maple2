using Maple2.Server.Game.Model.Enum;
using System.Numerics;
using static Maple2.Server.Game.Model.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.ActorStateComponent;

public partial class MovementState {
    public class NpcStandbyTask : NpcTask {
        private readonly MovementState movement;
        public IActor? Target { get; init; }
        private string Sequence { get; init; }
        private bool IsIdle { get; init; }
        public override bool CancelOnInterrupt => true;

        public NpcStandbyTask(TaskState taskState, MovementState movement, string sequence, NpcTaskPriority priority, bool isIdle) : base(taskState, priority) {
            this.movement = movement;
            Sequence = sequence;
            IsIdle = isIdle;
        }

        protected override void TaskResumed() {
            movement.Standby(this, Target, IsIdle, Sequence);
        }
    }

    private void Standby(NpcTask task, IActor? target, bool isIdle, string sequence) {
        Idle(sequence);
        dontIdleOnStateEnd = isIdle;

        if (target is null) {
            return;
        }

        actor.Transform.LookTo(Vector3.Normalize(target.Position - actor.Position));
    }
}
