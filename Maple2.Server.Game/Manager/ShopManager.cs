using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Shop;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Game.Manager;

public sealed class ShopManager {
    #region EntryId
    private int idCounter;

    /// <summary>
    /// Generates an EntryId unique to this specific manager instance.
    /// </summary>
    /// <returns>Returns a local EntryId</returns>
    private int NextEntryId() => Interlocked.Increment(ref idCounter);
    #endregion
    private readonly GameSession session;
    // TODO: the CharacterShopData's restock count need to be reset at daily reset.
    private IDictionary<int, CharacterShopData> accountShopData;
    private IDictionary<int, CharacterShopData> characterShopData;
    private IDictionary<int, Shop> instancedShops;
    private IDictionary<int, IDictionary<int, CharacterShopItemData>> characterShopItemData;
    private IDictionary<int, IDictionary<int, CharacterShopItemData>> accountShopItemData;
    private IDictionary<int, BuyBackItem> buyBackItems;
    private Shop? activeShop;
    private int shopNpcId;

    private readonly ILogger logger = Log.Logger.ForContext<ShopManager>();

    public ShopManager(GameSession session) {
        this.session = session;
        instancedShops = new Dictionary<int, Shop>();
        buyBackItems = new Dictionary<int, BuyBackItem>();
        using GameStorage.Request db = session.GameStorage.Context();

        accountShopData = db.GetCharacterShopData(session.AccountId);
        characterShopData = db.GetCharacterShopData(session.CharacterId);

        accountShopItemData = new Dictionary<int, IDictionary<int, CharacterShopItemData>>();
        ICollection<CharacterShopItemData> accountShopItemList = db.GetCharacterShopItemData(session.AccountId);
        foreach (CharacterShopItemData itemData in accountShopItemList) {
            if (accountShopItemData.TryGetValue(itemData.ShopId, out IDictionary<int, CharacterShopItemData>? itemDictionary)) {
                if (itemDictionary.ContainsKey(itemData.ShopItemId)) {
                    continue;
                }
                itemDictionary[itemData.ShopItemId] = itemData;
                continue;
            }
            accountShopItemData[itemData.ShopId] = new Dictionary<int, CharacterShopItemData> {
                [itemData.ShopItemId] = itemData,
            };
        }

        characterShopItemData = new Dictionary<int, IDictionary<int, CharacterShopItemData>>();
        ICollection<CharacterShopItemData> characterShopItemList = db.GetCharacterShopItemData(session.CharacterId);
        foreach (CharacterShopItemData itemData in characterShopItemList) {
            if (characterShopItemData.TryGetValue(itemData.ShopId, out IDictionary<int, CharacterShopItemData>? itemDictionary)) {
                if (itemDictionary.ContainsKey(itemData.ShopItemId)) {
                    continue;
                }
                itemDictionary[itemData.ShopItemId] = itemData;
                continue;
            }
            characterShopItemData[itemData.ShopId] = new Dictionary<int, CharacterShopItemData> {
                [itemData.ShopItemId] = itemData,
            };
        }
    }

    public void ClearActiveShop() {
        activeShop = null;
        shopNpcId = 0;
    }

    private bool TryGetShopData(int shopId, [NotNullWhen(true)] out CharacterShopData? data) {
        return accountShopData.TryGetValue(shopId, out data) || characterShopData.TryGetValue(shopId, out data);
    }

    private bool TryGetShopItemData(int shopId, [NotNullWhen(true)] out IDictionary<int, CharacterShopItemData>? data) {
        return accountShopItemData.TryGetValue(shopId, out data) || characterShopItemData.TryGetValue(shopId, out data);
    }

