using Grpc.Core;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<ChatResponse> Chat(ChatRequest request, ServerCallContext context) {
        using GameStorage.Request db = gameStorage.Context();

        List<Item> items = [];
        foreach (long itemUid in request.ItemUids) {
            Item? item = db.GetItem(itemUid);
            if (item != null) {
                items.Add(item);
            }
        }
        switch (request.ChatCase) {
            case ChatRequest.ChatOneofCase.Whisper:
                WhisperChat(request, items);
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.Party:
                PartyChat(request, items);
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.Guild:
                GuildChat(request, items);
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.World:
                WorldChat(request, items);
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.Super:
                SuperChat(request, items);
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.Club:
                ClubChat(request, items);
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.Wedding:
                return Task.FromResult(new ChatResponse());
            case ChatRequest.ChatOneofCase.SystemNotice:
                SystemChat(request, items);
                return Task.FromResult(new ChatResponse());
            default:
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, $"Invalid chat type: {request.ChatCase}"));
        }
    }

    private void WhisperChat(ChatRequest request, List<Item> items) {
        if (!server.GetSession(request.Whisper.RecipientId, out GameSession? session)) {
            throw new RpcException(new Status(StatusCode.NotFound, $"Unable to whisper: {request.Whisper.RecipientName}"));
        }

        if (items.Count > 0) {
            session.Send(MessengerBrowserPacket.Link(items.ToArray()));
        }

        session.Send(ChatPacket.Whisper(request.AccountId, request.CharacterId, request.Name, request.Message, request.Name));
    }

    private void PartyChat(ChatRequest request, List<Item> items) {
        foreach (long characterId in request.Party.MemberIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            if (items.Count > 0) {
                session.Send(MessengerBrowserPacket.Link(items.ToArray()));
            }
            session.Send(ChatPacket.Message(request.AccountId, request.CharacterId, request.Name, ChatType.Party, request.Message));
        }
    }

    private void GuildChat(ChatRequest request, List<Item> items) {
        foreach (long characterId in request.Guild.MemberIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            if (items.Count > 0) {
                session.Send(MessengerBrowserPacket.Link(items.ToArray()));
            }

            session.Send(ChatPacket.Message(request.AccountId, request.CharacterId, request.Name, ChatType.Guild, request.Message));
        }
    }

    private void WorldChat(ChatRequest request, List<Item> items) {
        if (items.Count > 0) {
            server.Broadcast(MessengerBrowserPacket.Link(items.ToArray()));
        }
        server.Broadcast(ChatPacket.Message(request.AccountId, request.CharacterId, request.Name, ChatType.World, request.Message));
    }

    private void SuperChat(ChatRequest request, List<Item> items) {
        if (items.Count > 0) {
            server.Broadcast(MessengerBrowserPacket.Link(items.ToArray()));
        }
        server.Broadcast(ChatPacket.Message(request.AccountId, request.CharacterId, request.Name, ChatType.Super, request.Message, request.Super.ItemId));
    }

    private void ClubChat(ChatRequest request, List<Item> items) {
        foreach (long characterId in request.Club.MemberIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            if (items.Count > 0) {
                session.Send(MessengerBrowserPacket.Link(items.ToArray()));
            }

            session.Send(ChatPacket.Message(request.AccountId, request.CharacterId, request.Name, ChatType.Club, request.Message, clubId: request.Club.ClubId));
        }
    }

    private void SystemChat(ChatRequest request, List<Item> items) {
        if (items.Count > 0) {
            server.Broadcast(MessengerBrowserPacket.Link(items.ToArray()));
        }
        server.Broadcast(ChatPacket.Message(request.AccountId, request.CharacterId, request.Name, ChatType.SystemNotice, request.Message));
    }
}
