using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Maple2.Model.Game;
using Maple2.Model.Game.Party;
using Maple2.Server.World.Containers;
using ChannelClient = Maple2.Server.Channel.Service.Channel.ChannelClient;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<ChatResponse> Chat(ChatRequest request, ServerCallContext context) {
        switch (request.ChatCase) {
            case ChatRequest.ChatOneofCase.Whisper:
                return WhisperChat(request);
            case ChatRequest.ChatOneofCase.Party:
                return PartyChat(request);
            case ChatRequest.ChatOneofCase.Guild:
                return GuildChat(request);
            case ChatRequest.ChatOneofCase.World:
                return WorldChat(request);
            case ChatRequest.ChatOneofCase.Super:
                return SuperChat(request);
            case ChatRequest.ChatOneofCase.Club:
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.Wedding:
                return Task.FromResult(new ChatResponse());
            default:
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, $"Invalid chat type: {request.ChatCase}"));
        }
    }

    private Task<ChatResponse> WhisperChat(ChatRequest request) {
        if (!playerLookup.TryGet(request.CharacterId, out PlayerInfo? info)) {
            throw new RpcException(new Status(StatusCode.NotFound,
                $"Unable to whisper: {request.Whisper.RecipientName}"));
        }

        int channel = info.Channel;
        if (!channelClients.TryGetClient(channel, out ChannelClient? channelClient)) {
            logger.Error("No registry for channel: {Channel}", channel);
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                $"Unable to whisper: {request.Whisper.RecipientName} on channel: {channel}"));
        }

        var channelChat = new ChatRequest {
            AccountId = request.AccountId,
            CharacterId = request.CharacterId,
            Name = request.Name,
            Message = request.Message,
            ItemUids = { request.ItemUids },
            Whisper = new ChatRequest.Types.Whisper {
                RecipientId = request.Whisper.RecipientId,
                RecipientName = request.Whisper.RecipientName,
            },
        };

        try {
            return Task.FromResult(channelClient.Chat(channelChat));
        } catch (RpcException ex) when (ex.StatusCode is StatusCode.NotFound) {
            logger.Information("{CharacterId} not found...", request.CharacterId);
            return Task.FromResult(new ChatResponse());
        }
    }

    private Task<ChatResponse> PartyChat(ChatRequest request) {
        if (!partyLookup.TryGet(request.Party.PartyId, out PartyManager? info)) {
            logger.Information("Party {PartyPartyId} not found...", request.Party.PartyId);
            return Task.FromResult(new ChatResponse());
        }

        foreach (IGrouping<short, PartyMember> group in info.Party.Members.Values.GroupBy(member => member.Info.Channel)) {
            if (!channelClients.TryGetClient(group.Key, out ChannelClient? client)) {
                continue;
            }

            request.Party.MemberIds.Clear();
            request.Party.MemberIds.AddRange(group.Select(member => member.Info.CharacterId));

            try {
                client.Chat(request);
            } catch { /* ignored */ }
        }

        return Task.FromResult(new ChatResponse());
    }

    private Task<ChatResponse> GuildChat(ChatRequest request) {
        if (!guildLookup.TryGet(request.Guild.GuildId, out GuildManager? info)) {
            logger.Information("Guild {GuildGuildId} not found...", request.Guild.GuildId);
            return Task.FromResult(new ChatResponse());
        }

        foreach (IGrouping<short, GuildMember> group in info.Guild.Members.Values.GroupBy(member => member.Info.Channel)) {
            if (!channelClients.TryGetClient(group.Key, out ChannelClient? client)) {
                continue;
            }

            request.Guild.MemberIds.Clear();
            request.Guild.MemberIds.AddRange(group.Select(member => member.Info.CharacterId));

            try {
                client.Chat(request);
            } catch { /* ignored */ }
        }

        return Task.FromResult(new ChatResponse());
    }

    private Task<ChatResponse> WorldChat(ChatRequest request) {
        foreach ((int channel, ChannelClient client) in channelClients) {
            client.Chat(request);
        }
        return Task.FromResult(new ChatResponse());
    }

    private Task<ChatResponse> SuperChat(ChatRequest request) {
        foreach ((int channel, ChannelClient client) in channelClients) {
            client.Chat(request);
        }
        return Task.FromResult(new ChatResponse());
    }
}
