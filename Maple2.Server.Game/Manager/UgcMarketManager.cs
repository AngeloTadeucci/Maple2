using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Serilog;

namespace Maple2.Server.Game.Manager;

public sealed class UgcMarketManager {
    private readonly GameSession session;

    private readonly IDictionary<long, UgcMarketItem> items;
    private IDictionary<long, SoldUgcMarketItem> sales;
    private IDictionary<long, PlayerInfo> favoriteDesigners;

    private readonly ILogger logger = Log.Logger.ForContext<UgcMarketManager>();

    public UgcMarketManager(GameSession session) {
        this.session = session;

        using GameStorage.Request db = session.GameStorage.Context();
        items = db.GetUgcListingsByAccountId(session.AccountId);
        sales = db.GetMySoldUgcListings(session.AccountId);
        favoriteDesigners = new Dictionary<long, PlayerInfo>();
        IList<long> designerList = session.Config.GetFavoriteDesigners();

        foreach (long characterId in designerList) {
            if (!session.PlayerInfo.GetOrFetch(characterId, out PlayerInfo? playerInfo)) {
                continue;
            }
            favoriteDesigners.Add(characterId, playerInfo);
        }
    }

    public void Load() {
        session.Send(MeretMarketPacket.LoadPersonalListings(items.Values));
    }

    public void RefreshSales() {
        using GameStorage.Request db = session.GameStorage.Context();
        Save(db);
        sales.Clear();
        sales = db.GetMySoldUgcListings(session.AccountId);
        session.Send(MeretMarketPacket.LoadSales(sales.Values));
    }

    public void ListItem(UgcMarketItem item) {
        items.Add(item.Id, item);
        session.Send(MeretMarketPacket.ListItem(item));
        session.Send(MeretMarketPacket.UpdateExpiration(item));
    }

    public void CollectProfit(long id) {
        if (!sales.TryGetValue(id, out SoldUgcMarketItem? sale) ||
            sale.SoldTime.FromEpochSeconds().AddDays(Constant.UGCShopProfitDelayDays) > DateTime.Now) {
            return;
        }
        using GameStorage.Request db = session.GameStorage.Context();
        if (!db.DeleteSoldUgcMarketItem(id)) {
            logger.Error("Unable to collect profit and delete SoldUgcMarketItem {Id}.", id);
            return;
        }
        sales.Remove(id);

        session.Currency.Meret += sale.Profit;
        session.Send(MeretMarketPacket.UpdateProfit(sale.Id));
    }

    public void RemoveListing(long id) {
        if (!items.ContainsKey(id)) {
            return;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        if (!db.DeleteUgcMarketItem(id)) {
            return;
        }

        items.Remove(id);
        session.Send(MeretMarketPacket.RemoveListing(id));
    }

    public void UnlistItem(long id) {
        if (!items.TryGetValue(id, out UgcMarketItem? item)) {
            return;
        }

        item.ListingEndTime = 0;
        item.PromotionEndTime = 0;
        item.Status = UgcMarketListingStatus.Expired;

        using GameStorage.Request db = session.GameStorage.Context();
        if (!db.SaveUgcMarketItem(item)) {
            return;
        }

        session.Send(MeretMarketPacket.UpdateExpiration(item));
    }

    public void AddDesigner(long characterId) {
        if (favoriteDesigners.ContainsKey(characterId)) {
            LoadDesignerItems(characterId);
            return;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        if (!session.PlayerInfo.GetOrFetch(characterId, out PlayerInfo? info)) {
            return;
        }

        favoriteDesigners.Add(characterId, info);
        session.Config.AddFavoriteDesigner(characterId);
        IList<UgcMarketItem> designerItems = db.GetUgcListingsByCharacterId(characterId);
        session.Send(MeretMarketPacket.AddDesigner(info, designerItems));
    }

    public void RemoveDesigner(long characterId) {
        if (!favoriteDesigners.Remove(characterId)) {
            return;
        }
        session.Config.RemoveFavoriteDesigner(characterId);

        session.Send(MeretMarketPacket.RemoveDesigner(characterId));
    }

    public void LoadDesigners() {
        session.Send(MeretMarketPacket.LoadDesigners(favoriteDesigners.Values));
    }

    /// <summary>
    /// Fetch the designers' active listings.
    /// </summary>
    public void LoadDesignerItems(long characterId) {
        if (!favoriteDesigners.TryGetValue(characterId, out PlayerInfo? playerInfo)) {
            return;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        IList<UgcMarketItem> designerItems = db.GetUgcListingsByCharacterId(characterId);

        session.Send(MeretMarketPacket.LoadDesignerItems(playerInfo, designerItems));
    }

    public bool TryGetItem(long id, [NotNullWhen(true)] out UgcMarketItem? item) {
        return items.TryGetValue(id, out item);
    }

    public void Save(GameStorage.Request db) {
        db.SaveUgcMarketItems(items.Values);
        db.SaveSoldUgcMarketItems(sales.Values);
    }
}
