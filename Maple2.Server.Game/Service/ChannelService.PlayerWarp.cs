using Grpc.Core;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<PlayerWarpResponse> PlayerWarp(PlayerWarpRequest request, ServerCallContext context) {
        switch (request.RequestCase) {
            case PlayerWarpRequest.RequestOneofCase.GoToPlayer:
                return Task.FromResult(GoToPlayer(request.GoToPlayer));
            default:
                return Task.FromResult(new PlayerWarpResponse());
        }
    }

    private PlayerWarpResponse GoToPlayer(PlayerWarpRequest.Types.GoToPlayer gotoPlayer) {

        if (!server.GetSession(gotoPlayer.CharacterId, out GameSession? session)) {
            return new PlayerWarpResponse {
                Error = -1,
            };
        }

        if (session.Field is null)
            return new PlayerWarpResponse() {
                Error = -1,
            };

        return new PlayerWarpResponse {
            RoomId = session.Field.RoomId,
            X = session.Player.Position.X,
            Y = session.Player.Position.Y,
            Z = session.Player.Position.Z,
        };
    }
}

