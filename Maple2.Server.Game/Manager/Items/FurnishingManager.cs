using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Game.Manager.Items;

public class FurnishingManager {
    private static long _cubeIdCounter = Constant.FurnishingBaseId;
    public static long NextCubeId() => Interlocked.Increment(ref _cubeIdCounter);

    private readonly GameSession session;

    private readonly ItemCollection storage;
    private readonly ConcurrentDictionary<long, PlotCube> inventory;

    private readonly ILogger logger = Log.Logger.ForContext<FurnishingManager>();

    public FurnishingManager(GameStorage.Request db, GameSession session) {
        this.session = session;
        storage = new ItemCollection(Constant.FurnishingStorageMaxSlot);
        inventory = new ConcurrentDictionary<long, PlotCube>();

        List<Item>? items = db.GetItemGroups(session.AccountId, ItemGroup.Furnishing).GetValueOrDefault(ItemGroup.Furnishing);
        if (items == null) {
            return;
        }

        foreach (Item item in items) {
            if (storage.Add(item).Count == 0) {
                Log.Error("Failed to add furnishing:{Uid}", item.Uid);
            }
        }

        foreach (PlotCube cube in db.LoadCubesForOwner(session.AccountId)) {
            inventory[cube.Id] = cube;
        }
    }

    public void Load() {
        lock (session.Item) {
            Item[] items = storage.Where(item => item.Amount > 0).ToArray();
            session.Send(FurnishingStoragePacket.Count(items.Length));
            session.Send(FurnishingStoragePacket.StartList());
            foreach (Item item in items) {
                session.Send(FurnishingStoragePacket.Add(item));
            }
            session.Send(FurnishingStoragePacket.EndList());

            // FurnishingInventory
            session.Send(FurnishingInventoryPacket.StartList());
            foreach (PlotCube cube in inventory.Values) {
                session.Send(FurnishingInventoryPacket.Add(cube));
            }
            session.Send(FurnishingInventoryPacket.EndList());
        }
    }

    public Item? GetCube(long itemUid) {
        lock (session.Item) {
            return storage.Get(itemUid);
        }
    }

    public Item? GetItem(int itemId) {
        lock (session.Item) {
            return storage.FirstOrDefault(item => item.Id == itemId);
        }
    }

    /// <summary>
    /// Places a cube of the specified item uid at the requested location.
    /// If there are no amount remaining, we still keep the entry to allow reuse of the item uid.
    /// </summary>
    /// <param name="uid">Uid of the item to withdraw from</param>
    /// <param name="cube">The cube to be placed</param>
    /// <returns>Information about the withdrawn cube</returns>
    public bool TryAddCube(long uid, PlotCube cube) {
        const int amount = 1;
        lock (session.Item) {
            if (session.Field == null) {
                return false;
            }

            Item? item = storage.Get(uid);
            if (item == null || item.Amount < amount) {
                return false;
            }

            // We do not remove item from inventory even if it hits 0
            // this allows us to preserve the uid for later.
            item.Amount -= amount;
            session.Send(item.Amount > 0
                ? FurnishingStoragePacket.Update(item.Uid, item.Amount)
                : FurnishingStoragePacket.Remove(item.Uid));

            if (!AddInventory(cube)) {
                logger.Fatal("Failed to add cube: {CubeId} to inventory", cube.Id);
                throw new InvalidOperationException($"Failed to add cube: {cube.Id} to inventory");
            }

            return true;
        }
    }

    public bool PurchaseCube(FurnishingShopTable.Entry furnishingShopMetadata) {
        if (!furnishingShopMetadata.Buyable) {
            return false;
        }

        if (furnishingShopMetadata.Price <= 0) {
            return true;
        }

        lock (session.Item) {
            long negAmount;
            switch (furnishingShopMetadata.FurnishingTokenType) {
                case FurnishingCurrencyType.Meso:
                    negAmount = -furnishingShopMetadata.Price;
                    if (session.Currency.CanAddMeso(negAmount) != negAmount) {
                        session.Send(CubePacket.Error(UgcMapError.s_err_ugcmap_not_enough_meso_balance));
                        return false;
                    }

                    session.Currency.Meso -= furnishingShopMetadata.Price;
                    break;
                case FurnishingCurrencyType.Meret:
                    negAmount = -furnishingShopMetadata.Price;
                    if (session.Currency.CanAddMeret(negAmount) != negAmount) {
                        session.Send(CubePacket.Error(UgcMapError.s_err_ugcmap_not_enough_merat_balance));
                        return false;
                    }

                    session.Currency.Meret -= furnishingShopMetadata.Price;
                    break;
            }
        }

        return true;
    }

