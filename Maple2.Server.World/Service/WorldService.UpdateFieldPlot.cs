using Grpc.Core;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<FieldPlotResponse> UpdateFieldPlot(FieldPlotRequest request, ServerCallContext context) {
        if (request.MapId == -1) {
            worldServer.FieldPlotExpiryCheck();
            return Task.FromResult(new FieldPlotResponse());
        }

        if (request.MapId <= 0) {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid map ID"));
        }

        foreach ((int, Channel.Service.Channel.ChannelClient) channel in channelClients) {
            channel.Item2.UpdateFieldPlot(request);
        }

        return Task.FromResult(new FieldPlotResponse());
    }
}
