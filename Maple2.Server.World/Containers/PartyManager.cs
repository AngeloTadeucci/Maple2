using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Party;
using Maple2.Server.Channel.Service;
using ChannelClient = Maple2.Server.Channel.Service.Channel.ChannelClient;

namespace Maple2.Server.World.Containers;

public class PartyManager : IDisposable {
    public required ChannelClientLookup ChannelClients { get; init; }
    public readonly Party Party;
    private readonly ConcurrentDictionary<long, (string, DateTime)> pendingInvites;

    public PartyManager(Party party) {
        Party = party;
        pendingInvites = new ConcurrentDictionary<long, (string, DateTime)>();
    }

    public void Dispose() {
        Broadcast(new PartyRequest {
            Disband = new PartyRequest.Types.Disband(),
        });
    }

    public void Broadcast(PartyRequest request) {
        if (request.PartyId > 0 && request.PartyId != Party.Id) {
            throw new InvalidOperationException($"Broadcasting {request.PartyCase} for incorrect party: {request.PartyId} => {Party.Id}");
        }

        request.PartyId = Party.Id;
        foreach (IGrouping<short, PartyMember> group in Party.Members.Values.GroupBy(member => member.Info.Channel)) {
            if (!ChannelClients.TryGetClient(group.Key, out ChannelClient? client)) {
                continue;
            }

            request.ReceiverIds.Clear();
            request.ReceiverIds.AddRange(group.Select(member => member.Info.CharacterId));

            try {
                client.Party(request);
            } catch { }
        }
    }

    public void FindNewLeader(long characterId) {
        PartyMember? newLeader = Party.Members.Values.FirstOrDefault(m => m.CharacterId != characterId);
        if (newLeader == null) {
            CheckForDisband();
            return;
        }
        UpdateLeader(characterId, newLeader.CharacterId);
    }

    public bool CheckForDisband() {
        if (Party.Members.Count <= 2) {
            Dispose();
            return true;
        }
        return false;
    }

    public PartyError Invite(long requestorId, PlayerInfo player) {
        if (!Party.Members.TryGetValue(requestorId, out PartyMember? requestor)) {
            return PartyError.s_party_err_not_exist;
        }
        bool isLeader = requestorId == Party.LeaderCharacterId;
        if (!isLeader) {
            return PartyError.s_party_err_not_chief;
        }
        if (Party.Members.Count >= Party.Capacity) {
            return PartyError.s_party_err_full;
        }
        if (Party.Members.ContainsKey(player.CharacterId)) {
            return PartyError.s_party_err_cannot_invite;
        }
        if (!ChannelClients.TryGetClient(player.Channel, out ChannelClient? client)) {
            return PartyError.s_party_err_cannot_invite;
        }

        try {
            pendingInvites[player.CharacterId] = (requestor.Name, DateTime.Now.AddSeconds(30));
            var request = new PartyRequest {
                PartyId = Party.Id,
                ReceiverIds = {
                    player.CharacterId
                },
                Invite = new PartyRequest.Types.Invite {
                    SenderId = requestor.CharacterId,
                    SenderName = requestor.Name,
                },
            };

            PartyResponse response = client.Party(request);
            return (PartyError) response.Error;
        } catch (RpcException) {
            return PartyError.s_party_err_not_found;
        }
    }

    public PartyError Join(PlayerInfo info) {
        if (Party.Members.Count >= Party.Capacity) {
            return PartyError.s_party_err_full_limit_player;
        }
        if (Party.Members.ContainsKey(info.CharacterId)) {
            return PartyError.s_party_err_alreadyInvite;
        }

        PartyMember member = new PartyMember {
            PartyId = Party.Id,
            Info = info.Clone(),
            JoinTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            LoginTime = info.UpdateTime,
        };

        Broadcast(new PartyRequest {
            AddMember = new PartyRequest.Types.AddMember {
                CharacterId = member.CharacterId,
                JoinTime = member.JoinTime,
                LoginTime = member.LoginTime,
            },
        });
        Party.Members.TryAdd(info.CharacterId, member);
        return PartyError.none;
    }

