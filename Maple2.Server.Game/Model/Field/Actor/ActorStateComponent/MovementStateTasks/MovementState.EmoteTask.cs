using Maple2.Model.Enum;
using Maple2.Server.Game.Model.Enum;
using static Maple2.Server.Game.Model.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.ActorStateComponent;

public partial class MovementState {
    public class NpcEmoteTask : NpcTask {
        private readonly MovementState movement;
        public string Sequence { get; init; }
        private bool IsIdle { get; init; }
        public override bool CancelOnInterrupt => true;
        private float Duration { get; init; }

        public NpcEmoteTask(TaskState taskState, MovementState movement, string sequence, NpcTaskPriority priority, bool isIdle, float duration) : base(taskState, priority) {
            this.movement = movement;
            this.movement.emoteLimitTick = 0;

            Sequence = sequence;
            IsIdle = isIdle;
            Duration = duration;
        }

        protected override void TaskResumed() {
            movement.Emote(this, Sequence, IsIdle, Duration);
        }

        protected override void TaskFinished(bool isCompleted) {
            movement.emoteLimitTick = 0;
            movement.Idle();
        }

        public override string ToString() {
            return $"{GetType().Name} (Priority: {Priority}, Status: {Status}, Sequence: {Sequence})";
        }
    }

    private void Emote(NpcTask task, string sequence, bool isIdle, float duration) {
        if (!CanTransitionToState(ActorState.Emotion)) {
            task.Cancel();
            return;
        }

        if (!actor.Animation.TryPlaySequence(sequence, 1, AnimationType.Misc)) {
            task.Cancel();
            return;
        }

        if (duration > 0) {
            emoteLimitTick = actor.Field.FieldTick + (long) duration;
        }

        emoteActionTask = task;
        stateSequence = actor.Animation.PlayingSequence;

        SetState(isIdle ? ActorState.Idle : ActorState.Emotion);
    }
}
