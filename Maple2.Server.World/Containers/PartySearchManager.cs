using Maple2.Model.Game.Party;
using ChannelClient = Maple2.Server.Channel.Service.Channel.ChannelClient;

namespace Maple2.Server.World.Containers;

public class PartySearchManager : IDisposable {
    public required ChannelClientLookup ChannelClients { get; init; }
    public readonly PartySearch PartySearch;
    public readonly Party Party;

    public PartySearchManager(PartySearch partySearch, Party party) {
        PartySearch = partySearch;
        Party = party;
    }

    public void Dispose() {
        Broadcast(new PartySearchRequest() {
            Remove = new PartySearchRequest.Types.Remove(),
            Id = PartySearch.Id,
        });
        Party.Search = null;
    }

    public void Set(long requestorId) {
        Broadcast(new PartySearchRequest {
            Set = new PartySearchRequest.Types.Set {
                PartySearch = new PartySearchInfo {
                    Id = PartySearch.Id,
                    PartyId = PartySearch.PartyId,
                    CreationTime = PartySearch.CreationTime,
                    LeaderAccountId = PartySearch.LeaderAccountId,
                    LeaderCharacterId = PartySearch.LeaderCharacterId,
                    LeaderName = PartySearch.LeaderName,
                    MemberCount = PartySearch.MemberCount,
                    Name = PartySearch.Name,
                    NoApproval = PartySearch.NoApproval,
                    Size = PartySearch.Size,
                },
                RequestorId = requestorId,
            },
        });
    }

    public void Remove() {
        Broadcast(new PartySearchRequest {
            Remove = new PartySearchRequest.Types.Remove(),
            Id = PartySearch.Id,
            PartyId = Party.Id,
        });
        Dispose();
    }

    public void Broadcast(PartySearchRequest request) {
        if (request.Id > 0 && request.Id != PartySearch.Id) {
            throw new InvalidOperationException($"Broadcasting {request.PartySearchCase} for incorrect party search: {request.Id} => {PartySearch.Id}");
        }

        request.PartyId = Party.Id;
        request.Id = PartySearch.Id;
        foreach (IGrouping<short, PartyMember> group in Party.Members.Values.GroupBy(member => member.Info.Channel)) {
            if (!ChannelClients.TryGetClient(group.Key, out ChannelClient? client)) {
                continue;
            }

            request.ReceiverIds.Clear();
            request.ReceiverIds.AddRange(group.Select(member => member.Info.CharacterId));

            try {
                client.PartySearch(request);
            } catch { }
        }
    }

}