    public PartyError Kick(long requestorId, long characterId) {
        if (!Party.Members.TryGetValue(requestorId, out PartyMember? _)) {
            return PartyError.s_party_err_not_found;
        }
        if (requestorId != Party.LeaderCharacterId) {
            return PartyError.s_party_err_not_chief;
        }
        if (characterId == Party.LeaderCharacterId) {
            return PartyError.none;
        }
        if (!Party.Members.TryGetValue(characterId, out PartyMember? member)) {
            return PartyError.s_party_err_not_exist;
        }
        if (CheckForDisband()) {
            return PartyError.none;
        }

        Broadcast(new PartyRequest {
            RemoveMember = new PartyRequest.Types.RemoveMember {
                CharacterId = member.CharacterId,
                IsKicked = true,
            },
        });
        Party.Members.TryRemove(member.CharacterId, out _);
        return PartyError.none;
    }

    public PartyError Leave(long characterId) {
        if (!Party.Members.TryGetValue(characterId, out PartyMember? member)) {
            return PartyError.s_party_err_not_found;
        }

        if (characterId == Party.LeaderCharacterId) {
            FindNewLeader(characterId);
        }

        if (CheckForDisband()) {
            return PartyError.none;
        }

        Broadcast(new PartyRequest {
            RemoveMember = new PartyRequest.Types.RemoveMember {
                CharacterId = member.CharacterId,
            },
        });
        Party.Members.TryRemove(member.CharacterId, out _);
        return PartyError.none;
    }

    public PartyError UpdateLeader(long requestorId, long characterId) {
        if (!Party.Members.TryGetValue(requestorId, out PartyMember? requestor)) {
            return PartyError.s_party_err_not_found;
        }
        if (!Party.Members.TryGetValue(characterId, out PartyMember? member)) {
            return PartyError.s_party_err_not_found;
        }

        if (requestorId != Party.LeaderCharacterId) {
            return PartyError.s_party_err_not_chief;
        }

        Party.LeaderCharacterId = member.Info.CharacterId;
        Party.LeaderAccountId = member.Info.AccountId;
        Party.LeaderName = member.Info.Name;
        Broadcast(new PartyRequest {
            UpdateLeader = new PartyRequest.Types.UpdateLeader {
                CharacterId = characterId,
            },
        });

        return PartyError.none;
    }

    public string ConsumeInvite(long characterId) {
        foreach ((long id, (string name, DateTime expiryTime)) in pendingInvites) {
            // Remove any expired entries while iterating.
            if (expiryTime < DateTime.Now) {
                pendingInvites.Remove(id, out _);
                continue;
            }

            if (id == characterId) {
                pendingInvites.Remove(id, out _);
                return name;
            }
        }

        return string.Empty;
    }

    public PartyError StartReadyCheck(long requestorId) {
        if (Party.Vote != null) {
            return PartyError.s_party_err_already_vote;
        }

        if (!Party.Members.TryGetValue(requestorId, out PartyMember? requestor)) {
            return PartyError.s_party_err_not_found;
        }
        if (requestorId != Party.LeaderCharacterId) {
            return PartyError.s_party_err_not_chief;
        }

        Party.Vote = new PartyVote(PartyVoteType.ReadyCheck, Party.Members.Keys, requestorId);
        Broadcast(new PartyRequest {
            StartReadyCheck = new PartyRequest.Types.StartReadyCheck {
                CharacterId = requestorId,
            },
        });

        return PartyError.none;
    }

    public PartyError ReadyCheckReply(long requestorId, bool ready) {
        if (Party.Vote == null) {
            return PartyError.s_party_err_not_found;
        }

        if (!Party.Members.TryGetValue(requestorId, out PartyMember? requestor)) {
            return PartyError.s_party_err_not_found;
        }

        if (Party.Vote.Approvals.Contains(requestorId) || Party.Vote.Disapprovals.Contains(requestorId)) {
            return PartyError.s_party_err_already_vote;
        }

        if (ready) {
            Party.Vote.Approvals.Add(requestorId);
        } else {
            Party.Vote.Disapprovals.Add(requestorId);
        }

        Broadcast(new PartyRequest {
            ReadyCheckReply = new PartyRequest.Types.ReadyCheckReply {
                CharacterId = requestorId,
                IsReady = ready,
            },
        });

        CheckEndReadyCheck();

        return PartyError.none;
    }

    private void CheckEndReadyCheck() {
        if (Party.Vote == null) {
            return;
        }

        int votes = Party.Vote.Approvals.Count + Party.Vote.Disapprovals.Count;
        if (votes >= Party.Vote.PartyCharacterIds.Count) {
            Broadcast(new PartyRequest {
                EndReadyCheck = new PartyRequest.Types.EndReadyCheck {
                    PartyId = Party.Id,
                },
            });
            Party.Vote = null;
        }
    }
}
