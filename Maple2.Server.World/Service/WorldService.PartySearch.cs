using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game.Party;
using Maple2.Server.World.Containers;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<PartySearchInfoResponse> PartySearchInfo(PartySearchInfoRequest request, ServerCallContext context) {
        if (!(request.Id != 0 && partySearchLookup.TryGet(request.Id, out PartySearchManager? manager)) && !(request.PartyId != 0 && partySearchLookup.TryGetByPartyId(request.PartyId, out manager))) {
            return Task.FromResult(new PartySearchInfoResponse());
        }

        return Task.FromResult(new PartySearchInfoResponse { PartySearch = ToPartySearchInfo(manager.PartySearch) });
    }

    public override Task<PartySearchResponse> PartySearch(PartySearchRequest request, ServerCallContext context) {
        switch (request.PartySearchCase) {
            case PartySearchRequest.PartySearchOneofCase.Create:
                return Task.FromResult(Create(request.PartyId, request.Create));
            case PartySearchRequest.PartySearchOneofCase.Fetch:
                return Task.FromResult(Fetch(request.Fetch));
            case PartySearchRequest.PartySearchOneofCase.Remove:
                return Task.FromResult(Remove(request.Id, request.Remove));
            default:
                return Task.FromResult(new PartySearchResponse { Error = (int) PartyError.none });
        }
    }

    private PartySearchResponse Create(int partyId, PartySearchRequest.Types.Create create) {
        if (!partyLookup.TryGet(partyId, out PartyManager? party)) {
            return new PartySearchResponse { Error = (int) PartySearchError.s_partysearch_err_server_db };
        }

        PartySearchError error = partySearchLookup.Create(party.Party, create.Name, create.NoApproval, create.Size, out long partySearchId);

        if (error != PartySearchError.none) {
            return new PartySearchResponse { Error = (int) error };
        }

        if (!partySearchLookup.TryGet(partySearchId, out PartySearchManager? manager)) {
            return new PartySearchResponse { Error = (int) PartySearchError.s_partysearch_err_server_db };
        }

        manager.Set(create.RequestorId);
        return new PartySearchResponse { Error = (int) PartySearchError.none };
    }

    private PartySearchResponse Fetch(PartySearchRequest.Types.Fetch fetch) {
        ICollection<PartySearch> entries = partySearchLookup.FetchEntries((PartySearchSort) fetch.SortBy, fetch.SearchString, fetch.Page);
        return new PartySearchResponse { PartySearches = { entries.Select(ToPartySearchInfo).ToList() } };
    }

    private PartySearchResponse Remove(long listingId, PartySearchRequest.Types.Remove remove) {
        PartySearchError error = partySearchLookup.Remove(listingId);

        if (error == PartySearchError.none) {
            return new PartySearchResponse { Error = (int) PartySearchError.none };
        }

        return new PartySearchResponse { Error = (int) error };
    }

    private static PartySearchInfo ToPartySearchInfo(PartySearch partySearch) {
        return new PartySearchInfo {
            Id = partySearch.Id,
            PartyId = partySearch.PartyId,
            CreationTime = partySearch.CreationTime,
            LeaderAccountId = partySearch.LeaderAccountId,
            LeaderCharacterId = partySearch.LeaderCharacterId,
            LeaderName = partySearch.LeaderName,
            MemberCount = partySearch.MemberCount,
            Name = partySearch.Name,
            NoApproval = partySearch.NoApproval,
            Size = partySearch.Size,
        };
    }
}
