using System;
using Grpc.Core;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Login.Session;
using Maple2.Server.World.Service;
using static Maple2.Model.Error.MigrationError;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

namespace Maple2.Server.Login.PacketHandlers;

public class ResponseKeyHandler : PacketHandler<LoginSession> {
    public override RecvOp OpCode => RecvOp.ResponseKey;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required WorldClient World { private get; init; }
    // ReSharper restore All
    #endregion

    public ResponseKeyHandler() { }

    public override void Handle(LoginSession session, IByteReader packet) {
        long accountId = packet.ReadLong();
        ulong token = packet.Read<ulong>();
        var machineId = packet.Read<Guid>();

        try {
            var request = new MigrateInRequest {
                AccountId = accountId,
                Token = token,
                MachineId = machineId.ToString(),
            };

            MigrateInResponse response = World.MigrateIn(request);
            session.Init(accountId, machineId);
            session.Send(MigrationPacket.MoveResult(ok));
        } catch (Exception ex) when (ex is RpcException or InvalidOperationException) {
            session.Send(MigrationPacket.MoveResult(s_move_err_default));
            session.Disconnect();
        }
    }
}
