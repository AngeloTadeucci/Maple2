using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;

namespace Maple2.Server.Game.Model.State;

public sealed class StateHitNpc : NpcState {
    public readonly float UnknownF1;
    public readonly float UnknownF2;
    public readonly float UnknownF3;
    public readonly byte UnknownB;

    public StateHitNpc(float unknownF1, float unknownF2, float unknownF3, byte unknownB) {
        State = ActorState.Hit;
        SubState = ActorSubState.None;

        UnknownF1 = unknownF1;
        UnknownF2 = unknownF2;
        UnknownF3 = unknownF3;
        UnknownB = unknownB;
    }

    public override void WriteTo(IByteWriter writer) {
        writer.WriteFloat(UnknownF1);
        writer.WriteFloat(UnknownF2);
        writer.WriteFloat(UnknownF3);
        writer.WriteByte(UnknownB);
    }
}
