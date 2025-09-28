using System.Net;
using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.World.Service;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

namespace Maple2.Server.Game.PacketHandlers;

public class GlobalPortal : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.GlobalPortal;

    private enum Command : byte {
        Enter = 2,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required WorldClient World { private get; init; }

    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Enter:
                HandleEnter(session, packet);
                return;
        }
    }

    private void HandleEnter(GameSession session, IByteReader packet) {
        int eventId = packet.ReadInt();
        int index = packet.ReadInt();

        TimeEventResponse eventResponse = World.TimeEvent(new TimeEventRequest {
            JoinGlobalPortal = new TimeEventRequest.Types.JoinGlobalPortal {
                EventId = eventId,
                Index = index,
            },
        });

        if (eventResponse.Error != 0 || eventResponse.GlobalPortalInfo == null) {
            return;
        }

        GlobalPortalInfo portal = eventResponse.GlobalPortalInfo;

        session.MigrationSave();
        try {
            var request = new MigrateOutRequest {
                AccountId = session.AccountId,
                CharacterId = session.CharacterId,
                MachineId = session.MachineId.ToString(),
                Channel = portal.Channel,
                Server = Server.World.Service.Server.Game,
                RoomId = portal.RoomId,
                MapId = portal.MapId,
                PortalId = portal.PortalId,
            };

            MigrateOutResponse response = World.MigrateOut(request);
            var endpoint = new IPEndPoint(IPAddress.Parse(response.IpAddress), response.Port);
            session.Send(MigrationPacket.GameToGame(endpoint, response.Token, eventResponse.GlobalPortalInfo.MapId));
            session.State = SessionState.ChangeMap;
        } catch (RpcException ex) {
            session.Send(MigrationPacket.GameToGameError(MigrationError.s_move_err_default));
            session.Send(NoticePacket.Disconnect(new InterfaceText(ex.Message)));
        } finally {
            session.Disconnect();
        }
    }
}
