using Maple2.Server.Game.Model.Enum;
using static Maple2.Server.Game.Model.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.ActorStateComponent;

public partial class MovementState {
    public class NpcCleanupPatrolDataTask : NpcTask {
        private readonly MovementState movement;
        public override bool CancelOnInterrupt => false;

        public NpcCleanupPatrolDataTask(TaskState taskState, MovementState movement) : base(taskState, NpcTaskPriority.Cleanup) {
            this.movement = movement;
        }

        protected override void TaskResumed() {
            if (movement.actor.Patrol is null) {
                movement.actor.Field.RemoveNpc(movement.actor.ObjectId);
            }
        }
    }
}
