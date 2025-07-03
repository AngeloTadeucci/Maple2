using Grpc.Core;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<PlayerWarpResponse> PlayerWarp(PlayerWarpRequest request, ServerCallContext context) {
        switch (request.RequestCase) {
            case PlayerWarpRequest.RequestOneofCase.GoToPlayer:
                return Task.FromResult(GoToPlayer(request, request.RequesterId));
            default:
                return Task.FromResult(new PlayerWarpResponse());
        }
    }

    private PlayerWarpResponse GoToPlayer(PlayerWarpRequest request, long requesterId) {
        if (request.GoToPlayer.CharacterId == 0) {
            return new PlayerWarpResponse {
                Error = -1,
            };
        }

        if (channelClients.TryGetClient(request.GoToPlayer.Channel, out Channel.Service.Channel.ChannelClient? channelClient)) {
            return channelClient.PlayerWarp(request);
        }

        return new PlayerWarpResponse {
            Error = -1,
        };
    }
}
