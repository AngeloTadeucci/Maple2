namespace Maple2.Server.Game.Model.Routine;

public class WaitRoutine(FieldNpc npc, short sequenceId, float duration) : NpcRoutine(npc, sequenceId) {
    private TimeSpan duration = TimeSpan.FromSeconds(duration);

    public override Result Update(TimeSpan elapsed) {
        duration -= elapsed;
        if (duration.Ticks > 0) {
            return Result.InProgress;
        }

        OnCompleted();
        return Result.Success;
    }
}
