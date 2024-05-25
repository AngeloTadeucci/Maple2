using Maple2.Server.Game.Model.Enum;
using System.Numerics;
using static Maple2.Server.Game.Model.Field.Actor.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;

public partial class MovementState {
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
}
