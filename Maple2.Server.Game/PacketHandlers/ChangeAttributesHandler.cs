using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util;
using static Maple2.Model.Error.ChangeAttributesError;

namespace Maple2.Server.Game.PacketHandlers;

public class ChangeAttributesHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.ChangeAttributes;

    private enum Command : byte {
        Change = 0,
        Select = 2,
        Unknown5 = 5,
        ForceFill = 6,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required ItemStatsCalculator ItemStatsCalc { private get; init; }
    public required Lua.Lua Lua { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Change:
                HandleChange(session, packet);
                return;
            case Command.Select:
                HandleSelect(session, packet);
                return;
            case Command.Unknown5:
                HandleUnknown5(session, packet);
                return;
            case Command.ForceFill:
                HandleForceFill(session, packet);
                return;
        }
    }

    private void HandleChange(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        packet.ReadLong();
        bool useLock = packet.ReadBool();

        bool isSpecialAttribute = false;
        short attribute = -1;
        if (useLock) {
            isSpecialAttribute = packet.ReadBool();
            attribute = packet.ReadShort();
        }

        lock (session.Item) {
            // Validate item being changed + lock attributes/lock item if necessary.
            Item? item = session.Item.Inventory.Get(itemUid, InventoryType.Gear)
                         ?? session.Item.Inventory.Get(itemUid, InventoryType.Pets);
            if (item == null) {
                session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_not_in_inven));
                return;
            }
            if (!IsValidItem(item)) {
                session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_impossible));
                return;
            }
            if (item.Stats == null) {
                session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_null_status));
                return;
            }

            Item? lockItem = null;
            if (useLock) {
                ItemStats.Option itemOption = item.Stats[ItemStats.Type.Random];
                if (isSpecialAttribute) {
                    if (!itemOption.Special.ContainsKey((SpecialAttribute) attribute)) {
                        session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_impossible));
                        return;
                    }
                } else {
                    if (!itemOption.Basic.ContainsKey((BasicAttribute) attribute)) {
                        session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_impossible));
                        return;
                    }
                }

                lockItem = GetLockConsumeItem(session, item);
                if (lockItem == null) {
                    session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_default));
                    return;
                }
            }

            // Calculate required catalysts.
            var ingredients = new List<IngredientInfo>();
            (string tag1Str, int amount1, string tag2Str, int amount2, string tag3Str, int amount3) = item.Type.IsPet
                ? Lua.CalcGetPetRemakeIngredient(item.TimeChangedOption, item.Rarity, 0)
                : Lua.CalcGetItemRemakeIngredientNew(item.Type.Type, item.TimeChangedOption, item.Rarity, item.Metadata.Limit.Level);
            if (Enum.TryParse<ItemTag>(tag1Str, out ItemTag tag1)) {
                ingredients.Add(new IngredientInfo(tag1, amount1));
            }
            if (Enum.TryParse<ItemTag>(tag2Str, out ItemTag tag2)) {
                ingredients.Add(new IngredientInfo(tag2, amount2));
            }
            if (Enum.TryParse<ItemTag>(tag3Str, out ItemTag tag3)) {
                ingredients.Add(new IngredientInfo(tag3, amount3));
            }

            // Clone the item so we can preview changes without modifying existing item.
            Item changeItem = item.Clone();
            if (changeItem.Stats == null) { // This should be impossible, but check again to make linter happy.
                session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_null_status));
                return;
            }

            // Randomize attributes.
            if (lockItem != null) {
                // Add back the locked attribute.
                if (isSpecialAttribute) {
                    if (!ItemStatsCalc.UpdateRandomOption(ref changeItem, new LockOption((SpecialAttribute) attribute, true))) {
                        session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_default));
                        return;
                    }
                } else {
                    if (!ItemStatsCalc.UpdateRandomOption(ref changeItem, new LockOption((BasicAttribute) attribute, true))) {
                        session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_default));
                        return;
                    }
                }
            } else {
                if (!ItemStatsCalc.UpdateRandomOption(ref changeItem)) {
                    session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_default));
                    return;
                }
            }

            // Consume required materials.
            if (!session.Item.Inventory.Consume(ingredients)) {
                session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_lack_price));
                return;
            }
            if (lockItem != null && !session.Item.Inventory.Consume(lockItem.Uid, 1)) {
                session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_fail_lack_lock_consume_item));
                return;
            }

            item.TimeChangedOption++;
            changeItem.TimeChangedOption++;
            session.ChangeAttributesItem = changeItem;
            session.Send(ChangeAttributesPacket.PreviewItem(session.ChangeAttributesItem));
        }
    }

    private static void HandleSelect(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        if (session.ChangeAttributesItem == null || session.ChangeAttributesItem.Uid != itemUid) {
            session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_fail_apply_option));
            return;
        }

        lock (session.Item) {
            Item? item = session.Item.Inventory.Get(itemUid, InventoryType.Gear)
                         ?? session.Item.Inventory.Get(itemUid, InventoryType.Pets);
            if (item == null) {
                session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_not_in_inven));
                return;
            }

            if (item.Stats == null || session.ChangeAttributesItem.Stats == null) {
                session.Send(ChangeAttributesPacket.Error(s_itemremake_error_server_null_status));
                return;
            }

            item.Stats[ItemStats.Type.Random] = session.ChangeAttributesItem.Stats[ItemStats.Type.Random];
            session.ChangeAttributesItem = null;
            session.Send(ChangeAttributesPacket.SelectItem(item));
        }
    }

    private static void HandleUnknown5(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        packet.ReadLong(); // unknown
        bool useLock = packet.ReadBool();
        if (useLock) {
            byte index = packet.ReadByte();
            var attribute = (BasicAttribute) packet.ReadShort();
        }

        packet.ReadBool();
        packet.ReadInt();
    }

    private static void HandleForceFill(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        int count = packet.ReadInt();
    }

    // It needs to be epic or better armor and accessories at level 50 and above.
    private static bool IsValidItem(Item item) {
        if (item.Rarity is < Constant.ChangeAttributesMinRarity or > Constant.ChangeAttributesMaxRarity) {
            return false;
        }
        if (!item.Type.IsWeapon && !item.Type.IsArmor && !item.Type.IsAccessory && !item.Type.IsCombatPet) {
            return false;
        }
        if (item.Metadata.Limit.Level < Constant.ChangeAttributesMinLevel) {
            return false;
        }

        return true;
    }

    // TODO: Rather than selecting first time, select best option (e.g. earliest expiring,untradeable)
    public static Item? GetLockConsumeItem(GameSession session, Item changeItem) {
        if (!IsValidItem(changeItem)) {
            return null;
        }

        if (changeItem.Type.IsWeapon) {
            return session.Item.Inventory.Filter(item => {
                if (item.Metadata.Property.Tag != ItemTag.LockItemOptionWeapon || item.IsExpired()) {
                    return false;
                }
                return item.Rarity switch {
                    4 => item.Id is 30000859 or 30001134 or 30001162,
                    5 => item.Id is 30001039 or 30050859 or 30051134 or 30051162,
                    6 => item.Id is 30060859 or 30061134 or 30061162,
                    _ => false,
                };
            }, InventoryType.Misc).FirstOrDefault();
        }
        if (changeItem.Type.IsArmor) {
            return session.Item.Inventory.Filter(item => {
                if (item.Metadata.Property.Tag != ItemTag.LockItemOptionArmor || item.IsExpired()) {
                    return false;
                }
                return item.Rarity switch {
                    4 => item.Id is 30000860 or 30000891 or 30000911 or 30001163,
                    5 => item.Id is 30001040 or 30050860 or 30050911 or 30051163 or 31002003,
                    6 => item.Id is 30060860 or 30060911 or 30061163 or 31001995,
                    _ => false,
                };
            }, InventoryType.Misc).FirstOrDefault();
        }
        if (changeItem.Type.IsAccessory) {
            return session.Item.Inventory.Filter(item => {
                if (item.Metadata.Property.Tag != ItemTag.LockItemOptionAccessory || item.IsExpired()) {
                    return false;
                }
                // TODO: 30061170=(Event) Accessory Attribute Lock Scroll
                return item.Rarity switch {
                    4 => item.Id is 30000889 or 30001038 or 30001164,
                    5 => item.Id is 30050889 or 30051038 or 30051164,
                    6 => item.Id is 30060889 or 30061038 or 30061164,
                    _ => false,
                };
            }, InventoryType.Misc).FirstOrDefault();
        }
        if (changeItem.Type.IsCombatPet) {
            return session.Item.Inventory.Filter(item => {
                if (item.Metadata.Property.Tag != ItemTag.LockItemOptionPet || item.IsExpired()) {
                    return false;
                }
                return item.Id is 30000923 or 30000924 or 30001308;
            }, InventoryType.Misc).FirstOrDefault();
        }
        return null;
    }
}
