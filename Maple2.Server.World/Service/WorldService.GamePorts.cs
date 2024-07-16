
using Grpc.Core;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<PortResponse> Port(PortRequest request, ServerCallContext context) {
        (ushort gamePort, int grpcPort, int channel) = channelClients.FindOrCreateChannelByIp(request.GameIp);

        return Task.FromResult(new PortResponse {
            GamePort = gamePort,
            GrpcPort = grpcPort,
            GameChannel = channel
        });
    }
}