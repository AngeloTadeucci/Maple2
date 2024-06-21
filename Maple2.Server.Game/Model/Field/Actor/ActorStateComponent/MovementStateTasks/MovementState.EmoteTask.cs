﻿using Maple2.Model.Enum;
using Maple2.Server.Game.Model.Enum;
using static Maple2.Server.Game.Model.Field.Actor.ActorStateComponent.TaskState;

namespace Maple2.Server.Game.Model.Field.Actor.ActorStateComponent;

public partial class MovementState {
    public class NpcEmoteTask : NpcTask {
        private MovementState movement;
        public string Sequence { get; init; } = string.Empty;
        public bool IsIdle { get; init; }
        override public bool CancelOnInterrupt { get => true; }
        public float Duration { get; init; }

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
    }

    private void Emote(NpcTask task, string sequence, bool isIdle, float duration) {
        if (!CanTransitionToState(ActorState.Emotion)) {
            task.Cancel();
            return;
        }

        if (!actor.AnimationState.TryPlaySequence(sequence, 1, AnimationType.Misc)) {
            task.Cancel();
            return;
        }

        if (duration > 0) {
            emoteLimitTick = actor.Field.FieldTick + (long) duration;
        }

        emoteActionTask = task;
        stateSequence = actor.AnimationState.PlayingSequence;

        SetState(isIdle ? ActorState.Emotion : ActorState.EmotionIdle);
    }
}
