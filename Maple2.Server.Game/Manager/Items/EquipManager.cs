using System.Collections.Concurrent;
using Maple2.Database.Storage;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Manager.Items;

public class EquipManager {
    private readonly GameSession session;

    public readonly ConcurrentDictionary<EquipSlot, Item> Gear;
    public readonly ConcurrentDictionary<EquipSlot, Item> Outfit;
    public readonly ConcurrentDictionary<BadgeType, Item> Badge;

    private readonly List<Item> delete = [];

    public EquipManager(GameStorage.Request db, GameSession session) {
        this.session = session;

        Gear = new ConcurrentDictionary<EquipSlot, Item>();
        Outfit = new ConcurrentDictionary<EquipSlot, Item>();
        Badge = new ConcurrentDictionary<BadgeType, Item>();

        // Load from DB
        foreach ((ItemGroup tab, List<Item> items) in db.GetItemGroups(session.CharacterId, ItemGroup.Gear, ItemGroup.Outfit, ItemGroup.Badge)) {
            foreach (Item item in items) {
                EquipSlot equipSlot = item.EquipSlot();
                if (equipSlot is EquipSlot.Unknown) {
                    delete.Add(item);
                    continue;
                }

                switch (tab) {
                    case ItemGroup.Gear:
                        Gear[equipSlot] = item;
                        break;
                    case ItemGroup.Outfit:
                        Outfit[equipSlot] = item;
                        break;
                    case ItemGroup.Badge:
                        if (item.Badge != null) {
                            Badge[item.Badge.Type] = item;
                        }
                        break;
                }
            }
        }
    }

    public Item? Get(long itemUid) {
        return Gear.Values.FirstOrDefault(item => item.Uid == itemUid)
               ?? Outfit.Values.FirstOrDefault(item => item.Uid == itemUid)
               ?? Badge.Values.FirstOrDefault(item => item.Uid == itemUid);
    }

    public Item? Get(EquipSlot equipSlot) {
        return Gear.TryGetValue(equipSlot, out Item? item) ? item
            : Outfit.TryGetValue(equipSlot, out item) ? item
            : null;
    }

    public Item? Get(BadgeType badgeType) {
        return Badge.TryGetValue(badgeType, out Item? item) ? item : null;
    }

    /// <summary>
    /// Equips an item to its specified slot
    /// </summary>
    /// <param name="itemUid">The uid of the item to be equipped</param>
    /// <param name="slot">The slot to be equipped to.</param>
    /// <param name="isSkin">Whether the item is a skin or not.</param>
    /// <returns>The items if any that were unequipped to equip this item.</returns>
    public bool Equip(long itemUid, EquipSlot slot, bool isSkin) {
        if (!Enum.IsDefined<EquipSlot>(slot)) {
            return false;
        }

        lock (session.Item) {
            // Check that item is valid and can be equipped.
            Item? item = session.Item.Inventory.Get(itemUid);
            if (item == null) {
                session.Send(NoticePacket.MessageBox(StringCode.s_item_invalid_do_not_have));
                return false;
            }
            StringCode result = ValidateEquipItem(session, item);
            if (result != StringCode.s_empty_string) {
                session.Send(NoticePacket.MessageBox(result));
                return false;
            }

            if (item.Metadata.Property.IsSkin != isSkin) {
                return false;
            }
            if (!ValidEquipSlotForItem(slot, item)) {
                return false;
            }

            ConcurrentDictionary<EquipSlot, Item> equips = isSkin ? Outfit : Gear;
            if (item.Metadata.SlotNames.Length > 1) { // Two-Handed Weapon, Overall Armor
                int requiredFreeSlots = 0;
                foreach (EquipSlot removeSlot in item.Metadata.SlotNames) {
                    if (equips.ContainsKey(removeSlot)) {
                        requiredFreeSlots++;
                    }
                }

                // +1 because we will have a free slot from the item being equipped.
                int freeSlots = session.Item.Inventory.FreeSlots(isSkin ? InventoryType.Outfit : InventoryType.Gear) + 1;
                if (freeSlots <= requiredFreeSlots) {
                    return false;
                }
            }

            // Remove item being equipped from inventory so unequipped items and be moved there.
            if (!session.Item.Inventory.Remove(itemUid, out item)) {
                return false;
            }

            if (!UnequipInternal(slot, isSkin, item.Slot)) {
                throw new InvalidOperationException("Failed to unequip item");
            }
            if (item.Metadata.SlotNames.Length > 1) { // Two-Handed Weapon, Overall Armor
                foreach (EquipSlot unequipSlot in item.Metadata.SlotNames) {
                    if (!UnequipInternal(unequipSlot, isSkin)) {
                        throw new InvalidOperationException("Failed to unequip item");
                    }
                }
            }

            if (item.Metadata.Limit.TransferType is TransferType.BindOnEquip or TransferType.BindOnLoot) {
                session.Item.Bind(item);
            }

            item.Group = isSkin ? ItemGroup.Outfit : ItemGroup.Gear;
            item.Slot = (short) slot;
            equips[slot] = item;
            session.Field?.Broadcast(EquipPacket.EquipItem(session.Player, item, 0));
            if (item.Template != null) {
                session.ConditionUpdate(ConditionType.change_ugc_equip);
            }
            session.Stats.Refresh();
            return true;
        }
    }

