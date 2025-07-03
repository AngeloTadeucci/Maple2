using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class NewsNotificationHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.NewsNotification;

    private enum Command : byte {
        RequestToken = 0,
        OpenUi = 1,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        if (packet.ReadByte() != 0) {
            return;
        }

        var command = packet.Read<Command>();
        switch (command) {
            case Command.RequestToken:
                HandleRequestToken(session, packet);
                return;
            case Command.OpenUi:
                HandleOpenUi(session, packet);
                return;
        }
    }

    private static void HandleRequestToken(GameSession session, IByteReader packet) {
        packet.ReadByte(); // Always 0?
    }

    private static void HandleOpenUi(GameSession session, IByteReader packet) {
        int type = packet.ReadInt();
        session.Send(NewsNotificationPacket.OpenUi(type));
    }
}
