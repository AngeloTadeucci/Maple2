using Grpc.Core;
using Maple2.Model.Game;
using ChannelClient = Maple2.Server.Channel.Service.Channel.ChannelClient;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<BuddyResponse> Buddy(BuddyRequest request, ServerCallContext context) {
        if (!playerLookup.TryGet(request.ReceiverId, out PlayerInfo? info)) {
            return Task.FromResult(new BuddyResponse());
        }

        int channel = info.Channel;
        if (!channelClients.TryGetClient(channel, out ChannelClient? channelClient)) {
            logger.Error("No registry for channel: {Channel}", channel);
            return Task.FromResult(new BuddyResponse());
        }

        try {
            return Task.FromResult(channelClient.Buddy(request));
        } catch (RpcException ex) when (ex.StatusCode is StatusCode.NotFound) {
            logger.Information("{CharacterId} not found...", request.ReceiverId);
            return Task.FromResult(new BuddyResponse());
        }
    }
}
