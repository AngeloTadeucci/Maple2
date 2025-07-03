using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class AttributePointHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.AttributePoint;

    private enum Command : byte {
        Increment = 2,
        Reset = 3,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Increment:
                HandleIncrement(session, packet);
                return;
            case Command.Reset:
                HandleReset(session);
                return;
        }
    }

    private void HandleIncrement(GameSession session, IByteReader packet) {
        var type = packet.Read<BasicAttribute>();
        session.Config.AllocateStatPoint(type);
    }

    private void HandleReset(GameSession session) {
        session.Config.ResetStatPoints();
    }
}
