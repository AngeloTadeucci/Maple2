using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.World.Service;

namespace Maple2.Server.Game.Manager;

public sealed class BlackMarketManager {
    private readonly Lua.Lua lua;

    private readonly GameSession session;
    private Dictionary<long, BlackMarketListing> listings;
    public BlackMarketManager(GameSession session, Lua.Lua lua) {
        this.session = session;
        this.lua = lua;
        listings = new Dictionary<long, BlackMarketListing>();
    }

    public void LoadMyListings() {
        listings.Clear();
        using GameStorage.Request db = session.GameStorage.Context();
        ICollection<BlackMarketListing> entries = db.GetBlackMarketListings(session.CharacterId).ToList();
        foreach (BlackMarketListing entry in entries) {
            listings.Add(entry.Id, entry);
        }

        session.Send(BlackMarketPacket.MyListings(entries));
    }

    public void Add(long itemUid, long price, int quantity) {
        Item? item = session.Item.Inventory.Get(itemUid);
        if (item == null) {
            session.Send(BlackMarketPacket.Error(BlackMarketError.s_blackmarket_error_register_not_exist_in_inven));
            return;
        }

        if (item.Amount < quantity) {
            session.Send(BlackMarketPacket.Error(BlackMarketError.s_blackmarket_error_lack_sale_count));
            return;
        }

        if (item.IsExpired()) {
            return;
        }

        float depositPercent = lua.CalcBlackMarketRegisterDepositPercent();
        long depositFee = lua.CalcBlackMarketRegisterDeposit((long) (price * quantity * depositPercent));

        if (session.Currency.CanAddMeso(-depositFee) != -depositFee) {
            session.Send(BlackMarketPacket.Error(BlackMarketError.s_err_lack_meso));
            return;
        }


        if (!session.Item.Inventory.Remove(itemUid, out item, quantity)) {
            return;
        }

        var listing = new BlackMarketListing(item) {
            AccountId = session.AccountId,
            CharacterId = session.CharacterId,
            Deposit = depositFee,
            ExpiryTime = DateTime.Now.AddDays(Constant.BlackMarketSellEndDay).ToEpochSeconds(),
            Price = price,
            Quantity = quantity,
        };

        using GameStorage.Request db = session.GameStorage.Context();
        listing = db.CreateBlackMarketingListing(listing);
        if (listing == null) {
            session.Send(BlackMarketPacket.Error(BlackMarketError.s_blackmarket_error_fail_register));
            // TODO: Give item and deposit by mail.
            return;
        }

        // Also add to World
        BlackMarketResponse response = session.World.BlackMarket(new BlackMarketRequest {
            Add = new BlackMarketRequest.Types.Add {
                ListingId = listing.Id,
            },
        });

        var error = (BlackMarketError) response.Error;
        if (error != BlackMarketError.none) {
            session.Send(BlackMarketPacket.Error(error));
            // TODO: Give item and deposit by mail.
            return;
        }

        listings.Add(listing.Id, listing);
        session.Send(BlackMarketPacket.Add(listing));
    }

    public void Preview(int itemId, int rarity) {
        // We're not checking if the item is in the inventory or not as this is just for previewing. We'll check in Create.
        if (!session.ItemMetadata.TryGet(itemId, out ItemMetadata? metadata)) {
            return;
        }
        var type = new ItemType(metadata.Id);
        long sellPrice =  Core.Formulas.Shop.SellPrice(metadata, type, rarity);

        session.Send(BlackMarketPacket.Preview(itemId, rarity, sellPrice));
    }

    public void Search() {

    }
}
