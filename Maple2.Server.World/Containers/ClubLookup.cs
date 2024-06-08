using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Club;

namespace Maple2.Server.World.Containers;

public class ClubLookup : IDisposable {
    private readonly GameStorage gameStorage;
    private readonly ChannelClientLookup channelClients;
    private readonly PlayerInfoLookup playerLookup;
    private readonly PartyLookup partyLookup;

    private readonly ConcurrentDictionary<long, ClubManager> clubs;

    public ClubLookup(ChannelClientLookup channelClients, PlayerInfoLookup playerLookup, GameStorage gameStorage, PartyLookup partyLookup) {
        this.gameStorage = gameStorage;
        this.channelClients = channelClients;
        this.playerLookup = playerLookup;
        this.partyLookup = partyLookup;

        clubs = new ConcurrentDictionary<long, ClubManager>();
    }

    public void Dispose() {
        // We must dispose all ClubManager to save state.
        foreach (ClubManager manager in clubs.Values) {
            manager.Dispose();
        }
    }

    public bool TryGet(long clubId, [NotNullWhen(true)] out ClubManager? club) {
        if (clubs.TryGetValue(clubId, out club)) {
            return true;
        }

        club = FetchClub(clubId);
        return club != null;
    }

    public List<ClubManager> TryGetByCharacterId(long characterId) {
        List<ClubManager> clubManagers = [];
        using GameStorage.Request db = gameStorage.Context();
        IList<long> clubIds = db.ListClubs(characterId);
        foreach (long clubId in clubIds) {
            if (TryGet(clubId, out ClubManager? club)) {
                // If the club is staged, we can dispose it.
                if (club.Club.State == ClubState.Staged) {
                    club.Dispose();
                    continue;
                }
                clubManagers.Add(club);
            }
        }
        return clubManagers;
    }

    public ClubManager? FetchClub(long clubId) {
        using GameStorage.Request db = gameStorage.Context();
        Club? club = db.GetClub(clubId);
        if (club == null) {
            return null;
        }

        SetMembers(db, club);

        ClubManager manager = new ClubManager(club) {
            GameStorage = gameStorage,
            ChannelClients = channelClients,
        };

        return clubs.TryAdd(clubId, manager) ? manager : null;

    }

    private void SetMembers(GameStorage.Request db, Club club) {
        List<ClubMember> members = db.GetClubMembers(playerLookup, club.Id);
        club.Leader = members.First(member => member.Info.CharacterId == club.LeaderId);
        foreach (ClubMember member in members) {
            club.Members.TryAdd(member.Info.CharacterId, member);
        }
    }

    public ClubError Create(string name, long leaderId, out long clubId) {
        clubId = 0;
        using GameStorage.Request db = gameStorage.Context();
        if (db.ClubExists(clubName: name)) {
            return ClubError.s_club_err_name_exist;
        }

        if (!partyLookup.TryGetByCharacter(leaderId, out PartyManager? party)) {
            return ClubError.s_club_err_unknown;
        }

        Club? club = db.CreateClub(name, leaderId, party.Party.Members.Values.ToList());
        if (club == null) {
            return ClubError.s_club_err_unknown;
        }

        SetMembers(db, club);

        clubId = club.Id;
        clubs.TryAdd(clubId, new ClubManager(club) {
            GameStorage = gameStorage,
            ChannelClients = channelClients,
        });

        return ClubError.none;
    }

    public ClubError Disband(long clubId) {
        if (!TryGet(clubId, out ClubManager? manager)) {
            return ClubError.s_club_err_null_club;
        }

        if (!clubs.TryRemove(clubId, out manager)) {
            return ClubError.s_club_err_unknown;
        }

        manager.Disband();

        using GameStorage.Request db = gameStorage.Context();
        if (!db.DeleteClub(clubId)) {
            return ClubError.s_club_err_unknown;
        }

        return ClubError.none;
    }

}
