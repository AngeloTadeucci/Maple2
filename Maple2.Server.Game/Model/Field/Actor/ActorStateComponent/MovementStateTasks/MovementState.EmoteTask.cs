using Maple2.Model.Enum;
using Maple2.Server.Game.Model.Enum;
using static Maple2.Server.Game.Model.Field.Actor.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;

public partial class MovementState {
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
}
