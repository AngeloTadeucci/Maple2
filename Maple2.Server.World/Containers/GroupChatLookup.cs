using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.GroupChat;
using Serilog;

namespace Maple2.Server.World.Containers;

public class GroupChatLookup(ChannelClientLookup channelClients, PlayerInfoLookup playerLookup) : IDisposable {

    private readonly ConcurrentDictionary<int, GroupChatManager> groupChats = new();
    private int nextGroupChatId = 1;

    public void Dispose() {
        foreach (GroupChatManager manager in groupChats.Values) {
            manager.Dispose();
        }
    }

    public bool TryGet(int groupChatId, [NotNullWhen(true)] out GroupChatManager? groupChat) {
        return groupChats.TryGetValue(groupChatId, out groupChat);
    }

    public IEnumerable<GroupChatManager> TryGetMany(long characterId) {
        foreach (GroupChatManager manager in groupChats.Values) {
            if (manager.GroupChat.Members.TryGetValue(characterId, out GroupChatMember? member)) {
                yield return manager;
            }
        }
    }

    public GroupChatError Create(long requesterId, out int groupChatId) {
        groupChatId = nextGroupChatId++;

        PlayerInfo? requesterInfo = playerLookup.GetPlayerInfo(requesterId);
        if (requesterInfo == null) {
            return GroupChatError.s_err_groupchat_null_target_user;
        }

        var groupChat = new GroupChat(groupChatId);
        var manager = new GroupChatManager(groupChat) {
            ChannelClients = channelClients,
        };

        if (!groupChats.TryAdd(groupChatId, manager)) {
            return GroupChatError.s_err_groupchat_null_target_user;
        }

        manager.Create(requesterInfo);
        return GroupChatError.none;
    }

    public void Disband(int groupChatId) {
        if (!TryGet(groupChatId, out GroupChatManager? manager)) {
            Log.Error("Failed to disband group chat {GroupChatId} because it was not found", groupChatId);
            return;
        }

        if (!groupChats.TryRemove(groupChatId, out manager)) {
            Log.Error("Unable to remove group chat {GroupChatId} in World", groupChatId);
            return;
        }
        manager.Dispose();
    }
}
