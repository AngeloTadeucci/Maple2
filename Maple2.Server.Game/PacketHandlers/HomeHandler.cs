using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class HomeHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.RequestHome;

    private enum Command : byte {
        Accept = 0,
        Invite = 1,
        Warp = 3,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Accept:
                HandleAccept(session, packet);
                return;
            case Command.Invite:
                HandleInvite(session, packet);
                return;
            case Command.Warp:
                HandleWarp(session, packet);
                return;
        }
    }

    private void HandleAccept(GameSession session, IByteReader packet) {
        string name = packet.ReadUnicodeString();
        byte unknown = packet.ReadByte(); // 1,5
    }

    private void HandleInvite(GameSession session, IByteReader packet) {
        string name = packet.ReadUnicodeString();
    }

    private void HandleWarp(GameSession session, IByteReader packet) {
        if (session.Field?.Metadata.Property.HomeReturnable != true) {
            session.Send(NoticePacket.MessageBox(StringCode.s_home_returnable_forbidden));
            return;
        }

        Maple2.Model.Game.Home home = session.Player.Value.Home;
        int homeMapId = home.Indoor.MapId;
        if (session.Field.MapId == homeMapId) {
            session.Send(NoticePacket.MessageBox(StringCode.s_home_returnable_forbidden_to_sameplace));
            return;
        }

        int type = packet.ReadInt(); // -1 = none

        if (string.IsNullOrEmpty(home.Indoor.Name)) {
            // new home
            session.Player.Value.Home.InitNewHome(session.Player.Value.Character.Name, 0);
            session.Housing.SaveHome();
            session.Housing.SavePlots();
        }

        long ownerId = home.Indoor.OwnerId;
        session.Send(session.PrepareField(homeMapId, ownerId: ownerId)
            ? FieldEnterPacket.Request(session.Player)
            : FieldEnterPacket.Error(MigrationError.s_move_err_default));
    }
}
