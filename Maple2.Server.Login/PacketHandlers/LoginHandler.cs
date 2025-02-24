using System;
using System.Linq;
using Grpc.Core;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Global.Service;
using Maple2.Server.Login.Session;
using GlobalClient = Maple2.Server.Global.Service.Global.GlobalClient;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

namespace Maple2.Server.Login.PacketHandlers;

public class LoginHandler : PacketHandler<LoginSession> {
    public override RecvOp OpCode => RecvOp.ResponseLogin;

    private enum Command : byte {
        ServerList = 1,
        CharacterList = 2,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required GlobalClient Global { private get; init; }
    public required WorldClient World { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(LoginSession session, IByteReader packet) {
        var command = packet.Read<Command>();

        try {
            switch (command) {
                case Command.ServerList:
                    session.ListServers();
                    return;
                case Command.CharacterList:
                    string user = packet.ReadUnicodeString();
                    string pass = packet.ReadUnicodeString();
                    packet.ReadShort(); // 1
                    var machineId = packet.Read<Guid>();

                    Logger.Debug("Logging in with user:{User}", user);
                    LoginResponse response = Global.Login(new LoginRequest {
                        Username = user,
                        Password = pass,
                        MachineId = machineId.ToString(),
                    });

                    if (response.Code != LoginResponse.Types.Code.Ok) {
                        session.Send(LoginResultPacket.Error((byte) response.Code, response.Message, response.AccountId));
                        session.Disconnect();
                        return;
                    }

                    // TODO: Account is already logged into game server.
                    // if (World.PlayerInfo(new PlayerInfoRequest {AccountId = response.AccountId}).Location.Channel > 0) {
                    //     session.Send(LoginResultPacket.Error((byte) LoginResponse.Types.Code.AlreadyLogin, "", response.AccountId));
                    //     return;
                    // }

                    session.Init(response.AccountId, machineId);

                    session.Send(LoginResultPacket.Success(response.AccountId));
                    session.Send(UgcPacket.SetEndpoint(Target.WebUri));

                    session.ListCharacters();
                    session.Send(GameEventPacket.Load(session.Server.GetEvents().ToArray()));
                    return;
                default:
                    Logger.Error("Invalid type: {Type}", command);
                    break;
            }
        } catch (RpcException ex) {
            Logger.Error(ex, "Failed to login");
        }

        // Disconnect by default if anything goes wrong.
        session.Disconnect();
    }
}
