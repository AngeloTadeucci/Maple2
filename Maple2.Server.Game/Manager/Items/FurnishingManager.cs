using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
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

        List<Item>? items = db.GetItemGroups(session.AccountId, ItemGroup.Furnishing)
            .GetValueOrDefault(ItemGroup.Furnishing);
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
            return storage.FirstOrDefault(item => item.Uid == itemUid);
        }
    }

    /// <summary>
    /// Places a cube of the specified item uid at the requested location.
    /// If there are no amount remaining, we still keep the entry to allow reuse of the item uid.
    /// </summary>
    /// <param name="uid">Uid of the item to withdraw from</param>
    /// <param name="cube">The cube to be placed</param>
    /// <returns>Information about the withdrawn cube</returns>
    public bool TryPlaceCube(long uid, [NotNullWhen(true)] out PlotCube? cube) {
        const int amount = 1;
        lock (session.Item) {
            if (session.Field == null) {
                cube = null;
                return false;
            }

            Item? item = storage.Get(uid);
            if (item == null || item.Amount < amount) {
                cube = null;
                return false;
            }

            // We do not remove item from inventory even if it hits 0
            // this allows us to preserve the uid for later.
            item.Amount -= amount;
            session.Send(item.Amount > 0
                ? FurnishingStoragePacket.Update(item.Uid, item.Amount)
                : FurnishingStoragePacket.Remove(item.Uid));

            cube = new PlotCube(item.Id, NextCubeId(), item.Template);
            if (!AddInventory(cube)) {
                logger.Fatal("Failed to add cube: {CubeId} to inventory", cube.Id);
                throw new InvalidOperationException($"Failed to add cube: {cube.Id} to inventory");
            }

            return true;
        }
    }

    // TODO: NOTE - This should also be called for opening a furnishing box
    public long PurchaseCube(int id) {
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

    public long PurchaseCube(Item item) {
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

            long itemUid = AddStorage(cube.ItemId);
            if (itemUid == 0) {
                logger.Fatal("Failed to return cube: {CubeId} to storage", cube.Id);
                throw new InvalidOperationException($"Failed to return cube: {cube.Id} to storage");
            }

            return true;
        }
    }

    private long AddStorage(int itemId) {
        Item? item = session.Field.ItemDrop.CreateItem(itemId);
        if (item == null) {
            return 0;
        }

        return AddStorage(item);
    }

    public long AddStorage(Item? item) {
        const int amount = 1;
        if (item == null) {
            return 0;
        }

        Item? stored = storage.FirstOrDefault(existing => existing.Id == item.Id);
        if (stored == null) {
            item.Group = ItemGroup.Furnishing;
            using GameStorage.Request db = session.GameStorage.Context();
            item = db.CreateItem(session.AccountId, item);
            if (item == null || storage.Add(item).Count <= 0) {
                return 0;
            }

            session.Send(FurnishingStoragePacket.Add(item));
            return item.Uid;
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

    public void Save(GameStorage.Request db) {
        lock (session.Item) {
            db.SaveItems(session.AccountId, storage.ToArray());
        }
    }
}
