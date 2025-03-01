
using Grpc.Core;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<AddChannelResponse> AddChannel(AddChannelRequest request, ServerCallContext context) {
        (ushort gamePort, int grpcPort, int channel) = channelClients.FindOrCreateChannelByIp(request.GameIp, request.GrpcGameIp, request.InstancedContent);

        return Task.FromResult(new AddChannelResponse {
            GamePort = gamePort,
            GrpcPort = grpcPort,
            GameChannel = channel,
        });
    }
}
