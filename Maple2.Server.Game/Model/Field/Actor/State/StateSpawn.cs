using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;

namespace Maple2.Server.Game.Model.State;

public sealed class StateSpawn : NpcState {
    public readonly int Unknown;

    public StateSpawn(int unknown) {
        State = ActorState.Spawn;
        SubState = ActorSubState.Idle_Idle;

        Unknown = unknown;
    }

    public override void WriteTo(IByteWriter writer) {
        writer.WriteInt(Unknown);
    }
}
