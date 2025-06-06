using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class LiftableHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Liftable;

    private enum Command : byte {
        Pickup = 1,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Pickup:
                HandlePickup(session, packet);
                return;
        }
    }

    private void HandlePickup(GameSession session, IByteReader packet) {
        string entityId = packet.ReadString();

        if (session.Field == null || session.HeldCube != null) {
            return;
        }
        if (!session.Field.TryGetLiftable(entityId, out FieldLiftable? liftable)) {
            return;
        }

        session.HeldCube = liftable.Pickup();
        if (session.HeldCube == null) {
            return;
        }

        session.Field.Broadcast(SetCraftModePacket.Liftable(session.Player.ObjectId, session.HeldCube));
    }
}
