using Maple2.Server.Game.Model.Enum;

namespace Maple2.Server.Game.Model.ActorStateComponent;

public partial class MovementState {
    private long emoteLimitTick;

    private void EmoteStateUpdate(long tickCount, long tickDelta) {
        if (tickCount >= emoteLimitTick && emoteLimitTick != 0) {
            emoteActionTask?.Finish(true);
        }
    }

    public void StateEmoteEvent(string keyName) {
        switch (keyName) {
            case "end":
                if (emoteLimitTick != 0) {
                    if (emoteActionTask is NpcEmoteTask emoteTask) {
                        actor.AnimationState.TryPlaySequence(emoteTask.Sequence, 1, AnimationType.Misc);
                    }
                    return;
                }

                emoteActionTask?.Completed();

                Idle();

                break;
            default:
                break;
        }
    }
}