    /// <summary>
    /// Unequips an item.
    /// </summary>
    /// <param name="itemUid">The uid of the item to be unequipped</param>
    /// <returns></returns>
    public bool Unequip(long itemUid) {
        lock (session.Item) {
            // Unequip from Gear.
            foreach ((EquipSlot slot, Item item) in Gear) {
                if (itemUid == item.Uid) {
                    if (UnequipInternal(slot, false)) {
                        session.Stats.Refresh();
                        return true;
                    }
                }
            }

            // Unequip from Outfit.
            foreach ((EquipSlot slot, Item item) in Outfit) {
                if (itemUid == item.Uid) {
                    if (UnequipInternal(slot, true)) {
                        session.Stats.Refresh();
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public bool EquipBadge(long itemUid) {
        lock (session.Item) {
            Item? badge = session.Item.Inventory.Get(itemUid);
            if (badge == null) {
                session.Send(NoticePacket.MessageBox(StringCode.s_item_invalid_do_not_have));
                return false;
            }
            StringCode result = ValidateEquipItem(session, badge);
            if (result != StringCode.s_empty_string) {
                session.Send(NoticePacket.MessageBox(result));
                return false;
            }

            // Remove item being equipped from inventory so unequipped items and be moved there.
            if (!session.Item.Inventory.Remove(itemUid, out badge) || badge.Badge == null) {
                return false;
            }

            if (!UnequipBadge(badge.Badge.Type, badge.Slot)) {
                throw new InvalidOperationException("Failed to unequip badge");
            }

            if (badge.Metadata.Limit.TransferType is TransferType.BindOnEquip or TransferType.BindOnLoot) {
                session.Item.Bind(badge);
            }

            badge.Group = ItemGroup.Badge;
            badge.Slot = -1;
            Badge[badge.Badge.Type] = badge;
            session.Field?.Broadcast(EquipPacket.EquipBadge(session.Player, badge));
            session.Stats.Refresh();

            if (badge.Badge.Type == BadgeType.PetSkin) {
                session.Pet?.BadgeChanged(badge.Badge);
            }

            return true;
        }
    }

    /// <summary>
    /// Equips a cosmetic item, unequips an cosmetic item (Hair, Eyes, Face Decor), and discards the previous cosmetic item.
    /// </summary>
    /// <param name="cosmetic">The hair, face, or face decor item to equip.</param>
    /// <param name="equipSlot">Slot to equip the cosmetic item.</param>
    public bool EquipCosmetic(Item cosmetic, EquipSlot equipSlot) {
        if (equipSlot is not (EquipSlot.HR or EquipSlot.FA or EquipSlot.FD)) {
            return false;
        }

        if (!ValidEquipSlotForItem(equipSlot, cosmetic)) {
            return false;
        }

        lock (session.Item) {
            StringCode result = ValidateEquipItem(session, cosmetic);
            if (result != StringCode.s_empty_string) {
                session.Send(NoticePacket.MessageBox(result));
                return false;
            }

            Item? currentCosmetic = Get(equipSlot);
            // Unequip and discard cosmetic item.
            if (currentCosmetic != null && !Unequip(currentCosmetic.Uid)) {
                return false;
            }

            // Item needs to be in the Outfit item group to display.
            cosmetic.Group = ItemGroup.Outfit;
            cosmetic.Slot = (short) equipSlot;
            Outfit[equipSlot] = cosmetic;
            session.Field?.Broadcast(EquipPacket.EquipItem(session.Player, cosmetic, 0));
            return true;
        }
    }

    public bool UnequipBadge(BadgeType slot, short inventorySlot = -1) {
        if (!Enum.IsDefined<BadgeType>(slot)) {
            return false;
        }

        lock (session.Item) {
            if (!Badge.TryRemove(slot, out Item? unequipBadge)) {
                return true; // Nothing to unequip
            }

            if (session.Item.Inventory.FreeSlots(InventoryType.Badge) == 0) {
                return false;
            }

            if (unequipBadge.Badge == null) {
                throw new InvalidOperationException("Unequipped badge that is not a badge");
            }

            unequipBadge.Group = ItemGroup.Default;
            unequipBadge.Slot = inventorySlot;

            bool success = session.Item.Inventory.Add(unequipBadge);
            if (!success) {
                throw new InvalidOperationException($"Failed to unequip badge: {unequipBadge.Uid}");
            }

            session.Field?.Broadcast(EquipPacket.UnequipBadge(session.Player, unequipBadge.Badge.Type));
            session.Stats.Refresh();

            if (unequipBadge.Badge.Type == BadgeType.PetSkin) {
                session.Pet?.BadgeChanged(null);
            }
            return true;
        }
    }

    private bool UnequipInternal(EquipSlot slot, bool isSkin, short inventorySlot = -1) {
        ConcurrentDictionary<EquipSlot, Item> equips = isSkin ? Outfit : Gear;
        if (!equips.TryRemove(slot, out Item? unequipItem)) {
            return true; // Nothing to unequip
        }

        if (session.Item.Inventory.FreeSlots(isSkin ? InventoryType.Outfit : InventoryType.Gear) == 0) {
            return false;
        }

        unequipItem.Group = ItemGroup.Default;
        unequipItem.Slot = inventorySlot;

        bool success;
        if (slot is EquipSlot.HR or EquipSlot.ER or EquipSlot.FA or EquipSlot.FD) {
            session.Item.Inventory.Discard(unequipItem);
            success = true;
        } else {
            success = session.Item.Inventory.Add(unequipItem);
        }

        if (!success) {
            throw new InvalidOperationException($"Failed to unequip item: {unequipItem.Uid}");
        }

        session.Field?.Broadcast(EquipPacket.UnequipItem(session.Player, unequipItem));
        session.Buffs.RemoveItemBuffs(unequipItem);
        return true;
    }

    public static StringCode ValidateEquipItem(GameSession session, Item item) {
        if (item.Metadata.Limit.Level > session.Player.Value.Character.Level) {
            return StringCode.s_item_err_puton_low_level;
        }
        if (item.IsExpired()) {
            return StringCode.s_item_err_puton_expired;
        }
        if (item.Metadata.Limit.JobLimits.Length > 0 && !item.Metadata.Limit.JobLimits.Contains(session.Player.Value.Character.Job.Code())) {
            return StringCode.s_item_err_puton_job;
        }
        if (item.Binding != null && item.Binding.CharacterId != session.CharacterId) {
            return StringCode.s_item_err_puton_invalid_binding;
        }

        return StringCode.s_empty_string;
    }

    private static bool ValidEquipSlot(EquipSlot slot) {
        return slot is not (EquipSlot.SK or EquipSlot.OH or EquipSlot.ER or EquipSlot.Unknown);
    }

    private static bool ValidEquipSlotForItem(EquipSlot slot, Item item) {
        if (!ValidEquipSlot(slot) || item.Metadata.SlotNames.Length == 0) {
            return false;
        }

        // LH and RH are both valid for OH equip.
        if (item.Metadata.SlotNames.Contains(EquipSlot.OH)) {
            return slot is EquipSlot.LH or EquipSlot.RH;
        }

        return slot == item.Metadata.SlotNames[0];
    }

    public void Save(GameStorage.Request db) {
        lock (session.Item) {
            db.SaveItems(0, delete.ToArray());
            db.SaveItems(session.CharacterId, Gear.Values.ToArray());
            db.SaveItems(session.CharacterId, Outfit.Values.ToArray());
            db.SaveItems(session.CharacterId, Badge.Values.ToArray());
        }
    }
}
