using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Autofac;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Game.Event;
using Maple2.PacketLib.Tools;
using Maple2.Model.Game.Shop;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Network;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game;

public class GameServer : Server<GameSession> {
    private readonly object mutex = new();
    private readonly FieldManager.Factory fieldFactory;
    private readonly HashSet<GameSession> connectingSessions;
    private readonly Dictionary<long, GameSession> sessions;
    private readonly ImmutableList<SystemBanner> bannerCache;
    private readonly ConcurrentDictionary<int, PremiumMarketItem> premiumMarketCache;
    private Dictionary<int, Shop> shopCache;
    private Dictionary<int, Dictionary<int, ShopItem>> shopItemCache;
    private readonly GameStorage gameStorage;

    public int Channel => Target.GameChannel;

    public GameServer(FieldManager.Factory fieldFactory, PacketRouter<GameSession> router, IComponentContext context, GameStorage gameStorage, ServerTableMetadataStorage serverTableMetadataStorage)
            : base(Target.GamePort, router, context, serverTableMetadataStorage) {
        this.fieldFactory = fieldFactory;
        connectingSessions = [];
        sessions = new Dictionary<long, GameSession>();
        this.gameStorage = gameStorage;

        using GameStorage.Request db = gameStorage.Context();
        bannerCache = db.GetBanners().ToImmutableList();
        shopCache = db.GetShops().ToDictionary(shop => shop.Id, shop => shop);
        shopItemCache = db.GetShopItems();
        premiumMarketCache = new ConcurrentDictionary<int, PremiumMarketItem>(
            db.GetPremiumMarketItems().Select(item => new KeyValuePair<int, PremiumMarketItem>(item.Id, item)));
    }

    public override void OnConnected(GameSession session) {
        lock (mutex) {
            connectingSessions.Remove(session);
            sessions[session.CharacterId] = session;
        }
    }

    public override void OnDisconnected(GameSession session) {
        lock (mutex) {
            connectingSessions.Remove(session);
            sessions.Remove(session.CharacterId);
        }
    }

    public bool GetSession(long characterId, [NotNullWhen(true)] out GameSession? session) {
        lock (mutex) {
            return sessions.TryGetValue(characterId, out session);
        }
    }

    public IEnumerable<GameSession> GetSessions() {
        lock (mutex) {
            return sessions.Values;
        }
    }

    protected override void AddSession(GameSession session) {
        lock (mutex) {
            connectingSessions.Add(session);
        }

        Logger.Information("Game client connecting: {Session}", session);
        session.Start();
    }

    public FieldManager? GetField(int mapId, int instanceId = 0) {
        return fieldFactory.Get(mapId, instanceId);
    }

    public GameEvent? FindEvent(GameEventType type) {
        return eventCache.Values.FirstOrDefault(gameEvent => gameEvent.Metadata.Type == type && gameEvent.IsActive());
    }

    public GameEvent? FindEvent(int eventId) {
        return eventCache.TryGetValue(eventId, out GameEvent? gameEvent) && gameEvent.IsActive() ? gameEvent : null;
    }

    public void AddEvent(GameEvent gameEvent) {
        if (!eventCache.TryAdd(gameEvent.Id, gameEvent)) {
            return;
        }

        foreach (GameSession session in sessions.Values) {
            session.Send(GameEventPacket.Add(gameEvent));
        }
    }

    public void RemoveEvent(int eventId) {
        if (!eventCache.Remove(eventId, out GameEvent? gameEvent)) {
            return;
        }

        foreach (GameSession session in sessions.Values) {
            session.Send(GameEventPacket.Remove(gameEvent.Id));
        }
    }

    public IEnumerable<GameEvent> GetEvents() => eventCache.Values.Where(gameEvent => gameEvent.IsActive());

    public Shop? FindShop(GameSession session, int shopId) {
        using GameStorage.Request db = gameStorage.Context();
        if (!shopCache.TryGetValue(shopId, out Shop? shop)) {
            shop = db.GetShop(shopId);
            if (shop != null) {
                shopCache[shopId] = shop;
            }
            if (shop?.RestockTime == 0) { // everything else would be player-based shops that would get refreshed
                if (!shopItemCache.TryGetValue(shopId, out Dictionary<int, ShopItem>? shopItems)) {
                    shopItems = db.GetShopItems(shopId).ToDictionary(item => item.Id);
                    shopItemCache[shopId] = shopItems;
                }
                foreach ((int shopItemId, ShopItem shopItem) in shopItems) {
                    Item? item = session.Field.ItemDrop.CreateItem(shopItem.ItemId, shopItem.Rarity, shopItem.Quantity);
                    if (item == null) {
                        continue;
                    }
                    shopItem.Item = item;
                    shop.Items[shopItem.Id] = shopItem;
                }
            } else {
                return shop;
            }
        }

        if (shop.Items.Count == 0 && shopItemCache.TryGetValue(shop.Id, out Dictionary<int, ShopItem>? items)) {
            foreach ((int shopItemId, ShopItem shopItem) in items) {
                Item? item = session.Field.ItemDrop.CreateItem(shopItem.ItemId, shopItem.Rarity, shopItem.Quantity);
                if (item == null) {
                    continue;
                }
                shopItem.Item = item;
                shop.Items[shopItem.Id] = shopItem;
            }
        }

        return shopCache[shopId];
    }

    public IList<ShopItem> FindShopItems(int shopId) {
        if (shopItemCache.TryGetValue(shopId, out Dictionary<int, ShopItem>? items)) {
            return items.Values.ToList();
        }

        using GameStorage.Request db = gameStorage.Context();
        return db.GetShopItems(shopId);
    }

    public IList<SystemBanner> GetSystemBanners() => bannerCache;

    public ICollection<PremiumMarketItem> GetPremiumMarketItems(params int[] tabIds) {
        if (tabIds.Length == 0) {
            return premiumMarketCache.Values;
        }

        return premiumMarketCache.Values.Where(item => tabIds.Contains(item.TabId)).ToList();
    }

    public PremiumMarketItem? GetPremiumMarketItem(int id, int subId) {
        if (subId == 0) {
            return premiumMarketCache.GetValueOrDefault(id);
        }

        return premiumMarketCache.TryGetValue(id, out PremiumMarketItem? item) ? item.AdditionalQuantities.FirstOrDefault(subItem => subItem.Id == subId) : null;
    }

    public void DailyReset() {
        foreach (GameSession session in sessions.Values) {
            session.DailyReset();
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken) {
        lock (mutex) {
            foreach (GameSession session in connectingSessions) {
                session.Send(NoticePacket.Disconnect(new InterfaceText("GameServer Maintenance")));
                session.Dispose();
            }
            foreach (GameSession session in sessions.Values) {
                session.Send(NoticePacket.Disconnect(new InterfaceText("GameServer Maintenance")));
                session.Dispose();
            }
            fieldFactory.Dispose();
        }

        return base.StopAsync(cancellationToken);
    }

    public void Broadcast(ByteWriter packet) {
        foreach (GameSession session in sessions.Values) {
            session.Send(packet);
        }
    }
}
