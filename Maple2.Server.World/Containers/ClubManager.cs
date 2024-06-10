using System.Collections.Concurrent;
using Grpc.Core;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Club;
using Maple2.Model.Metadata;
using Maple2.Server.Channel.Service;
using ChannelClient = Maple2.Server.Channel.Service.Channel.ChannelClient;
using ClubResponse = Maple2.Server.Channel.Service.ClubResponse;


namespace Maple2.Server.World.Containers;

public class ClubManager : IDisposable {
    public required GameStorage GameStorage { get; init; }
    public required ChannelClientLookup ChannelClients { get; init; }

    public readonly Club Club;

    private readonly List<long> acceptedInvites = []; // for tracking invites accepted to create club.
    private readonly ConcurrentDictionary<long, (string, DateTime)> pendingInvites;


    public ClubManager(Club club) {
        Club = club;
        pendingInvites = new ConcurrentDictionary<long, (string, DateTime)>();
    }

    public ClubError NewClubInvite(long characterId, Model.Enum.ClubResponse reply) {
        if (Club.State == ClubState.Established) {
            return ClubError.s_club_err_unknown;
        }
        if (!Club.Members.TryGetValue(characterId, out ClubMember? member)) {
            return ClubError.s_club_err_unknown;
        }

        if (reply == Model.Enum.ClubResponse.Accept) {
            if (acceptedInvites.Contains(characterId)) {
                return ClubError.s_club_err_unknown;
            }

            acceptedInvites.Add(characterId);
            // All members have accepted the invite
            if (acceptedInvites.Count == Club.Members.Count) {
                Broadcast(new ClubRequest {
                    Establish = new ClubRequest.Types.Establish(),
                    ClubId = Club.Id,
                });

                using GameStorage.Request db = GameStorage.Context();
                Club.State = ClubState.Established;
                db.SaveClub(Club);
            }
            return ClubError.none;
        }

        Broadcast(new ClubRequest {
            ClubId = Club.Id,
            StagedClubInviteReply = new ClubRequest.Types.StagedClubInviteReply {
                CharacterId = characterId,
                Reply = (int) reply,
                Name = member.Name,
            },
        });

        // Any rejected invite will delete the club.
        Broadcast(new ClubRequest {
            ClubId = Club.Id,
            StagedClubFail = new ClubRequest.Types.StagedClubFail {
                Reply = (int) Model.Enum.ClubResponse.Fail,
            },
        });
        return ClubError.none;
    }