    public long AddCube(HeldCube cube) {
        const int amount = 1;
        lock (session.Item) {
            int count = storage.Count;
            long itemUid = AddStorage(cube);
            if (itemUid == 0) {
                return 0;
            }

            session.Send(FurnishingStoragePacket.Purchase(cube.ItemId, amount));
            if (storage.Count != count) {
                session.Send(FurnishingStoragePacket.Count(storage.Count));
            }
            return itemUid;
        }
    }

    // TODO: NOTE - This should also be called for opening a furnishing box
    public long AddCube(int id) {
        const int amount = 1;
        lock (session.Item) {
            int count = storage.Count;
            long itemUid = AddStorage(id);
            if (itemUid == 0) {
                return 0;
            }

            session.Send(FurnishingStoragePacket.Purchase(id, amount));
            if (storage.Count != count) {
                session.Send(FurnishingStoragePacket.Count(storage.Count));
            }
            return itemUid;
        }
    }

    public long AddCube(Item item) {
        lock (session.Item) {
            int count = storage.Count;
            long itemUid = AddStorage(item);
            if (itemUid == 0) {
                return 0;
            }

            session.Send(FurnishingStoragePacket.Purchase(item.Id, item.Amount));
            if (storage.Count != count) {
                session.Send(FurnishingStoragePacket.Count(storage.Count));
            }

            return itemUid;
        }
    }

    public bool RetrieveCube(long uid) {
        lock (session.Item) {
            if (!RemoveInventory(uid, out PlotCube? cube)) {
                return false;
            }

            long itemUid = AddStorage(cube);
            if (itemUid == 0) {
                logger.Fatal("Failed to return cube: {CubeId} to storage", cube.Id);
                throw new InvalidOperationException($"Failed to return cube: {cube.Id} to storage");
            }

            return true;
        }
    }

    private long AddStorage(HeldCube cube) {
        Item? item = session.Field?.ItemDrop.CreateItem(cube.ItemId);
        if (item == null) {
            return 0;
        }

        return AddStorage(item, cube.Template);
    }

    private long AddStorage(int itemId) {
        Item? item = session.Field?.ItemDrop.CreateItem(itemId);
        if (item == null) {
            return 0;
        }

        return AddStorage(item);
    }

    public long AddStorage(Item item, UgcItemLook? template = null) {
        const int amount = 1;

        lock (session.Item) {
            Item? stored = storage.FirstOrDefault(existing => existing.Id == item.Id && existing.Template?.Url == template?.Url);
            if (stored == null) {
                if (storage.OpenSlots <= 0) {
                    logger.Error("Furnishing storage is full, cannot add item: {ItemId}", item.Id);
                    return 0;
                }
                item.Group = ItemGroup.Furnishing;
                using GameStorage.Request db = session.GameStorage.Context();
                Item? newItem = db.CreateItem(session.AccountId, item);
                if (newItem == null) {
                    return 0;
                }

                if (storage.Add(newItem).Count <= 0) {
                    db.SaveItems(0, newItem);
                    return 0;
                }

                session.Send(FurnishingStoragePacket.Add(newItem));
                return newItem.Uid;
            }

            if (stored.Amount + amount > item.Metadata.Property.SlotMax) {
                return 0;
            }

            int previousAmount = stored.Amount;
            stored.Amount += amount;
            if (previousAmount == 0) {
                session.Send(FurnishingStoragePacket.Add(stored));
                return stored.Uid;
            }

            session.Send(FurnishingStoragePacket.Update(stored.Uid, stored.Amount));
            return stored.Uid;
        }
    }

    private bool AddInventory(PlotCube cube) {
        if (!inventory.TryAdd(cube.Id, cube)) {
            return false;
        }

        session.Send(FurnishingInventoryPacket.Add(cube));
        return true;
    }

    private bool RemoveInventory(long uid, [NotNullWhen(true)] out PlotCube? cube) {
        if (!inventory.Remove(uid, out cube)) {
            return false;
        }

        session.Send(FurnishingInventoryPacket.Remove(uid));
        return true;
    }

    public void RemoveItem(long itemUid) {
        lock (session.Item) {
            Item? item = storage.Get(itemUid);
            if (item is null) {
                return;
            }

            item.Amount = 0;

            session.Send(FurnishingStoragePacket.Remove(itemUid));
            return;
        }
    }

    public void SendStorageCount() {
        lock (session.Item) {
            session.Send(FurnishingStoragePacket.Count(storage.Count));
        }
    }

    public void Save(GameStorage.Request db) {
        lock (session.Item) {
            db.SaveItems(session.AccountId, storage.ToArray());
        }
    }
}
