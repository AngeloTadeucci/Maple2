using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class StateHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.State;

    private enum Command : byte {
        Jump = 0,
        Land = 1,
    }
    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Jump:
                HandleJump(session);
                break;
        }
    }

    private static void HandleJump(GameSession session) {
        session.ConditionUpdate(ConditionType.jump);
    }
}
