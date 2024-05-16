using Grpc.Core;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Party;
using Maple2.Server.Channel.Service;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<PartySearchResponse> PartySearch(PartySearchRequest request, ServerCallContext context) {
        switch (request.PartySearchCase) {
            case PartySearchRequest.PartySearchOneofCase.Set:
                return Task.FromResult(Set(request.PartyId, request.ReceiverIds, request.Set));
            case PartySearchRequest.PartySearchOneofCase.Remove:
                return Task.FromResult(Remove(request.PartyId, request.ReceiverIds, request.Remove));
            default:
                return Task.FromResult(new PartySearchResponse { Error = (int) PartySearchError.s_partysearch_err_server_db });
        }
    }

    private PartySearchResponse Set(int partyId, IEnumerable<long> receiverIds, PartySearchRequest.Types.Set set) {
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                return new PartySearchResponse { Error = (int) PartySearchError.s_partysearch_err_server_db };
            }

            if (partyId != session.Party.Party?.Id) {
                return new PartySearchResponse { Error = (int) PartySearchError.s_partysearch_err_server_db };
            }

            PartySearchInfo info = set.PartySearch;
            session.Party.SetPartySearch(new PartySearch(info.Id, info.Name, info.Size) {
                PartyId = partyId,
                LeaderAccountId = info.LeaderAccountId,
                LeaderCharacterId = info.LeaderCharacterId,
                LeaderName = info.LeaderName,
                MemberCount = info.MemberCount,
                NoApproval = info.NoApproval,
            });

            if (session.Party.Party.Search == null) {
                return new PartySearchResponse { Error = (int) PartySearchError.s_partysearch_err_server_db };
            }

            if (receiverId == set.RequestorId) {
                session.Send(PartySearchPacket.Add(session.Party.Party.Search));
            }
        }

        return new PartySearchResponse();
    }

    private PartySearchResponse Remove(int partyId, IEnumerable<long> receiverIds, PartySearchRequest.Types.Remove remove) {
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                return new PartySearchResponse {
                    Error = (int) PartySearchError.s_partysearch_err_server_db
                };
            }

            if (partyId != session.Party.Party?.Id) {
                return new PartySearchResponse {
                    Error = (int) PartySearchError.s_partysearch_err_server_db
                };
            }

            session.Party.SetPartySearch(null);
        }
        return new PartySearchResponse();
    }
}

