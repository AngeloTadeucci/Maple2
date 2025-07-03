using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game.Party;
using Maple2.Model.Metadata;

namespace Maple2.Server.World.Containers;

public class PartySearchLookup : IDisposable {
    private readonly ChannelClientLookup channelClients;

    private readonly ConcurrentDictionary<long, PartySearchManager> partySearches;
    private long nextPartySearchId = 1;

    public PartySearchLookup(ChannelClientLookup channelClients) {
        this.channelClients = channelClients;

        partySearches = new ConcurrentDictionary<long, PartySearchManager>();
    }

    public void Dispose() {
        foreach (PartySearchManager manager in partySearches.Values) {
            manager.Dispose();
        }
    }

    public bool TryGet(long listingId, [NotNullWhen(true)] out PartySearchManager? listing) {
        return partySearches.TryGetValue(listingId, out listing);
    }

    public bool TryGetByPartyId(int partyId, [NotNullWhen(true)] out PartySearchManager? listing) {
        listing = null;
        foreach (PartySearchManager manager in partySearches.Values) {
            if (manager.PartySearch.PartyId == partyId) {
                listing = manager;
                return true;
            }
        }
        return false;
    }

    public ICollection<PartySearch> FetchEntries(PartySearchSort sortBy, string searchString, int page) {
        ICollection<PartySearch> entries = new List<PartySearch>();
        foreach (PartySearchManager manager in partySearches.Values) {
            if (!string.IsNullOrEmpty(searchString) && !manager.PartySearch.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            if (manager.PartySearch.MemberCount >= manager.PartySearch.Size) {
                continue;
            }

            entries.Add(manager.PartySearch);
        }

        // Sort entries
        switch (sortBy) {
            case PartySearchSort.Newest:
                entries = entries.OrderByDescending(entry => entry.CreationTime).ToList();
                break;
            case PartySearchSort.Oldest:
                entries = entries.OrderBy(entry => entry.CreationTime).ToList();
                break;
            case PartySearchSort.LeastMembers:
                entries = entries.OrderBy(entry => entry.MemberCount).ToList();
                break;
            case PartySearchSort.MostMembers:
                entries = entries.OrderByDescending(entry => entry.MemberCount).ToList();
                break;
        }

        // Get entries by page
        int offset = page * Constant.PartyFinderListingsPageCount - Constant.PartyFinderListingsPageCount;
        return entries.Skip(offset).Take(Constant.PartyFinderListingsPageCount).ToList();
    }

    public PartySearchError Create(Party party, string name, bool noApproval, int size, out long listingId) {
        listingId = nextPartySearchId++;

        party.Search = new PartySearch(listingId, name, size) {
            PartyId = party.Id,
            NoApproval = noApproval,
            LeaderAccountId = party.LeaderAccountId,
            LeaderCharacterId = party.LeaderCharacterId,
            LeaderName = party.LeaderName,
            MemberCount = party.Members.Count,
            CreationTime = party.CreationTime,
        };

        var manager = new PartySearchManager(party.Search, party) {
            ChannelClients = channelClients,
        };

        if (!partySearches.TryAdd(listingId, manager)) {
            return PartySearchError.s_partysearch_err_server_db;
        }

        return PartySearchError.none;
    }

    public PartySearchError Remove(long listingId) {
        if (!partySearches.TryRemove(listingId, out PartySearchManager? manager)) {
            return PartySearchError.s_partysearch_err_server_db;
        }

        manager.Remove();
        return PartySearchError.none;
    }
}
