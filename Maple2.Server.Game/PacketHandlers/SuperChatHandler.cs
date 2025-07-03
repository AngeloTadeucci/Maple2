using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class SuperChatHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.SuperWorldChat;

    private enum Command : byte {
        Select = 0,
        Deselect = 1,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Select:
                HandleSelect(session, packet);
                return;
            case Command.Deselect:
                HandleDeselect(session);
                return;
        }
    }

    private static void HandleSelect(GameSession session, IByteReader packet) {
        int itemId = packet.ReadInt();

        Item? superChatItem = session.Item.Inventory.Find(itemId).FirstOrDefault();
        if (superChatItem?.Metadata.Function?.Type != ItemFunction.SuperWorldChat ||
            !int.TryParse(superChatItem.Metadata.Function?.Parameters.Split(",").First(), out int superChatId)) {
            return;
        }

        session.SuperChatId = superChatId;
        session.SuperChatItemId = itemId;
        session.Send(SuperChatPacket.Select(session.Player.ObjectId, itemId));
    }

    private static void HandleDeselect(GameSession session) {
        session.SuperChatId = 0;
        session.SuperChatItemId = 0;
        session.Send(SuperChatPacket.Deselect(session.Player.ObjectId));
    }
}
