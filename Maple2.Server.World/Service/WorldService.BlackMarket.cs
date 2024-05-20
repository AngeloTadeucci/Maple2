using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;


namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<BlackMarketResponse> BlackMarket(BlackMarketRequest request, ServerCallContext context) {
        switch (request.BlackMarketCase) {
            case BlackMarketRequest.BlackMarketOneofCase.Search:
                return Task.FromResult(Search(request.Search));
            case BlackMarketRequest.BlackMarketOneofCase.Add:
                return Task.FromResult(Add(request.Add));
            default:
                return Task.FromResult(new BlackMarketResponse());
        }
    }

    private BlackMarketResponse Search(BlackMarketRequest.Types.Search search) {
         IList<StatOption>? sd = search.StatOptions;

         Dictionary<BasicAttribute, BasicOption> basicOptions = new();
         Dictionary<SpecialAttribute, SpecialOption> specialOptions = new();
         if (search.StatOptions != null) {
             foreach (StatOption statOption in search.StatOptions) {
                 switch (statOption.StatId) {
                     case >= 1000 and < 11000: // BasicAttribute with percent value
                         basicOptions[(BasicAttribute) (statOption.StatId - 1000)] = new BasicOption((float) (statOption.Value + 5) / 10000);
                         break;
                     case >= 11000: // SpecialAttribute with percent value
                         specialOptions[(SpecialAttribute) (statOption.StatId - 11000)] = new SpecialOption((float) (statOption.Value + 5) / 10000);
                         break;
                     default: // BasicAttribute with flat value
                         basicOptions[(BasicAttribute) statOption.StatId] = new BasicOption(statOption.Value);
                         break;
                 }
             }
         }

         ICollection<long> listingIds = blackMarketLookup.Search(search.Categories.ToArray(), search.MinLevel, search.MaxLevel, (JobFilterFlag) search.JobFlag, search.Rarity, search.MinEnchantLevel,
             search.MaxEnchantLevel, search.MinSocketCount, search.MaxSocketCount, search.Name, search.StartPage, (BlackMarketSort) search.SortBy, basicOptions, specialOptions);



        return new BlackMarketResponse {
            Search = new BlackMarketResponse.Types.Search {
                ListingIds = { listingIds },
            }
        };
    }

    private static void ReadStat(Dictionary<BasicAttribute, BasicOption> basicOptions, Dictionary<SpecialAttribute, SpecialOption> specialOptions, int statId, int value) {

    }

    private BlackMarketResponse Add(BlackMarketRequest.Types.Add add) {
        BlackMarketError error = blackMarketLookup.Add(add.ListingId);
        return new BlackMarketResponse {
            Error = (int) error,
        };
    }
}