    public void Load(int shopId, int npcId = 0) {
        Shop? shop = session.FindShop(shopId);
        if (shop == null) {
            logger.Warning("Shop {Id} has not been implemented", shopId);
            return;
        }

        if (shop.RestockData != null) {
            shop = GetInstancedShop(shop);
        }

        activeShop = shop;
        shopNpcId = npcId;

        session.Send(ShopPacket.Open(activeShop, shopNpcId));
        session.Send(ShopPacket.LoadItems(shop.Items.Values));
        if (!shop.DisableBuyback) {
            session.Send(ShopPacket.BuyBackItemCount((short) buyBackItems.Count));
            if (buyBackItems.Count > 0) {
                session.Send(ShopPacket.LoadBuyBackItem((BuyBackItem[]) buyBackItems.Values));
            }
        }
    }

    private Shop GetInstancedShop(Shop serverShop) {
        if (instancedShops.TryGetValue(serverShop.Id, out Shop? instancedShop)) {
            if (instancedShop.RestockTime < DateTime.Now.ToEpochSeconds()) {
            }
            return instancedShop;
        }


        if (!TryGetShopData(serverShop.Id, out CharacterShopData? data)) {
            long restockTime = GetRestockTime(serverShop.RestockData!.Interval);
            if (serverShop.RestockTime > 0) {
                restockTime = serverShop.RestockTime;
            }
            return CreateInstancedShop(serverShop, CreateShopData(serverShop), restockTime);

        }
        if (data.RestockTime < DateTime.Now.ToEpochSeconds()) {
            return CreateInstancedShop(serverShop, CreateShopData(serverShop), GetRestockTime(serverShop.RestockData!.Interval));
        }

        // Assemble shop
        instancedShop = serverShop.Clone();
        instancedShop.RestockTime = data.RestockTime;

        if (!TryGetShopItemData(instancedShop.Id, out IDictionary<int, CharacterShopItemData>? dataDictionary)) {
            dataDictionary = CreateShopItemData(serverShop);
        }

        foreach ((int shopItemId, CharacterShopItemData itemData) in dataDictionary) {
            if (!serverShop.Items.TryGetValue(shopItemId, out ShopItem? shopItem)) {
                continue;
            }

            ShopItem itemCopy = shopItem.Clone();
            itemCopy.Item = itemData.Item;
            itemCopy.StockPurchased = itemData.StockPurchased;
            instancedShop.Items.Add(shopItemId, itemCopy);
        }

        instancedShops[instancedShop.Id] = instancedShop;
        return instancedShop;
    }

    private Shop CreateInstancedShop(Shop shop, CharacterShopData shopData, long restockTime) {
        Shop instancedShop = shop.Clone()!;

        // First delete any existing shop item data
        long ownerId = shop.RestockData!.PersistantInventory ? session.AccountId : session.CharacterId;
        using GameStorage.Request db = session.GameStorage.Context();

        if (TryGetShopItemData(shop.Id, out IDictionary<int, CharacterShopItemData>? shopItemDatas)) {
            foreach ((int shopItemId, CharacterShopItemData itemData) in shopItemDatas) {
                db.DeleteCharacterShopItemData(ownerId, shopItemId);
            }
        }

        // Create new shop data
        instancedShop.Items = GetShopItems(shop);
        instancedShop.RestockTime = restockTime;
        instancedShop.RestockData!.RestockCount = shopData.RestockCount;
        CreateShopItemData(instancedShop);
        instancedShops[instancedShop.Id] = instancedShop;
        return instancedShop;
    }

    private SortedDictionary<int, ShopItem> GetShopItems(Shop shop) {
        var items = new SortedDictionary<int, ShopItem>();
        using GameStorage.Request db = session.GameStorage.Context();
        IEnumerable<ShopItem> itemList;
        if (shop.RestockData?.Interval == ShopRestockInterval.Minute) {
            itemList = session.FindShopItems(shop.Id).OrderBy(_ => Random.Shared.Next()).Take(12);
        } else {
            itemList = shop.Items.Values;
        }

        foreach (ShopItem shopItem in itemList) {
            Item? item = session.Field.ItemDrop.CreateItem(shopItem.ItemId, shopItem.Rarity, shopItem.Quantity);
            if (item == null) {
                continue;
            }
            shopItem.Item = item;
            items.Add(shopItem.Id, shopItem.Clone());
        }

        return items;
    }

