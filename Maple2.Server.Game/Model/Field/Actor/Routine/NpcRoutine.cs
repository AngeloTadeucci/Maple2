using Maple2.Model.Enum;
using Maple2.Server.Game.Model.State;

namespace Maple2.Server.Game.Model.Routine;

public abstract class NpcRoutine {
    public enum Result { Unknown, InProgress, Success, Failure }

    public bool Completed { get; private set; }
    protected FieldNpc Npc;

    // Used for chaining predetermined routines.
    public Func<NpcRoutine>? NextRoutine { get; init; }

    protected NpcRoutine(FieldNpc npc, short sequenceId) {
        Npc = npc;

        Npc.SequenceId = sequenceId;
    }

    /// <summary>
    /// Synchronizes an NpcRoutine.
    /// </summary>
    /// <returns>bool representing whether this routine is completed</returns>
    public abstract Result Update(TimeSpan elapsed);

    public virtual void OnCompleted() {
        if (Completed) {
            return;
        }

        Completed = true;
        // Force Idle on completion
        Npc.SequenceId = Npc.IdleSequence.Id;
        if (Npc.State.State != ActorState.Idle) {
            Npc.State = new NpcState();
        }
    }
}
