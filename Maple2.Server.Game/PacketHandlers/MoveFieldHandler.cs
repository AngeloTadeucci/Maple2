using System.Net;
using Grpc.Core;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.World.Service;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

namespace Maple2.Server.Game.PacketHandlers;

public class MoveFieldHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.RequestMoveField;

    private enum Command : byte {
        Portal = 0,
        LeaveDungeon = 1,
        VisitHome = 2,
        Return = 3,
        DecorPlanner = 4,
        BlueprintDesigner = 5,
        ModelHome = 6,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required MapMetadataStorage MapMetadata { private get; init; }
    public required GameStorage GameStorage { private get; init; }
    public required WorldClient World { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();

        switch (command) {
            case Command.Portal:
                HandlePortal(session, packet);
                return;
            case Command.VisitHome: // s_action_privilege_portal
                HandleVisitHome(session, packet);
                return;
            case Command.LeaveDungeon:
            case Command.Return:
                HandleReturn(session);
                return;
            case Command.DecorPlanner: // s_tutorial_designHome_limit
                HandleDecorPlanner(session);
                return;
            case Command.BlueprintDesigner: // s_tutorial_blueprint_limit
                HandleBlueprintDesigner(session);
                return;
            case Command.ModelHome: // s_meratmarket_ask_move_to_modelhouse
                HandleModelHome(session);
                return;
        }
    }

    private void HandlePortal(GameSession session, IByteReader packet) {
        if (session.Field == null) return;

        int mapId = packet.ReadInt();
        if (mapId != session.Field.MapId) {
            return;
        }

        int portalId = packet.ReadInt();
        packet.ReadInt();
        packet.ReadInt();
        packet.ReadUnicodeString();
        string password = packet.ReadUnicodeString(); // Password for locked portals

        session.Field.UsePortal(session, portalId, password);
    }

    private void HandleVisitHome(GameSession session, IByteReader packet) {
        packet.ReadInt();
        packet.ReadInt();
        packet.ReadInt();
        long accountId = packet.ReadLong();
        string passcode = packet.ReadUnicodeString();

        using GameStorage.Request db = session.GameStorage.Context();
        Home? home = db.GetHome(accountId);
        if (home == null) {
            session.Send(FieldEnterPacket.Error(MigrationError.s_move_err_no_server));
            return;
        }

        if (home.Passcode != null && home.Passcode != passcode) {
            session.Send(NoticePacket.MessageBox(StringCode.s_home_password_mismatch));
            return;
        }

        try {
            var request = new MigrateOutRequest {
                AccountId = session.AccountId,
                CharacterId = session.CharacterId,
                MachineId = session.MachineId.ToString(),
                Server = Server.World.Service.Server.Game,
                MapId = home.Indoor.MapId,
                OwnerId = home.Indoor.OwnerId,
            };

            MigrateOutResponse response = World.MigrateOut(request);
            var endpoint = new IPEndPoint(IPAddress.Parse(response.IpAddress), response.Port);
            session.Send(MigrationPacket.GameToGame(endpoint, response.Token, home.Indoor.MapId));
            session.State = SessionState.ChangeMap;
        } catch (RpcException ex) {
            session.Send(MigrationPacket.GameToGameError(MigrationError.s_move_err_default));
            session.Send(NoticePacket.Disconnect(new InterfaceText(ex.Message)));
        } finally {
            session.Disconnect();
        }
    }

    private void HandleReturn(GameSession session) {
        session.ReturnField();
    }

    private void HandleDecorPlanner(GameSession session) {
        Home home = session.Player.Value.Home;
        if (!home.IsHomeSetup) {
            return;
        }

        session.EnterDecorPlanner();
    }

    private void HandleBlueprintDesigner(GameSession session) {

    }

    private void HandleModelHome(GameSession session) {

    }
}
