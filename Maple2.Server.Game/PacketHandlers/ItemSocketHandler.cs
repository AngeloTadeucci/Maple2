using System.Diagnostics;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.LuaFunctions;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using static Maple2.Model.Error.ItemSocketError;

namespace Maple2.Server.Game.PacketHandlers;

public class ItemSocketHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.ItemSocketSystem;

    private enum Command : byte {
        UnlockSocket = 0,
        StageUnlockSocket = 2,
        UpgradeGemstone = 4,
        StageUpgradeGemstone = 6,
        EquipGemstone = 8,
        UnequipGemstone = 10,
        Unknown14 = 14,
        Unknown15 = 15,
        Unknown16 = 16,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required ItemMetadataStorage ItemMetadata { private get; init; }
    public required TableMetadataStorage TableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.UnlockSocket:
                HandleUnlockSocket(session, packet);
                return;
            case Command.StageUnlockSocket:
                HandleStageUnlockSocket(session, packet);
                return;
            case Command.UpgradeGemstone:
                HandleUpgradeGemstone(session, packet);
                return;
            case Command.StageUpgradeGemstone:
                HandleStageUpgradeGemstone(session, packet);
                return;
            case Command.EquipGemstone:
                HandleEquipGemstone(session, packet);
                return;
            case Command.UnequipGemstone:
                HandleUnequipGemstone(session, packet);
                return;
        }
    }

    private void HandleUnlockSocket(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        byte count = packet.ReadByte();
        long[] materialUids = new long[count];
        for (int i = 0; i < count; i++) {
            materialUids[i] = packet.ReadLong();
        }

        lock (session.Item) {
            Item? item = session.Item.GetGear(itemUid);
            if (item == null) {
                session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_invalid_target));
                return;
            }

            if (item.Socket == null || item.Socket.UnlockSlots >= item.Socket.MaxSlots) {
                session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_socket_unlock_all));
                return;
            }

            foreach (long materialUid in materialUids) {
                Item? material = session.Item.Inventory.Get(materialUid, InventoryType.Gear);
                if (material == null) {
                    session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_invalid_target));
                    return;
                }

                if (!IsSocketMaterialEqual(item, material) || !AllSocketsEmpty(material)) {
                    session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_invalid_target_ingredient));
                    return;
                }
            }

            if (!ConsumeSocketUnlockIngredients(item)) {
                return;
            }

            foreach (long materialUid in materialUids) {
                if (!session.Item.Inventory.Consume(materialUid, amount: 1)) {
                    Logger.Fatal("Failed to consume material: {ItemUid}", materialUid);
                    session.Send(ItemSocketPacket.Error(code: 1, error: s_itemsocketsystem_error_server_default));
                    return;
                }
            }

            item.Socket.UnlockSlots++;
            session.Send(ItemSocketPacket.UnlockSocket(true, item));
            session.ConditionUpdate(ConditionType.socket_unlock, targetLong: item.Socket.UnlockSlots);
            session.ConditionUpdate(ConditionType.socket_unlock_success);
            session.ConditionUpdate(ConditionType.socket_unlock_try);
        }

        #region Local Function
        bool ConsumeSocketUnlockIngredients(Item equip) {
            (int _, string tag, int amount) =
                Lua.CalcItemSocketUnlockIngredient(0, equip.Rarity, (ushort) equip.Metadata.Limit.Level, 0, equip.Metadata.Property.SkinType);

            var ingredient = new IngredientInfo(Enum.Parse<ItemTag>(tag), amount);
            if (!session.Item.Inventory.Consume([ingredient])) {
                session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_lack_price));
                return false;
            }

            return true;
        }
        #endregion
    }

    private void HandleStageUnlockSocket(GameSession session, IByteReader packet) {
        long zero = packet.ReadLong(); // 0
        byte max = packet.ReadByte(); // 0xFF (-1/255)
        Debug.Assert(zero == 0 && max == byte.MaxValue);

        long itemUid = packet.ReadLong();

        Item? item = session.Item.GetGear(itemUid);
        if (item == null) {
            return;
        }

        session.Send(ItemSocketPacket.StageUnlockSocket(item, 100f));
    }

    private void HandleUpgradeGemstone(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        sbyte slot = packet.Read<sbyte>(); // -1 if not equipped
        long gemUid = packet.ReadLong();   // this has a client-generated uid when if equipped

        lock (session.Item) {
            if (itemUid != 0) {
                (ItemGemstone gem, GemstoneUpgradeTable.Entry entry)? result = CheckUpgradeGemstoneOnItem(session, itemUid, slot);
                if (result == null) {
                    return;
                }

                if (!ConsumeGemstoneUpgradeIngredients(result.Value.entry)) {
                    return;
                }

                result.Value.gem.ItemId = result.Value.entry.NextItemId;
                session.Send(ItemSocketPacket.UpgradeGemstone(itemUid, true, gemUid, result.Value.gem));
                session.ConditionUpdate(ConditionType.gemstone_upgrade, targetLong: result.Value.entry.Level + 1);
                session.ConditionUpdate(ConditionType.gemstone_upgrade_try);
                session.ConditionUpdate(ConditionType.gemstone_upgrade_success);
            } else {
                (GemstoneUpgradeTable.Entry entry, ItemMetadata upgrade)? result = CheckUpgradeGemstoneInInventory(session, gemUid);
                if (result == null) {
                    return;
                }

                if (session.Item.Inventory.FreeSlots(InventoryType.Gemstone) < 1) {
                    session.Send(ItemSocketPacket.Error(code: 5, error: s_itemsocketsystem_error_server_default));
                    return;
                }

                if (!ConsumeGemstoneUpgradeIngredients(result.Value.entry)) {
                    return;
                }

                if (!session.Item.Inventory.Remove(gemUid, out Item? gem, amount: 1)) {
                    session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_invalid_target_gemstone));
                    return;
                }

                Item upgradeGem = gem.Mutate(result.Value.upgrade);
                if (!session.Item.Inventory.Add(upgradeGem, true)) {
                    Logger.Fatal("Failed to add upgraded gem {ItemUid} to inventory", upgradeGem.Uid);
                    session.Send(ItemSocketPacket.Error(code: 6, error: s_itemsocketsystem_error_server_default));
                    return;
                }

                session.Send(ItemSocketPacket.UpgradeGemstone(itemUid, true, upgradeGem));
                session.ConditionUpdate(ConditionType.gemstone_upgrade, targetLong: result.Value.entry.Level + 1);
                session.ConditionUpdate(ConditionType.gemstone_upgrade_try);
                session.ConditionUpdate(ConditionType.gemstone_upgrade_success);
            }
        }

        #region Local Function
        bool ConsumeGemstoneUpgradeIngredients(GemstoneUpgradeTable.Entry entry) {
            IngredientInfo[] ingredients = entry.Ingredients.Select(ingredient => new IngredientInfo(ingredient.ItemTag, ingredient.Amount)).ToArray();
            if (!session.Item.Inventory.Consume(ingredients)) {
                session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_invalid_target_ingredient_count));
                return false;
            }

            return true;
        }
        #endregion
    }

    private void HandleStageUpgradeGemstone(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        sbyte slot = packet.Read<sbyte>(); // -1 if not equipped
        long gemUid = packet.ReadLong();   // this has a client-generated uid when if equipped

        if (itemUid != 0) {
            if (CheckUpgradeGemstoneOnItem(session, itemUid, slot) == null) {
                return;
            }
        } else {
            if (CheckUpgradeGemstoneInInventory(session, gemUid) == null) {
                return;
            }
        }

        session.Send(ItemSocketPacket.StageUpgradeGemstone(itemUid, slot, gemUid, 100f));
    }

    private void HandleEquipGemstone(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        long gemUid = packet.ReadLong();
        byte socketSlot = packet.ReadByte();

        lock (session.Item) {
            Item? item = session.Item.GetGear(itemUid);
            if (item == null) {
                session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_invalid_target));
                return;
            }
            Item? gem = session.Item.Inventory.Get(gemUid, InventoryType.Gemstone);
            if (gem == null) {
                session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_invalid_target_gemstone));
                return;
            }

            SocketState state = CheckSocketSlot(item.Socket, socketSlot);
            switch (state) {
                case SocketState.None:
                    session.Send(ItemSocketPacket.Error(code: 8, error: s_itemsocketsystem_error_server_default));
                    return;
                case SocketState.Locked:
                    session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_socket_lock));
                    return;
                case SocketState.Used:
                    session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_socket_used));
                    return;
            }

            Debug.Assert(state == SocketState.Empty);
            if (!session.Item.Inventory.Consume(gemUid, amount: 1)) {
                session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_invalid_target_gemstone));
                return;
            }

            // item.Socket cannot be null after CheckSocketSlot()
            var itemGemstone = new ItemGemstone(gem.Id, gem.Binding, gem.Stats, gem.IsLocked, gem.UnlockTime);
            item.Socket!.Sockets[socketSlot] = itemGemstone;
            session.Send(ItemSocketPacket.EquipGemstone(item.Uid, socketSlot, itemGemstone));
            session.ConditionUpdate(ConditionType.gemstone_puton);
        }
    }

    private void HandleUnequipGemstone(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        byte socketSlot = packet.ReadByte();

        lock (session.Item) {
            Item? item = session.Item.GetGear(itemUid);
            if (item == null) {
                session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_invalid_target));
                return;
            }

            SocketState state = CheckSocketSlot(item.Socket, socketSlot);
            switch (state) {
                case SocketState.None:
                    session.Send(ItemSocketPacket.Error(code: 10, error: s_itemsocketsystem_error_server_default));
                    return;
                case SocketState.Locked:
                    session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_socket_lock));
                    return;
                case SocketState.Empty:
                    session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_socket_empty));
                    return;
            }

            Debug.Assert(state == SocketState.Used);
            // item.Socket cannot be null after CheckSocketSlot()
            // itemGemstone cannot be null if SocketState.Used
            ItemGemstone itemGemstone = item.Socket!.Sockets[socketSlot]!;
            if (!ConsumeGemstoneUnequipIngredients(itemGemstone)) {
                return;
            }

            Item? gem = session.Field?.ItemDrop.CreateItem(itemGemstone.ItemId, rarity: Constant.GemstoneGrade);
            if (gem == null) {
                session.Send(ItemSocketPacket.Error(11, error: s_itemsocketsystem_error_server_default));
                return;
            }

            if (!session.Item.Inventory.Add(gem, true)) {
                return; // Failed to add to inventory
            }

            item.Socket!.Sockets[socketSlot] = null;
            session.Send(ItemSocketPacket.UnequipGemstone(item.Uid, socketSlot));
            session.ConditionUpdate(ConditionType.gemstone_putoff);
        }

        #region Local Function
        bool ConsumeGemstoneUnequipIngredients(ItemGemstone gemstone) {
            if (!TableMetadata.GemstoneUpgradeTable.Entries.TryGetValue(gemstone.ItemId, out GemstoneUpgradeTable.Entry? entry)) {
                session.Send(ItemSocketPacket.Error(code: 12, error: s_itemsocketsystem_error_server_default));
                return false;
            }

            (string tag, int amount) = Lua.CalcGetGemStonePutOffPrice(Constant.GemstoneGrade, (ushort) entry.Level, 0);
            var ingredient = new IngredientInfo(Enum.Parse<ItemTag>(tag), amount);
            if (!session.Item.Inventory.Consume([ingredient])) {
                session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_lack_price));
                return false;
            }

            return true;
        }
        #endregion
    }

    private (ItemGemstone, GemstoneUpgradeTable.Entry)? CheckUpgradeGemstoneOnItem(GameSession session, long itemUid, sbyte slot) {
        ItemSocket? itemSocket = session.Item.GetGear(itemUid)?.Socket;
        if (itemSocket == null) {
            session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_invalid_target));
            return null;
        }
        if (slot < 0 || slot >= itemSocket.UnlockSlots) {
            session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_server_default));
            return null;
        }

        ItemGemstone? gem = itemSocket.Sockets[slot];
        if (gem == null) {
            session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_socket_empty));
            return null;
        }

        if (!TableMetadata.GemstoneUpgradeTable.Entries.TryGetValue(gem.ItemId, out GemstoneUpgradeTable.Entry? entry)) {
            session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_invalid_target_ingredient));
            return null;
        }
        if (entry.NextItemId == 0) {
            session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_gemstone_maxlevel));
            return null;
        }

        return (gem, entry);
    }

    private (GemstoneUpgradeTable.Entry, ItemMetadata)? CheckUpgradeGemstoneInInventory(GameSession session, long gemUid) {
        Item? gem = session.Item.Inventory.Get(gemUid, InventoryType.Gemstone);
        if (gem == null) {
            session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_invalid_target_gemstone));
            return null;
        }

        if (!TableMetadata.GemstoneUpgradeTable.Entries.TryGetValue(gem.Id, out GemstoneUpgradeTable.Entry? entry)) {
            session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_invalid_target_ingredient));
            return null;
        }
        if (entry.NextItemId == 0) {
            session.Send(ItemSocketPacket.Error(error: s_itemsocketsystem_error_gemstone_maxlevel));
            return null;
        }
        if (!ItemMetadata.TryGet(entry.NextItemId, out ItemMetadata? upgrade)) {
            session.Send(ItemSocketPacket.Error(code: 4, error: s_itemsocketsystem_error_server_default));
            return null;
        }

        return (entry, upgrade);
    }

    private static bool IsSocketMaterialEqual(Item item, Item material) {
        return item.Id == material.Id
               && item.Rarity == material.Rarity
               && item.Socket != null
               && material.Socket != null
               && item.Socket.UnlockSlots == material.Socket.UnlockSlots;
    }

    private static bool AllSocketsEmpty(Item material) {
        return material.Socket == null || material.Socket.Sockets.All(gem => gem == null);
    }

    private enum SocketState { None, Locked, Used, Empty }
    private static SocketState CheckSocketSlot(ItemSocket? itemSocket, byte slot) {
        if (itemSocket == null || slot > itemSocket.MaxSlots) {
            return SocketState.None;
        }
        if (slot > itemSocket.UnlockSlots) {
            return SocketState.Locked;
        }
        return itemSocket.Sockets[slot] != null ? SocketState.Used : SocketState.Empty;
    }
}
