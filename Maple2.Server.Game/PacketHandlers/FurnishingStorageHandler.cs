using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class FurnishingStorageHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.FurnishingStorage;

    private enum Command : byte {
        Delete = 6,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Delete:
                HandleDelete(session, packet);
                return;
        }
    }

    private static void HandleDelete(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        session.Item.Furnishing.RemoveItem(itemUid);
    }
}
