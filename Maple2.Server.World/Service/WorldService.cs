using System.Threading.Tasks;
using Grpc.Core;
using Maple2.Server.World.Containers;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Maple2.Server.World.Service;

public partial class WorldService(IMemoryCache tokenCache, ChannelClientLookup channelClients, PlayerInfoLookup playerLookup, GuildLookup guildLookup, PartyLookup partyLookup, GroupChatLookup groupChatLookup)
    : World.WorldBase {
    private readonly ILogger logger = Log.Logger.ForContext<WorldService>();

    public override Task<ChannelsResponse> Channels(ChannelsRequest request, ServerCallContext context) {
        var response = new ChannelsResponse();
        response.Channels.AddRange(channelClients.Keys);
        return Task.FromResult(response);
    }
}
