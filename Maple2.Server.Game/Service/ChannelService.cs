using Maple2.Server.Game.Util.Sync;
using Serilog;

namespace Maple2.Server.Game.Service;

public partial class ChannelService : Channel.Service.Channel.ChannelBase {
    private readonly GameServer server;
    private readonly PlayerInfoStorage playerInfos;

    private readonly ILogger logger = Log.Logger.ForContext<ChannelService>();

    public ChannelService(GameServer server, PlayerInfoStorage playerInfos) {
        this.server = server;
        this.playerInfos = playerInfos;
    }
}
