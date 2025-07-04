using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Server.Game.Model.State;

public class NpcState : IByteSerializable {
    public ActorState State { get; init; } = ActorState.Idle;
    public virtual ActorSubState SubState { get; init; } = ActorSubState.Idle_Idle;

    public virtual void WriteTo(IByteWriter writer) { }
}