    private CharacterShopData CreateShopData(Shop shop) {
        if (!TryGetShopData(shop.Id, out CharacterShopData? data)) {
            data = new CharacterShopData {
                Interval = shop.RestockData!.Interval,
                RestockTime = GetRestockTime(shop.RestockData!.Interval),
                ShopId = shop.Id,
            };

            if (shop.RestockData.PersistantInventory) {
                accountShopData[shop.Id] = data;
            } else {
                characterShopData[shop.Id] = data;
            }
        }

        shop.RestockTime = data.RestockTime;

        // We do not save shops with minute intervals to database
        if (shop.RestockData!.Interval != ShopRestockInterval.Minute) {
            using GameStorage.Request db = session.GameStorage.Context();
            long ownerId = shop.RestockData.PersistantInventory ? session.AccountId : session.CharacterId;
            data = db.CreateCharacterShopData(ownerId, data);
            if (data == null) {
                throw new InvalidOperationException($"Failed to create shop data {shop.Id} for character {session.CharacterId}");
            }
        }

        return data;
    }

    private IDictionary<int, CharacterShopItemData> CreateShopItemData(Shop shop) {
        Dictionary<int, CharacterShopItemData> itemData = new Dictionary<int, CharacterShopItemData>();
        using GameStorage.Request db = session.GameStorage.Context();
        long ownerId = shop.RestockData!.PersistantInventory ? session.AccountId : session.CharacterId;
        foreach ((int id, ShopItem item) in shop.Items) {
            var data = new CharacterShopItemData {
                ShopId = shop.Id,
                ShopItemId = id,
                Item = item.Item,
            };
            if (shop.RestockData.Interval != ShopRestockInterval.Minute) {
                data = db.CreateCharacterShopItemData(ownerId, data);
                if (data == null) {
                    continue;
                }
            }
            itemData[id] = data;
        }

        if (shop.RestockData?.PersistantInventory == true) {
            accountShopItemData[shop.Id] = itemData;
            return accountShopItemData[shop.Id];
        }
        characterShopItemData[shop.Id] = itemData;
        return characterShopItemData[shop.Id];
    }

    public void InstantRestock() {
        if (activeShop?.RestockData?.DisableInstantRestock != false) {
            return;
        }

        Shop? shop = session.FindShop(activeShop.Id);
        if (shop == null) {
            return;
        }

        int cost = activeShop.RestockData.Cost;
        ShopCurrencyType currencyType = activeShop.RestockData.CurrencyType;
        if (activeShop.RestockData.EnableCostMultiplier) {
            (ShopCurrencyType CurrencyType, int Cost) multiplierCost = Core.Formulas.Shop.ExcessRestockCost(activeShop.RestockData);
            cost = multiplierCost.Cost;
            currencyType = multiplierCost.CurrencyType;
        }
        if (!Pay(new ShopCost {
            Amount = cost,
            Type = currencyType,
        }, activeShop.RestockData.Cost)) {
            return;
        }

        if (!TryGetShopData(shop.Id, out CharacterShopData? data)) {
            data = CreateShopData(shop);
        }
        data.RestockTime = GetInstantRestockTime(activeShop.RestockData!.Interval);
        data.RestockCount++;
        activeShop = CreateInstancedShop(shop, data, data.RestockTime);
        session.Send(ShopPacket.InstantRestock());
        session.Send(ShopPacket.Open(activeShop, shopNpcId));
        session.Send(ShopPacket.LoadItems(activeShop.Items.Values));
    }

