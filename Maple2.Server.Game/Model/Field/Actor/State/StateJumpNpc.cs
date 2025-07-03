using System.Numerics;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;

namespace Maple2.Server.Game.Model.State;

public sealed class StateJumpNpc : NpcState {
    public readonly bool IsAbsolute;
    public readonly Vector3 StartPosition;
    public readonly Vector3 EndPosition;
    public readonly float Angle;
    public readonly float Scale;

    public override ActorSubState SubState {
        get => base.SubState;
        init {
            base.SubState = value;
            if (base.SubState.State() is not (ActorState.Jump or ActorState.JumpTo)) {
                base.SubState = ActorSubState.Jump_Jump;
            }
            State = base.SubState.State();
        }
    }

    public StateJumpNpc(in Vector3 startPosition, in Vector3 endPosition, float angle, float scale) {
        State = ActorState.Jump;
        SubState = ActorSubState.Jump_Jump; // Not sure, seems to be None

        IsAbsolute = true;
        StartPosition = startPosition;
        EndPosition = endPosition;
        Angle = angle;
        Scale = scale;
    }

    public StateJumpNpc(in Vector3 endPosition) {
        State = ActorState.Jump;
        SubState = ActorSubState.Jump_Jump; // Not sure, seems to be None

        EndPosition = endPosition;
        Angle = 0.45f;
        Scale = 1f;
    }

    public override void WriteTo(IByteWriter writer) {
        writer.WriteBool(IsAbsolute);
        if (IsAbsolute) {
            writer.Write<Vector3>(StartPosition);
            writer.Write<Vector3>(EndPosition);
            writer.WriteFloat(Angle);
            writer.WriteFloat(Scale);
        } else {
            writer.Write<Vector3>(EndPosition);
        }
    }
}
