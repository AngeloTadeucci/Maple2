using Grpc.Core;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Session;
using Serilog;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<HeartbeatResponse> Heartbeat(HeartbeatRequest request, ServerCallContext context) {
        if (request.CharacterId == 0) {
            logger.Warning("Heartbeat from unknown session. No character id.");
            return Task.FromResult(new HeartbeatResponse {
                Success = false,
            });
        }
        if (!server.GetSession(request.CharacterId, out GameSession? session)) {
            logger.Warning("Heartbeat from unknown session: {CharacterId}", request.CharacterId);
            return Task.FromResult(new HeartbeatResponse {
                Success = false,
            });
        }

        session.Send(RequestPacket.Heartbeat());
        return Task.FromResult(new HeartbeatResponse {
            Success = true,
        });
    }
}
