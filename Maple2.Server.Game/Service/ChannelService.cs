using Maple2.Server.Game.Util.Sync;
using Serilog;

namespace Maple2.Server.Game.Service;

public partial class ChannelService(GameServer server, PlayerInfoStorage playerInfos) : Channel.Service.Channel.ChannelBase {

    private readonly ILogger logger = Log.Logger.ForContext<ChannelService>();

}
