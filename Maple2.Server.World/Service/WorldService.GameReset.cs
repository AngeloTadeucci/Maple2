using ChannelClient = Maple2.Server.Channel.Service.Channel.ChannelClient;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public Task<GameResetResponse> GameReset(GameResetRequest request) {
        switch (request.ResetCase) {
            case GameResetRequest.ResetOneofCase.Daily:
                return Task.FromResult(Daily());
            default:
                return Task.FromResult(new GameResetResponse());
        }
    }

    private GameResetResponse Daily() {
        foreach ((int channel, ChannelClient client) in channelClients) {
            client.GameReset(new GameResetRequest {
                Daily = new GameResetRequest.Types.Daily(),
            });
        }
        return new GameResetResponse();
    }
}
