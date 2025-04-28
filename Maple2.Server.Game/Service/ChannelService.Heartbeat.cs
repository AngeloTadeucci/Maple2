using Grpc.Core;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<HeartbeatResponse> Heartbeat(HeartbeatRequest request, ServerCallContext context) {
        if (request.CharacterId == 0) {
            throw new RpcException(new Status(StatusCode.NotFound, "Character ID is 0."));
        }
        if (!server.GetSession(request.CharacterId, out GameSession? session)) {
            throw new RpcException(new Status(StatusCode.NotFound, "Session not found."));
        }

        session.Send(RequestPacket.Heartbeat());
        return Task.FromResult(new HeartbeatResponse {
            Success = true,
        });
    }
}
