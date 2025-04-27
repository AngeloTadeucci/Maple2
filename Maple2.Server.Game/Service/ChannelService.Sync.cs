using Grpc.Core;
using Maple2.Model.Game;
using Maple2.Server.Channel.Service;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<MailNotificationResponse> MailNotification(MailNotificationRequest request, ServerCallContext context) {
        if (!server.GetSession(request.CharacterId, out GameSession? session)) {
            throw new RpcException(new Status(StatusCode.NotFound, $"Unable to find: {request.CharacterId}"));
        }

        session.Mail.Notify(true);

        return Task.FromResult(new MailNotificationResponse {
            Delivered = true,
        });
    }

    public override Task<PlayerUpdateResponse> UpdatePlayer(PlayerUpdateRequest request, ServerCallContext context) {
        playerInfos.ReceiveUpdate(request);
        return Task.FromResult(new PlayerUpdateResponse());
    }

    public override Task<DisconnectResponse> Disconnect(DisconnectRequest request, ServerCallContext context) {
        if (request is { CharacterId: <= 0 }) {
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"CharacterId not specified"));
        }

        if (!server.GetSession(request.CharacterId, out GameSession? session)) {
            throw new RpcException(new Status(StatusCode.NotFound, $"Unable to find: {request.CharacterId}"));
        }

        session.Disconnect();
        return Task.FromResult(new DisconnectResponse {
            Success = true,
        });
    }
}
