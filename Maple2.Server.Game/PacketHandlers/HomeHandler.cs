using System.Net;
using Grpc.Core;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.World.Service;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;


namespace Maple2.Server.Game.PacketHandlers;

public class HomeHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.RequestHome;

    private enum Command : byte {
        Accept = 0,
        Invite = 1,
        Warp = 3,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required MapMetadataStorage MapMetadataStorage { private get; init; }
    public required WorldClient World { private get; init; }

    // ReSharper restore All
    #endregion

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

        Home home = session.Player.Value.Home;
        int homeMapId = home.Indoor.MapId;
        if (session.Field.MapId == homeMapId) {
            session.Send(NoticePacket.MessageBox(StringCode.s_home_returnable_forbidden_to_sameplace));
            return;
        }

        int templateId = packet.ReadInt(); // -1 = none

        if (templateId < -1 && templateId > 2) {
            return;
        }

        // Template ids in XML are 1-based, but the client uses 0-based
        templateId++;

        MapMetadataStorage.TryGetExportedUgc(templateId.ToString(), out ExportedUgcMapMetadata? exportedUgcMap);

        if (string.IsNullOrEmpty(home.Indoor.Name)) {
            session.Housing.InitNewHome(session.Player.Value.Character.Name, exportedUgcMap);
        }

        try {
            var request = new MigrateOutRequest {
                AccountId = session.AccountId,
                CharacterId = session.CharacterId,
                MachineId = session.MachineId.ToString(),
                Server = Server.World.Service.Server.Game,
                MapId = homeMapId,
                OwnerId = home.Indoor.OwnerId,
            };

            MigrateOutResponse response = World.MigrateOut(request);
            var endpoint = new IPEndPoint(IPAddress.Parse(response.IpAddress), response.Port);
            session.Send(MigrationPacket.GameToGame(endpoint, response.Token, homeMapId));
            session.State = SessionState.ChangeMap;
        } catch (RpcException ex) {
            session.Send(MigrationPacket.GameToGameError(MigrationError.s_move_err_default));
            session.Send(NoticePacket.Disconnect(new InterfaceText(ex.Message)));
        } finally {
            session.Disconnect();
        }
    }
}
