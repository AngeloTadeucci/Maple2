using Grpc.Core;
using Maple2.Server.Channel.Service;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<BlackMarketResponse> BlackMarket(BlackMarketRequest request, ServerCallContext context) {
        switch (request.BlackMarketCase) {
            case BlackMarketRequest.BlackMarketOneofCase.PurchaseResponse:
                return Task.FromResult(PurchaseResponse(request.PurchaseResponse));
            default:
                return Task.FromResult(new BlackMarketResponse());
        }
    }

    private BlackMarketResponse PurchaseResponse(BlackMarketRequest.Types.PurchaseResponse purchaseResponse) {
        if (server.GetSession(purchaseResponse.SellerId, out GameSession? session)) {
            session.Send(BlackMarketPacket.PurchaseResponse());
        }
        return new BlackMarketResponse();
    }
}

