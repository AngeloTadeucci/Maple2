using System;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Server.Channel.Service;
using ChannelClient = Maple2.Server.Channel.Service.Channel.ChannelClient;


namespace Maple2.Server.World.Containers;

public class ClubManager : IDisposable {
    public required GameStorage GameStorage { get; init; }
    public required ChannelClientLookup ChannelClients { get; init; }

    public readonly Club Club;

    private List<long> acceptedInvites = []; // for tracking invites accepted to create club.

    public ClubManager(Club club) {
        Club = club;
    }

    public ClubError NewClubInvite(long characterId, ClubInviteReply reply) {
        if (Club.State == ClubState.Established) {
            return ClubError.s_club_err_unknown;
        }
        if (!Club.Members.TryGetValue(characterId, out ClubMember? member)) {
            return ClubError.s_club_err_unknown;
        }

        if (reply == ClubInviteReply.Accept) {
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
                bool saved = db.SaveClub(Club);
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
                Reply = (int) ClubInviteReply.Fail,
            },
        });
        return ClubError.none;
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
            } catch { /* ignored */ }
        }
    }

    public void Dispose() {
        using GameStorage.Request db = GameStorage.Context();
        db.SaveClub(Club);
    }
}
