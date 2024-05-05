using System.Numerics;
using Maple2.Server.Game.Model.State;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Model.Routine;

public class JumpRoutine : NpcRoutine {
    private readonly Vector3 endPosition;

    public static JumpRoutine DistanceA(FieldNpc npc, int distance, float duration, float scale) {
        Vector3 endPosition = npc.Position.Offset(distance, npc.Rotation);
        return new JumpRoutine(npc, -2, endPosition, duration, scale);
    }

    public static JumpRoutine PositionA(FieldNpc npc, Vector3 endPosition, float duration) {
        return new JumpRoutine(npc, -2, endPosition, duration, 0.3f);
    }

    public static JumpRoutine InPlaceA(FieldNpc npc, float duration) {
        return new JumpRoutine(npc, -2, duration);
    }

    public static JumpRoutine DistanceB(FieldNpc npc, int distance, float duration, float scale) {
        Vector3 endPosition = npc.Position.Offset(distance, npc.Rotation);
        return new JumpRoutine(npc, -3, endPosition, duration, scale);
    }

    public static JumpRoutine PositionB(FieldNpc npc, Vector3 endPosition, float duration) {
        return new JumpRoutine(npc, -3, endPosition, duration, 0f);
    }

    public static JumpRoutine InPlaceB(FieldNpc npc, float duration) {
        return new JumpRoutine(npc, -3, duration);
    }

    private TimeSpan duration;

    private JumpRoutine(FieldNpc npc, short sequenceId, Vector3 endPosition, float duration, float scale) : base(npc, sequenceId) {
        this.endPosition = endPosition;
        this.duration = TimeSpan.FromSeconds(duration);
        Npc.State = new StateJumpNpc(Npc.Position, endPosition, duration, scale);

        NextRoutine = () => new WaitRoutine(npc, npc.IdleSequence.Id, npc.IdleSequence.Time);
    }

    private JumpRoutine(FieldNpc npc, short sequenceId, float duration) : base(npc, sequenceId) {
        endPosition = Npc.Position;
        this.duration = TimeSpan.FromSeconds(duration);
        Npc.State = new StateJumpNpc(Npc.Position);

        NextRoutine = () => new WaitRoutine(npc, npc.IdleSequence.Id, npc.IdleSequence.Time);
    }

    public override Result Update(TimeSpan elapsed) {
        duration -= elapsed;
        if (duration.Ticks > 0) {
            return Result.InProgress;
        }

        OnCompleted();
        return Result.Success;
    }

    public override void OnCompleted() {
        if (Completed) {
            return;
        }

        base.OnCompleted();
        // Npc.Position = Npc.Field.Navigation.UpdateAgent(Npc.Agent, endPosition);
        Npc.Velocity = default;
    }
}
