﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Party;

namespace Maple2.Server.World.Containers;

public class GroupChatLookup : IDisposable {
    private readonly ChannelClientLookup channelClients;
    private readonly PlayerInfoLookup playerLookup;

    private readonly ConcurrentDictionary<int, GroupChatManager> groupChats;
    private int nextGroupChatId = 1;

    public GroupChatLookup(ChannelClientLookup channelClients, PlayerInfoLookup playerLookup) {
        this.channelClients = channelClients;
        this.playerLookup = playerLookup;

        groupChats = new ConcurrentDictionary<int, GroupChatManager>();
    }

    public void Dispose() {
        foreach (GroupChatManager manager in groupChats.Values) {
            manager.Dispose();
        }
    }

    public bool TryGet(int groupChatId, [NotNullWhen(true)] out GroupChatManager? groupChat) {
        return groupChats.TryGetValue(groupChatId, out groupChat);
    }

    /*public bool TryGetByCharacter(long characterId, [NotNullWhen(true)] out PartyManager? party) {
        party = null;
        foreach (PartyManager manager in parties.Values) {
            if (manager.Party.Members.TryGetValue(characterId, out PartyMember? member)) {
                party = manager;
                return true;
            }
        }
        return false;
    }*/

    public GroupChatError Create(long requesterId, out int groupChatId) {
        groupChatId = nextGroupChatId++;

        PlayerInfo? requesterInfo = playerLookup.GetPlayerInfo(requesterId);
        if (requesterInfo == null) {
            return GroupChatError.s_err_groupchat_null_target_user;
        }

        Party party = new Party(partyId, leaderInfo.AccountId, leaderInfo.CharacterId, leaderInfo.Name);
        PartyManager manager = new PartyManager(party) {
            ChannelClients = channelClients,
        };

        if (!parties.TryAdd(partyId, manager)) {
            return PartyError.s_party_err_not_found;
        }

        return manager.Join(leaderInfo);
    }

    public PartyError Disband(long requestorId, int partyId) {
        if (!TryGet(partyId, out PartyManager? manager)) {
            return PartyError.s_party_err_not_found;
        }
        if (requestorId != manager.Party.LeaderCharacterId) {
            return PartyError.s_party_err_not_chief;
        }
        if (!parties.TryRemove(partyId, out manager)) {
            // Failed to remove party after validating.
            return PartyError.s_party_err_not_found;
        }
        manager.Dispose();

        return PartyError.none;
    }
}
