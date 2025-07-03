using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Manager.Items;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Serilog;

namespace Maple2.Server.Game.Manager;

public sealed class PetManager : IDisposable {
    private readonly GameSession session;
    private readonly FieldPet pet;
    private readonly ItemCollection items;

    public Item Pet => pet.Pet;
    private readonly PetConfig config;

    private ItemPet ItemPet => Pet.Pet!;
    public int OwnerId => session.Player.ObjectId;

    public PetManager(GameSession session, FieldPet pet) {
        this.session = session;
        this.pet = pet;

        short itemSlots = 0;
        if (session.ItemMetadata.TryGetPet(pet.Value.Id, out PetMetadata? metadata) && metadata.Type == 0) {
            itemSlots = (short) metadata.ItemSlots;
        }

        items = new ItemCollection(itemSlots);

        // AddOrUpdate Pet Collection
        if (!session.Player.Value.Unlock.Pets.TryGetValue(pet.Value.Id, out short rarity) || rarity < Pet.Rarity) {
            session.Player.Value.Unlock.Pets[pet.Value.Id] = (short) Pet.Rarity;
            session.Send(PetPacket.AddCollection(pet.Value.Id, (short) Pet.Rarity));
        }

        using GameStorage.Request db = session.GameStorage.Context();
        config = db.GetPetConfig(Pet.Uid);
        foreach (Item item in db.GetStorage(Pet.Uid)) {
            if (items.Add(item).Count == 0) {
                Log.Error("Failed to add storage item:{Uid}", item.Uid);
            }
        }

        session.Field?.Broadcast(PetPacket.Summon(pet));
    }

    public void Load() {
        session.Send(PetPacket.Load(OwnerId, ItemPet, config));
    }

    public void LoadInventory() {
        lock (session.Item) {
            session.Send(PetInventoryPacket.Load(items.ToList()));
        }
    }

    public StringCode Add(long uid, short slot, int amount) {
        lock (session.Item) {
            Item? deposit = session.Item.Inventory.Get(uid);
            if (deposit == null || deposit.Amount < amount) {
                return StringCode.s_item_err_invalid_count;
            }

            if (deposit.Pet != null) {
                return StringCode.s_pet_inventory_not_sendin_petitem;
            }

            if (items.OpenSlots == 0) {
                int remaining = items.GetStackResult(deposit, amount);
                if (amount == remaining) {
                    return StringCode.s_pet_inventory_not_sendin;
                }

                // Stack what we can and ignore the rest.
                amount -= remaining;
            }

            if (!session.Item.Inventory.Remove(uid, out deposit, amount)) {
                return StringCode.s_empty_string;
            }

            deposit.Slot = slot;
            IList<(Item, int Added)> result = items.Add(deposit, true);

            foreach ((Item item, int _) in result) {
                session.Send(deposit.Uid == item.Uid
                    ? PetInventoryPacket.Add(item)
                    : PetInventoryPacket.Update(item.Uid, item.Amount));
            }

            return StringCode.s_empty_string;
        }
    }

    public StringCode Remove(long uid, short slot, int amount) {
        lock (session.Item) {
            Item? withdraw = items.Get(uid);
            if (withdraw == null || withdraw.Amount < amount) {
                return StringCode.s_item_err_invalid_count;
            }

            if (!RemoveInternal(uid, amount, out withdraw)) {
                return StringCode.s_empty_string;
            }

            withdraw.Slot = slot;
            session.Item.Inventory.Add(withdraw, commit: true);
            return StringCode.s_empty_string;
        }
    }

    public bool Move(long uid, short dstSlot) {
        lock (session.Item) {
            if (dstSlot < 0 || dstSlot >= items.Size) {
                return false;
            }

            if (items.Remove(uid, out Item? srcItem)) {
                short srcSlot = srcItem.Slot;
                if (items.RemoveSlot(dstSlot, out Item? removeDst)) {
                    items[srcSlot] = removeDst;
                }

                items[dstSlot] = srcItem;
                session.Send(PetInventoryPacket.Move(removeDst?.Uid ?? 0, srcSlot, uid, dstSlot));
            }

            return true;
        }
    }

    public void BadgeChanged(ItemBadge? badge) {
        pet.UpdateSkin(badge?.PetSkinId ?? 0);
    }

    public void Rename(string name) {
        ItemPet.Name = name;
        ItemPet.RenameRemaining = 0;

        session.Send(PetPacket.Rename(OwnerId, ItemPet));
    }

    public void UpdatePotionConfig(PetPotionConfig[] potionConfig) {
        config.PotionConfig = potionConfig;

        session.Send(PetPacket.UpdatePotionConfig(OwnerId, config.PotionConfig));
    }

    public void UpdateLootConfig(PetLootConfig lootConfig) {
        config.LootConfig = lootConfig;

        session.Send(PetPacket.UpdateLootConfig(OwnerId, config.LootConfig));
    }

    public void Dispose() {
        using GameStorage.Request db = session.GameStorage.Context();
        lock (session.Item) {
            db.SavePetConfig(Pet.Uid, config);
            db.SaveItems(Pet.Uid, items.ToArray());
        }

        session.Field?.RemovePet(pet.ObjectId);
        session.Field?.Broadcast(PetPacket.UnSummon(pet));
        session.Pet = null;
    }

    #region Internal (No Locks)
    private bool RemoveInternal(long uid, int amount, [NotNullWhen(true)] out Item? removed) {
        if (amount > 0) {
            Item? item = items.Get(uid);
            if (item == null || item.Amount < amount) {
                session.Send(NoticePacket.MessageBox(StringCode.s_item_err_invalid_count));
                removed = null;
                return false;
            }

            // Otherwise, we would just do a full remove.
            if (item.Amount > amount) {
                using GameStorage.Request db = session.GameStorage.Context();
                removed = db.SplitItem(0, item, amount);
                if (removed == null) {
                    return false;
                }
                item.Amount -= amount;

                session.Send(PetInventoryPacket.Update(uid, item.Amount));
                return true;
            }
        }

        // Full remove of item
        if (items.Remove(uid, out removed)) {
            session.Send(PetInventoryPacket.Remove(uid));
            return true;
        }

        return false;
    }
    #endregion
}