    public void Refresh() {
        if (activeShop?.RestockData == null) {
            return;
        }

        Shop? shop = session.FindShop(activeShop.Id);
        if (shop == null) {
            return;
        }

        if (!TryGetShopData(shop.Id, out CharacterShopData? data)) {
            data = CreateShopData(shop);
        }
        data.RestockTime = GetInstantRestockTime(activeShop.RestockData!.Interval);
        activeShop = CreateInstancedShop(shop, data, data.RestockTime);
        session.Send(ShopPacket.Open(activeShop, shopNpcId));
        session.Send(ShopPacket.LoadItems(activeShop.Items.Values));
    }

    private void UpdateStockCount(int shopId, ShopItem shopItem, int quantity) {
        if (!TryGetShopItemData(shopId, out IDictionary<int, CharacterShopItemData>? data) ||
            !data.TryGetValue(shopItem.Id, out CharacterShopItemData? itemData)) {
            return;
        }
        itemData.StockPurchased += quantity;
        shopItem.StockPurchased += quantity;
    }

    private long GetRestockTime(ShopRestockInterval interval) {
        // Follows a time schedule
        DateTime now = DateTime.Now;
        switch (interval) {
            case ShopRestockInterval.Minute:
                return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1).ToEpochSeconds();
            case ShopRestockInterval.Day:
                return new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(1).ToEpochSeconds();
            case ShopRestockInterval.Week:
                return DateTime.Now.NextDayOfWeek(Constant.ResetDay).Date.ToEpochSeconds();
            case ShopRestockInterval.Month:
                return new DateTime(now.Year, now.Month, 1, 0, 0, 0).AddMonths(1).ToEpochSeconds();
            default:
                logger.Error("Unknown restock interval {Interval}", interval);
                return long.MaxValue;
        }
    }

    private long GetInstantRestockTime(ShopRestockInterval interval) {
        return interval switch {
            ShopRestockInterval.Minute => DateTime.Now.AddMinutes(1).ToEpochSeconds(),
            ShopRestockInterval.Day => DateTime.Now.AddDays(1).ToEpochSeconds(),
            ShopRestockInterval.Week => DateTime.Now.AddDays(7).ToEpochSeconds(),
            ShopRestockInterval.Month => DateTime.Now.AddMonths(1).ToEpochSeconds(),
            _ => long.MaxValue,
        };
    }


    public void Buy(int shopItemId, int quantity) {
        if (activeShop == null) {
            return;
        }

        if (!activeShop.Items.TryGetValue(shopItemId, out ShopItem? shopItem)) {
            session.Send(ShopPacket.Error(ShopError.s_err_invalid_item));
            return;
        }


        if (shopItem.RestrictedBuyData != null) {
            RestrictedBuyData buyData = shopItem.RestrictedBuyData;
            if (DateTime.Now.ToEpochSeconds() < buyData.StartTime || DateTime.Now.ToEpochSeconds() > buyData.EndTime) {
                session.Send(ShopPacket.Error(ShopError.s_err_invalid_item_cannot_buy_by_period));
                return;
            }

            if (buyData.TimeRanges.Count > 0) {
                int totalSeconds = (int) new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second).TotalSeconds;
                if (!buyData.TimeRanges.Any(time => time.StartTimeOfDay > totalSeconds || time.EndTimeOfDay < totalSeconds)) {
                    session.Send(ShopPacket.Error(ShopError.s_err_invalid_item_cannot_buy_by_period));
                    return;
                }
            }

            if (buyData.Days.Count > 0) {
                if (!buyData.Days.Contains(ToShopBuyDay(DateTime.Now.DayOfWeek))) {
                    session.Send(ShopPacket.Error(ShopError.s_err_invalid_item_cannot_buy_by_period));
                    return;
                }
            }
        }

        if (quantity > shopItem.StockCount - shopItem.StockPurchased) {
            session.Send(ShopPacket.Error(ShopError.s_err_lack_shopitem));
            return;
        }

        if (shopItem.RequireAchievementId > 0 && !session.Achievement.HasAchievement(shopItem.RequireAchievementId, shopItem.RequireAchievementRank)) {
            return;
        }

        if (shopItem.RequireGuildTrophy > 0 && session.Guild.Guild != null && session.Guild.Guild.AchievementInfo.Total < shopItem.RequireGuildTrophy) {
            session.Send(ShopPacket.Error(ShopError.s_err_lack_guild_trophy));
            return;
        }

        // TODO: Guild Merchant Type/Level, Championship, Alliance

        Item item = shopItem.Item.Clone();
        item.Amount = quantity;
        if (!session.Item.Inventory.CanAdd(item)) {
            session.Send(ShopPacket.Error(ShopError.s_err_inventory));
            return;
        }

        int price = shopItem.Cost.SaleAmount > 0 ? shopItem.Cost.SaleAmount * quantity : shopItem.Cost.Amount * quantity;
        if (!Pay(shopItem.Cost, price)) {
            return;
        }

        if (activeShop.RestockData != null && shopItem.StockCount > 0) {
            UpdateStockCount(activeShop.Id, shopItem, quantity);
            session.Send(ShopPacket.Update(shopItem.Id, quantity * shopItem.StockPurchased));
        }

        session.Item.Inventory.Add(item, true);
        session.Send(ShopPacket.Buy(shopItem, quantity, price));
    }

    public void PurchaseBuyBack(int id) {
        if (activeShop == null) {
            return;
        }

        if (!buyBackItems.TryGetValue(id, out BuyBackItem? buyBackItem)) {
            session.Send(ShopPacket.Error(ShopError.s_err_invalid_item));
            return;
        }

        if (!session.Item.Inventory.CanAdd(buyBackItem.Item)) {
            session.Send(ShopPacket.Error(ShopError.s_err_inventory));
            return;
        }

        if (!Pay(new ShopCost {
            Type = ShopCurrencyType.Meso,
            Amount = (int) buyBackItem.Price,
        }, (int) buyBackItem.Price)) {
            return;
        }

        if (!buyBackItems.Remove(id)) {
            logger.Error("Failed to remove buyback item {Id}", id);
            return;
        }
        session.Item.Inventory.Add(buyBackItem.Item, true);
        session.Send(ShopPacket.RemoveBuyBackItem(id));
    }

    public void Sell(long itemUid, int quantity) {
        if (activeShop == null) {
            return;
        }

        if (activeShop.DisableBuyback) {
            session.Send(ShopPacket.Error(ShopError.s_msg_cant_sell_to_only_sell_shop));
            return;
        }

        Item? item = session.Item.Inventory.Get(itemUid);
        if (item == null || !item.Metadata.Limit.ShopSell) {
            return;
        }

        if (!session.Item.Inventory.Remove(item.Uid, out item, quantity)) {
            logger.Error("Failed to remove item {Uid} from inventory", itemUid);
            return;
        }

        long sellPrice = Core.Formulas.Shop.SellPrice(item.Metadata, item.Type, item.Rarity);
        if (session.Currency.CanAddMeso(sellPrice) != sellPrice) {
            logger.Error("Could not add {sellPrice} meso(s) to player {CharacterId}", sellPrice, session.CharacterId);
            return;
        }

        session.Currency.Meso += sellPrice;

        if (buyBackItems.Count >= Constant.MaxBuyBackItems && !RemoveBuyBackItem()) {
            return;
        }

        int entryId = NextEntryId();
        buyBackItems[entryId] = new BuyBackItem {
            Id = entryId,
            Item = item,
            AddedTime = DateTime.Now.ToEpochSeconds(),
            Price = sellPrice,
        };

        session.Send(ShopPacket.LoadBuyBackItem(buyBackItems[entryId]));
    }

    private bool Pay(ShopCost cost, int price) {
        switch (cost.Type) {
            case ShopCurrencyType.Meso:
                if (session.Currency.CanAddMeso(-price) != -price) {
                    session.Send(ShopPacket.Error(ShopError.s_err_lack_meso));
                    return false;
                }
                session.Currency.Meso -= price;
                break;
            case ShopCurrencyType.Meret:
            case ShopCurrencyType.EventMeret:
                if (session.Currency.CanAddMeret(-price) != -price) {
                    session.Send(ShopPacket.Error(ShopError.s_err_lack_merat));
                    return false;
                }

                session.Currency.Meret -= price;
                break;
            case ShopCurrencyType.GameMeret:
                if (session.Currency.CanAddGameMeret(-price) != -price) {
                    session.Send(ShopPacket.Error(ShopError.s_err_lack_merat));
                    return false;
                }

                session.Currency.Meret -= price;
                break;
            case ShopCurrencyType.Item:
                var ingredient = new ItemComponent(cost.ItemId, -1, price, ItemTag.None);
                if (!session.Item.Inventory.ConsumeItemComponents(new[] {
                        ingredient
                    })) {
                    session.Send(ShopPacket.Error(ShopError.s_err_lack_payment_item, cost.ItemId));
                    return false;
                }
                break;
            default:
                CurrencyType currencyType = cost.Type switch {
                    ShopCurrencyType.ValorToken => CurrencyType.ValorToken,
                    ShopCurrencyType.Treva => CurrencyType.Treva,
                    ShopCurrencyType.Rue => CurrencyType.Rue,
                    ShopCurrencyType.HaviFruit => CurrencyType.HaviFruit,
                    ShopCurrencyType.StarPoint => CurrencyType.StarPoint,
                    ShopCurrencyType.MenteeToken => CurrencyType.MenteeToken,
                    ShopCurrencyType.MentorToken => CurrencyType.MentorToken,
                    ShopCurrencyType.MesoToken => CurrencyType.MesoToken,
                    ShopCurrencyType.ReverseCoin => CurrencyType.ReverseCoin,
                    _ => CurrencyType.None,

                };
                if (currencyType == CurrencyType.None || session.Currency[currencyType] < price) {
                    return false;
                }

                session.Currency[currencyType] -= cost.Amount;
                break;
        }
        return true;
    }

    private bool RemoveBuyBackItem() {
        while (buyBackItems.Count >= Constant.MaxBuyBackItems) {
            BuyBackItem? item = buyBackItems.Values.MinBy(entry => entry.AddedTime);
            if (item == null) {
                logger.Error("Failed to remove buyback item");
                return false;
            }

            buyBackItems.Remove(item.Id);
            session.Item.Inventory.Discard(item.Item);
            session.Send(ShopPacket.RemoveBuyBackItem(item.Id));
        }
        return true;
    }

    public void Save(GameStorage.Request db) {
        foreach (BuyBackItem item in buyBackItems.Values) {
            session.Item.Inventory.Discard(item.Item);
        }

        db.SaveCharacterShopData(session.AccountId, accountShopData.Values.Where(value => value.Interval != ShopRestockInterval.Minute).ToList());
        db.SaveCharacterShopData(session.CharacterId, characterShopData.Values.Where(value => value.Interval != ShopRestockInterval.Minute).ToList());
        db.SaveCharacterShopItemData(session.AccountId, accountShopItemData.Values.SelectMany(value => value.Values).ToList());
        db.SaveCharacterShopItemData(session.CharacterId, characterShopItemData.Values.SelectMany(value => value.Values).ToList());
    }

    private ShopBuyDay ToShopBuyDay(DayOfWeek dayOfWeek) {
        return dayOfWeek switch {
            DayOfWeek.Monday => ShopBuyDay.Monday,
            DayOfWeek.Tuesday => ShopBuyDay.Tuesday,
            DayOfWeek.Wednesday => ShopBuyDay.Wednesday,
            DayOfWeek.Thursday => ShopBuyDay.Thursday,
            DayOfWeek.Friday => ShopBuyDay.Friday,
            DayOfWeek.Saturday => ShopBuyDay.Saturday,
            DayOfWeek.Sunday => ShopBuyDay.Sunday,
            _ => throw new InvalidDataException("Invalid day of week"),
        };
    }
}
