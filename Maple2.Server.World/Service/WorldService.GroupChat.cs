using Grpc.Core;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.GroupChat;
using Maple2.Server.World.Containers;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<GroupChatInfoResponse> GroupChatInfo(GroupChatInfoRequest request, ServerCallContext context) {
        IEnumerable<GroupChatManager> groupChatManagers = groupChatLookup.TryGetMany(request.CharacterId);
        var groupChats = new List<GroupChatInfo>();
        foreach (GroupChatManager manager in groupChatManagers) {
            groupChats.Add(ToGroupChatInfo(manager.GroupChat));
        }

        return Task.FromResult(new GroupChatInfoResponse {
            Infos = {
                groupChats,
            },
        });
    }

    public override Task<GroupChatResponse> GroupChat(GroupChatRequest request, ServerCallContext context) {
        switch (request.GroupChatCase) {
            case GroupChatRequest.GroupChatOneofCase.Create:
                return Task.FromResult(CreateGroupChat(request.RequesterId, request.Create));
            case GroupChatRequest.GroupChatOneofCase.Invite:
                return Task.FromResult(InviteGroupChat(request.GroupChatId, request.RequesterId, request.Invite));
            case GroupChatRequest.GroupChatOneofCase.Leave:
                return Task.FromResult(LeaveGroupChat(request.GroupChatId, request.RequesterId, request.Leave));
            case GroupChatRequest.GroupChatOneofCase.Chat:
                return Task.FromResult(ChatGroupChat(request.GroupChatId, request.RequesterId, request.Chat));
            case GroupChatRequest.GroupChatOneofCase.Disband:
                return Task.FromResult(DisbandGroupChat(request.GroupChatId, request.Disband));
            default:
                return Task.FromResult(new GroupChatResponse {
                    Error = (int) GroupChatError.none,
                });
        }
    }

    private GroupChatResponse CreateGroupChat(long requesterId, GroupChatRequest.Types.Create create) {
        GroupChatError error = groupChatLookup.Create(requesterId, out int groupChatId);
        if (error != GroupChatError.none) {
            return new GroupChatResponse {
                Error = (int) error,
            };
        }
        if (!groupChatLookup.TryGet(groupChatId, out GroupChatManager? manager)) {
            return new GroupChatResponse {
                Error = (int) GroupChatError.s_err_groupchat_null_target_user,
            };
        }

        if (!playerLookup.TryGet(requesterId, out PlayerInfo? info)) {
            return new GroupChatResponse {
                Error = (int) GroupChatError.s_err_groupchat_null_target_user,
            };
        }

        return new GroupChatResponse {
            GroupChat = ToGroupChatInfo(manager.GroupChat),
        };
    }

    private GroupChatResponse InviteGroupChat(int groupChatId, long requestorId, GroupChatRequest.Types.Invite invite) {
        if (!groupChatLookup.TryGet(groupChatId, out GroupChatManager? manager)) {
            return new GroupChatResponse {
                Error = (int) GroupChatError.s_err_groupchat_null_target_user,
            };
        }
        if (!playerLookup.TryGet(invite.ReceiverId, out PlayerInfo? info)) {
            return new GroupChatResponse {
                Error = (int) GroupChatError.s_err_groupchat_null_target_user,
            };
        }

        GroupChatError error = manager.Invite(requestorId, info);
        return new GroupChatResponse {
            Error = (int) error,
        };
    }

    private GroupChatResponse LeaveGroupChat(int groupChatId, long requestorId, GroupChatRequest.Types.Leave leave) {
        if (!groupChatLookup.TryGet(groupChatId, out GroupChatManager? manager)) {
            return new GroupChatResponse {
                Error = (int) GroupChatError.s_err_groupchat_null_target_user,
            };
        }

        manager.Leave(requestorId);
        return new GroupChatResponse();
    }

    private GroupChatResponse ChatGroupChat(int groupChatId, long requesterId, GroupChatRequest.Types.Chat chat) {
        if (!groupChatLookup.TryGet(groupChatId, out GroupChatManager? manager) || !manager.GroupChat.Members.TryGetValue(requesterId, out GroupChatMember? member)) {
            return new GroupChatResponse {
                Error = (int) GroupChatError.s_err_groupchat_null_target_user,
            };
        }

        manager.Broadcast(new GroupChatRequest {
            Chat = new GroupChatRequest.Types.Chat {
                Message = chat.Message,
                RequesterName = member.Name,
            },
        });

        return new GroupChatResponse {
            GroupChatId = groupChatId,
        };
    }

    private GroupChatResponse DisbandGroupChat(int groupChatId, GroupChatRequest.Types.Disband disband) {
        if (!groupChatLookup.TryGet(groupChatId, out GroupChatManager? _)) {
            return new GroupChatResponse();
        }

        groupChatLookup.Disband(groupChatId);
        return new GroupChatResponse();
    }

    private static GroupChatInfo ToGroupChatInfo(GroupChat groupChat) {
        return new GroupChatInfo {
            Id = groupChat.Id,
            Members = {
                groupChat.Members.Values.Select(member => new GroupChatInfo.Types.Member {
                    CharacterId = member.CharacterId,
                }),
            },
        };
    }
}
