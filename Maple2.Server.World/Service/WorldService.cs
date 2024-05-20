using System.Threading.Tasks;
using Grpc.Core;
using Maple2.Server.World.Containers;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Maple2.Server.World.Service;

public partial class WorldService : World.WorldBase {
    private readonly ChannelClientLookup channelClients;
    private readonly PlayerInfoLookup playerLookup;
    private readonly GuildLookup guildLookup;
    private readonly PartyLookup partyLookup;
    private readonly PartySearchLookup partySearchLookup;
    private readonly GroupChatLookup groupChatLookup;
    private readonly BlackMarketLookup blackMarketLookup;
    private readonly ILogger logger = Log.Logger.ForContext<WorldService>();

    public WorldService(IMemoryCache tokenCache, ChannelClientLookup channelClients, PlayerInfoLookup playerLookup, GuildLookup guildLookup, PartyLookup partyLookup,
                        PartySearchLookup partySearchLookup, GroupChatLookup groupChatLookup, BlackMarketLookup blackMarketLookup) {
        this.tokenCache = tokenCache;
        this.channelClients = channelClients;
        this.playerLookup = playerLookup;
        this.guildLookup = guildLookup;
        this.partyLookup = partyLookup;
        this.partySearchLookup = partySearchLookup;
        this.groupChatLookup = groupChatLookup;
        this.blackMarketLookup = blackMarketLookup;
    }

    public override Task<ChannelsResponse> Channels(ChannelsRequest request, ServerCallContext context) {
        var response = new ChannelsResponse();
        response.Channels.AddRange(channelClients.Keys);
        return Task.FromResult(response);
    }
}
