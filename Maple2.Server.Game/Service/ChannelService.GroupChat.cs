using Grpc.Core;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.GroupChat;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<GroupChatResponse> GroupChat(GroupChatRequest request, ServerCallContext context) {
        switch (request.GroupChatCase) {
            case GroupChatRequest.GroupChatOneofCase.AddMember:
                return Task.FromResult(AddGroupChatMember(request.GroupChatId, request.ReceiverIds, request.AddMember));
            case GroupChatRequest.GroupChatOneofCase.RemoveMember:
                return Task.FromResult(RemoveGroupChatMember(request.GroupChatId, request.ReceiverIds, request.RemoveMember));
            case GroupChatRequest.GroupChatOneofCase.Chat:
                return Task.FromResult(ChatGroupChat(request.GroupChatId, request.ReceiverIds, request.Chat));
            case GroupChatRequest.GroupChatOneofCase.Disband:
                return Task.FromResult(DisbandGroupChat(request.GroupChatId, request.ReceiverIds, request.Disband));
            default:
                return Task.FromResult(new GroupChatResponse { Error = (int) GroupChatError.s_err_groupchat_null_target_user });
        }
    }

    private GroupChatResponse AddGroupChatMember(int groupChatId, IEnumerable<long> receiverIds, GroupChatRequest.Types.AddMember add) {
        if (!playerInfos.GetOrFetch(add.CharacterId, out PlayerInfo? info)) {
            return new GroupChatResponse { Error = (int) GroupChatError.s_err_groupchat_null_target_user };
        }

        IEnumerable<long> characterIds = receiverIds.ToList();
        foreach (long characterId in characterIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            if (characterId == add.CharacterId) {
                JoinGroupChat(session, add.RequesterName, characterIds, ToGroupChatInfo(groupChatId, characterIds));
                continue;
            }

            if (!session.GroupChats.TryGetValue(groupChatId, out GroupChatManager? manager)) {
                continue;
            }

            GroupChatMember? sender = manager.GetMember(add.RequesterId);
            if (sender == null) {
                continue;
            }
            manager.AddMember(sender, new GroupChatMember {
                Info = info.Clone(),
            });
        }

        return new GroupChatResponse();
    }

    private void JoinGroupChat(GameSession session, string inviterName, IEnumerable<long> receiverIds, GroupChatInfo groupChatInfo) {
        GroupChat groupChat = new GroupChat(groupChatInfo.Id);
        foreach (long receiverId in receiverIds) {
            if (!session.PlayerInfo.GetOrFetch(receiverId, out PlayerInfo? playerInfo)) {
                continue;
            }
            groupChat.Members.TryAdd(receiverId, new GroupChatMember {
                Info = playerInfo.Clone(),
            });
        }

        session.GroupChats.TryAdd(groupChatInfo.Id, new GroupChatManager(groupChatInfo, session));
        session.Send(GroupChatPacket.Join(inviterName, session.PlayerName, groupChatInfo.Id));
    }

    private GroupChatResponse RemoveGroupChatMember(int groupChatId, IEnumerable<long> receiverIds, GroupChatRequest.Types.RemoveMember remove) {
        foreach (long characterId in receiverIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            if (!session.GroupChats.TryGetValue(groupChatId, out GroupChatManager? manager)) {
                continue;
            }

            manager.RemoveMember(remove.CharacterId);
            if (characterId == remove.CharacterId) {
                session.GroupChats.Remove(groupChatId, out _);
            }
        }

        return new GroupChatResponse();
    }

    private GroupChatResponse ChatGroupChat(int groupChatId, IEnumerable<long> receiverIds, GroupChatRequest.Types.Chat chat) {
        foreach (long characterId in receiverIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            session.Send(GroupChatPacket.Chat(chat.RequesterName, groupChatId, chat.Message));
        }
        return new GroupChatResponse();
    }

    private GroupChatResponse DisbandGroupChat(int groupChatId, IEnumerable<long> receiverIds, GroupChatRequest.Types.Disband disband) {
        foreach (long characterId in receiverIds) {
            if (!server.GetSession(characterId, out GameSession? session) || !session.GroupChats.TryGetValue(groupChatId, out GroupChatManager? manager)) {
                continue;
            }

            manager.Disband();
        }

        return new GroupChatResponse();
    }

    private static GroupChatInfo ToGroupChatInfo(int groupChatId, IEnumerable<long> receiverIds) {
        return new GroupChatInfo {
            Id = groupChatId,
            Members = {
                receiverIds.Select(characterId => new GroupChatInfo.Types.Member {
                    CharacterId = characterId,
                }),
            },
        };
    }
}