    public ClubError Invite(long requestorId, PlayerInfo info) {
        if (!Club.Members.TryGetValue(requestorId, out ClubMember? requestor)) {
            return ClubError.s_club_err_null_member;
        }

        if (requestorId != Club.LeaderId) {
            return ClubError.s_club_err_no_master;
        }

        if (Club.Members.Count >= Constant.MaxClubMembers) {
            return ClubError.s_club_err_full_member;
        }

        if (Club.Members.Values.Any(member => string.Equals(member.Name, info.Name, StringComparison.CurrentCultureIgnoreCase))) {
            return ClubError.s_club_err_already_exist;
        }

        if (info.ClubIds.Count >= Constant.ClubMaxCount) {
            return ClubError.s_club_err_full_club_member;
        }

        if (!ChannelClients.TryGetClient(info.Channel, out ChannelClient? client)) {
            return ClubError.s_club_err_unknown;
        }

        try {
            pendingInvites[info.CharacterId] = (requestor.Name, DateTime.Now.AddSeconds(30));
            var request = new ClubRequest {
                ClubId = Club.Id,
                ReceiverIds = {
                    info.CharacterId
                },
                Invite = new ClubRequest.Types.Invite {
                    ClubName = Club.Name,
                    SenderId = requestorId,
                    SenderName = requestor.Name,
                },
            };

            ClubResponse response = client.Club(request);
            return (ClubError) response.Error;
        } catch (RpcException) {
            return ClubError.s_club_err_unknown;
        }
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

    public ClubError Join(string requestorName, PlayerInfo info) {
        if (Club.Members.Count >= Constant.MaxClubMembers) {
            return ClubError.s_club_err_full_member;
        }

        if (Club.Members.ContainsKey(info.CharacterId)) {
            return ClubError.s_club_err_already_exist;
        }

        using GameStorage.Request db = GameStorage.Context();
        ClubMember? member = db.CreateClubMember(Club.Id, info);
        if (member == null) {
            return ClubError.s_club_err_null_invite_member;
        }

        // Broadcast before adding this new member.
        Broadcast(new ClubRequest {
            ClubId = Club.Id,
            AddMember = new ClubRequest.Types.AddMember {
                CharacterId = member.CharacterId,
                RequestorName = requestorName,
                JoinTime = member.JoinTime,
            },
        });
        Club.Members.TryAdd(member.CharacterId, member);
        return ClubError.none;
    }

    public ClubError Leave(long characterId) {
        if (!Club.Members.TryGetValue(characterId, out ClubMember? member)) {
            return ClubError.s_club_err_null_member;
        }

        if (characterId == Club.LeaderId) {
            FindNewLeader(characterId);
        }

        Broadcast(new ClubRequest {
            RemoveMember = new ClubRequest.Types.RemoveMember {
                CharacterId = characterId,
            },
            ClubId = Club.Id,
        });

        using GameStorage.Request db = GameStorage.Context();
        if (!db.DeleteClubMember(Club.Id, member.CharacterId)) {
            return ClubError.s_club_err_unknown;
        }

        if (!Club.Members.TryRemove(member.CharacterId, out _)) {
            return ClubError.s_club_err_unknown;
        }

        return ClubError.none;
    }

    private void FindNewLeader(long characterId) {
        ClubMember? newLeader = Club.Members.Values.FirstOrDefault(member => member.CharacterId != characterId);
        if (newLeader == null) {
            // disband?
            return;
        }

        Club.Leader = newLeader;
        Club.LeaderId = newLeader.CharacterId;
        Broadcast(new ClubRequest {
            UpdateLeader = new ClubRequest.Types.UpdateLeader {
                CharacterId = newLeader.CharacterId,
            },
            ClubId = Club.Id,
        });
    }

    public ClubError Rename(string newName) {
        if (string.Equals(Club.Name, newName, StringComparison.CurrentCultureIgnoreCase)) {
            return ClubError.s_club_err_same_club_name;
        }

        if (Club.NameChangeCooldown.FromEpochSeconds() > DateTime.Now) {
            return ClubError.s_club_err_remain_time;
        }

        if (newName.Contains(' ')) {
            return ClubError.s_club_err_clubname_has_blank;
        }

        using GameStorage.Request db = GameStorage.Context();
        if (db.ClubExists(newName)) {
            return ClubError.s_club_err_name_exist;
        }

        Club.Name = newName;
        Club.NameChangeCooldown = DateTime.Now.AddHours(1).ToEpochSeconds();
        Broadcast(new ClubRequest {
            Rename = new ClubRequest.Types.Rename {
                Name = newName,
                ChangedTime = Club.NameChangeCooldown,
            },
            ClubId = Club.Id,
        });

        return ClubError.none;
    }

    public void Disband() {
        Broadcast(new ClubRequest {
            ClubId = Club.Id,
            Disband = new ClubRequest.Types.Disband(),
        });
        Dispose();
    }

    public void Broadcast(ClubRequest request) {
        if (request.ClubId > 0 && request.ClubId != Club.Id) {
            throw new InvalidOperationException($"Broadcasting {request.ClubCase} for incorrect guild: {request.ClubId} => {Club.Id}");
        }

        foreach (IGrouping<short, ClubMember> group in Club.Members.Values.GroupBy(member => member.Info.Channel)) {
            if (!ChannelClients.TryGetClient(group.Key, out ChannelClient? client)) {
                continue;
            }

            request.ReceiverIds.Clear();
            request.ReceiverIds.AddRange(group.Select(member => member.Info.CharacterId));

            try {
                client.Club(request);
            } catch { /* ignored */
            }
        }
    }

    public void Dispose() {
        using GameStorage.Request db = GameStorage.Context();
        db.SaveClub(Club);
    }
}
