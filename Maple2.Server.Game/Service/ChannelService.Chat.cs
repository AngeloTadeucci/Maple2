using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<ChatResponse> Chat(ChatRequest request, ServerCallContext context) {
        switch (request.ChatCase) {
            case ChatRequest.ChatOneofCase.Whisper:
                WhisperChat(request);
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.Party:
                PartyChat(request);
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.Guild:
                GuildChat(request);
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.World:
                WorldChat(request);
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.Super:
                SuperChat(request);
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.Club:
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.Wedding:
                return Task.FromResult(new ChatResponse());
            default:
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, $"Invalid chat type: {request.ChatCase}"));
        }
    }

    private void WhisperChat(ChatRequest request) {
        if (!server.GetSession(request.Whisper.RecipientId, out GameSession? session)) {
            throw new RpcException(new Status(StatusCode.NotFound, $"Unable to whisper: {request.Whisper.RecipientName}"));
        }

        session.Send(ChatPacket.Whisper(request.AccountId, request.CharacterId, request.Name, request.Message, request.Name));
    }

    private void PartyChat(ChatRequest request) {
        foreach (long characterId in request.Party.MemberIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            session.Send(ChatPacket.Message(request.AccountId, request.CharacterId, request.Name, ChatType.Party, request.Message));
        }
    }

    private void GuildChat(ChatRequest request) {
        foreach (long characterId in request.Guild.MemberIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            session.Send(ChatPacket.Message(request.AccountId, request.CharacterId, request.Name, ChatType.Guild, request.Message));
        }
    }

    private void WorldChat(ChatRequest request) {
        server.Broadcast(ChatPacket.Message(request.AccountId, request.CharacterId, request.Name, ChatType.World, request.Message));
    }

    private void SuperChat(ChatRequest request) {
        server.Broadcast(ChatPacket.Message(request.AccountId, request.CharacterId, request.Name, ChatType.Super, request.Message, request.Super.ItemId));
    }
}
