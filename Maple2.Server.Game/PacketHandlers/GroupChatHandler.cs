using System;
using Grpc.Core;
using Maple2.Database.Storage;
using Maple2.Model.Error;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.World.Service;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

namespace Maple2.Server.Game.PacketHandlers;

public class GroupChatHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.GroupChat;

    private enum Command : byte {
        Create = 1,
        Invite = 2,
        Leave = 4,
        Chat = 10,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required WorldClient World { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Create:
                HandleCreate(session);
                break;
            case Command.Invite:
                HandleInvite(session, packet);
                break;
            case Command.Leave:
                HandleLeave(session, packet);
                break;
            case Command.Chat:
                HandleChat(session, packet);
                break;
        }
    }

    private void HandleCreate(GameSession session) {
        if (session.GroupChats.Count >= Constant.GroupChatMaxCapacity) {
            session.Send(GroupChatPacket.Error(GroupChatError.s_err_groupchat_maxgroup, session.PlayerName, session.PlayerName));
            return;
        }

        GroupChatResponse response = World.GroupChat(new GroupChatRequest {
            Create = new GroupChatRequest.Types.Create(),
            RequesterId = session.CharacterId,
        });

        var error = (GroupChatError) response.Error;
        if (error != GroupChatError.none) {
            session.Send(GroupChatPacket.Error(error, session.PlayerName, string.Empty));
            return;
        }

        if (!session.GroupChats.TryAdd(response.GroupChat.Id, new GroupChatManager(response.GroupChat, session))) {
            throw new InvalidOperationException($"Failed to add group chat: {response.GroupChat.Id}");
        }

        session.Send(GroupChatPacket.Create(response.GroupChat.Id));
    }

    private void HandleInvite(GameSession session, IByteReader packet) {
        string targetPlayerName = packet.ReadUnicodeString();
        int groupChatId = packet.ReadInt();

        using GameStorage.Request db = session.GameStorage.Context();
        long characterId = db.GetCharacterId(targetPlayerName);
        if (characterId == 0) {
            session.Send(GroupChatPacket.Error(GroupChatError.s_err_groupchat_null_target_user, session.PlayerName, targetPlayerName));
            return;
        }

        try {
            GroupChatInfoResponse infoResponse = World.GroupChatInfo(new GroupChatInfoRequest {
                CharacterId = characterId,
            });

            if (infoResponse.Infos.Count >= Constant.GroupChatMaxCount) {
                session.Send(GroupChatPacket.Error(GroupChatError.s_err_groupchat_maxgroup, session.PlayerName, targetPlayerName));
                return;
            }

            session.Send(GroupChatPacket.Invite(session.PlayerName, targetPlayerName, groupChatId));
            var request = new GroupChatRequest {
                Invite = new GroupChatRequest.Types.Invite {
                    ReceiverId = characterId,
                },
                GroupChatId = groupChatId,
                RequesterId = session.CharacterId,
            };

            GroupChatResponse response = World.GroupChat(request);
            var error = (GroupChatError) response.Error;
            if (error != GroupChatError.none) {
                session.Send(GroupChatPacket.Error(error, session.PlayerName, targetPlayerName));
            }
        } catch (RpcException ex) {
            Logger.Error(ex, "Failed to invite {Name} to group chat", targetPlayerName);
            session.Send(GroupChatPacket.Error(GroupChatError.s_err_groupchat_add_member_target, session.PlayerName, targetPlayerName));
        }
    }

    private void HandleLeave(GameSession session, IByteReader packet) {
        int groupChatId = packet.ReadInt();

        if (!session.GroupChats.ContainsKey(groupChatId)) {
            return;
        }

        try {
            World.GroupChat(new GroupChatRequest {
                Leave = new GroupChatRequest.Types.Leave(),
                GroupChatId = groupChatId,
                RequesterId = session.CharacterId,
            });
        } catch (RpcException ex) {
            Logger.Error(ex, "Failed to leave group chat {GroupChatId}", groupChatId);
        }
    }

    private void HandleChat(GameSession session, IByteReader packet) {
        string message = packet.ReadUnicodeString();
        int groupChatId = packet.ReadInt();

        if (!session.GroupChats.ContainsKey(groupChatId)) {
            return;
        }

        World.GroupChat(new GroupChatRequest {
            Chat = new GroupChatRequest.Types.Chat {
                Message = message,
            },
            GroupChatId = groupChatId,
            RequesterId = session.CharacterId,
        });
    }
}
