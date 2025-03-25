using Maple2.Model.Enum;
using Maple2.Server.Game.Model.Enum;
using Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;
using static Maple2.Server.Game.Model.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.ActorStateComponent;

public partial class MovementState {
    public class NpcTalkTask : NpcTask {
        private readonly MovementState movement;

        public NpcTalkTask(TaskState queue, MovementState movement, NpcTaskPriority priority) : base(queue, priority) {
            this.movement = movement;
        }

        protected override void TaskResumed() {
            movement.walkTask?.Pause();
            movement.emoteActionTask?.Pause();
        }

        protected override void TaskFinished(bool isCompleted) {
            movement.walkTask?.Resume();
            movement.emoteActionTask?.Resume();
        }
    }
}
