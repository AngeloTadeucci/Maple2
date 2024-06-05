using Maple2.Database.Storage;
using Maple2.Model;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Shop;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.PacketHandlers;

public class BeautyHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.Beauty;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required ItemMetadataStorage ItemMetadata { private get; init; }
    public required TableMetadataStorage TableMetadata { private get; init; }

    // ReSharper restore All
    #endregion

    private enum Command : byte {
        Shop = 0,
        CreateBeauty = 3,
        Unknown4 = 4, // UpdateEar?
        UpdateBeauty = 5,
        UpdateSkin = 6,
        RandomHair = 7,
        Warp = 10,
        ConfirmRandomHair = 12,
        SaveHair = 16,
        AddSlots = 17,
        DeleteHair = 18,
        AskAddSlots = 19,
        ApplySavedHair = 21,
        GearDye = 22,
        Voucher = 23,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required NpcMetadataStorage NpcMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Shop:
                HandleShop(session, packet);
                return;
            case Command.CreateBeauty:
                HandleCreateBeauty(session, packet);
                return;
            case Command.Unknown4:
                packet.ReadLong();
                return;
            case Command.UpdateBeauty:
                HandleUpdateBeauty(session, packet);
                return;
            case Command.UpdateSkin:
                HandleUpdateSkin(session, packet);
                return;
            case Command.RandomHair:
                HandleRandomHair(session, packet);
                return;
            case Command.Warp:
                HandleWarp(session, packet);
                return;
            case Command.ConfirmRandomHair:
                HandleConfirmRandomHair(session, packet);
                return;
            case Command.SaveHair:
                HandleSaveHair(session, packet);
                return;
            case Command.AddSlots:
                HandleAddSlots(session, packet);
                return;
            case Command.DeleteHair:
                HandleDeleteHair(session, packet);
                return;
            case Command.AskAddSlots:
                HandleAskAddSlots(session, packet);
                return;
            case Command.ApplySavedHair:
                HandleApplySavedHair(session, packet);
                return;
            case Command.GearDye:
                HandleGearDye(session, packet);
                return;
            case Command.Voucher:
                HandleVoucher(session, packet);
                return;
        }
    }

    private void HandleShop(GameSession session, IByteReader packet) {
        int npcId = packet.ReadInt();
        var shopType = (BeautyShopType) packet.ReadByte();

        if (!NpcMetadata.TryGet(npcId, out NpcMetadata? metadata)) {
            return;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        BeautyShop? shop = db.GetBeautyShop(metadata.Basic.ShopId);
        if (shop == null) {
            // TODO: Error?
            return;
        }

        // filter out non-usable genders
        var entries = new List<BeautyShopEntry>();
        foreach (BeautyShopEntry entry in shop.Entries) {
            if (!ItemMetadata.TryGet(entry.ItemId, out ItemMetadata? itemMetadata)) {
                continue;
            }

            if (itemMetadata.Limit.Gender == session.Player.Value.Character.Gender ||
                itemMetadata.Limit.Gender == Gender.All) {
                entries.Add(entry);
            }

        }
        shop.Entries = entries;
        session.BeautyShop = shop;

        switch (shop.Category) {
            case BeautyShopCategory.Dye:
                switch (shop.ShopType) {
                    case BeautyShopType.Item:
                        session.Send(BeautyPacket.DyeShop(shop));
                        break;
                    case BeautyShopType.Skin:
                        session.Send(BeautyPacket.BeautyShop(shop));
                        break;
                    default:
                        return;
                }
                break;
            case BeautyShopCategory.Save:
                session.Send(BeautyPacket.SaveShop(shop));
                session.Beauty.Load();
                break;
            case BeautyShopCategory.Special:
            case BeautyShopCategory.Standard:
                session.Send(BeautyPacket.BeautyShop(shop));
                break;
            default:
                return;
        }
    }

    private void HandleCreateBeauty(GameSession session, IByteReader packet) {
        byte index = packet.ReadByte();
        bool useVoucher = packet.ReadBool();
        int itemId = packet.ReadInt();

        if (session.BeautyShop == null) {
            return;
        }

        BeautyShopEntry? entry = session.BeautyShop.Entries.FirstOrDefault(entry => entry.ItemId == itemId);
        if (entry == null) {
            return;
        }

        if (useVoucher) {
            if (!PayWithVoucher(session, session.BeautyShop)) {
                return;
            }
        } else {
            if (!PayWithCurrency(session, entry.Cost)) {
                return;
            }
        }

        if (ModifyBeauty(session, packet, session.BeautyShop.ShopType, entry.ItemId)) {
            session.ConditionUpdate(ConditionType.beauty_add, codeLong: itemId);
        }
    }

    private void HandleUpdateBeauty(GameSession session, IByteReader packet) {
        byte index = packet.ReadByte();
        bool useVoucher = packet.ReadBool();
        long uid = packet.ReadLong();

        if (session.BeautyShop == null) {
            return;
        }
        BeautyShop shop = session.BeautyShop;

        Item? cosmetic = session.Item.Equips.Get(uid);
        if (cosmetic == null) {
            return;
        }

        // Mirror NPC. Only for cap rotation/positioning
        if (cosmetic.Metadata.SlotNames.Contains(EquipSlot.CP)) {
            var appearance = packet.ReadClass<CapAppearance>();
            cosmetic.Appearance = appearance;
            session.Field?.Broadcast(ItemUpdatePacket.Update(session.Player, cosmetic));
        }

        if (useVoucher) {
            if (!PayWithVoucher(session, shop)) {
                return;
            }
        } else {
            if (!PayWithCurrency(session, shop.ServiceCost)) {
                return;
            }
        }

        EquipColor? startColor = cosmetic.Appearance?.Color;
        if (ModifyBeauty(session, packet, session.BeautyShop.ShopType, cosmetic.Id) && startColor != null) {
            Item newCosmetic = session.Item.Equips.Get(cosmetic.Metadata.SlotNames.First())!;
            if (!Equals(newCosmetic.Appearance?.Color, startColor)) {
                session.ConditionUpdate(ConditionType.beauty_change_color, codeLong: cosmetic.Id);
            }
        }
    }

    private void HandleUpdateSkin(GameSession session, IByteReader packet) {
        byte index = packet.ReadByte();
        var skinColor = packet.Read<SkinColor>();
        bool useVoucher = packet.ReadBool();

        if (session.BeautyShop == null) {
            return;
        }

        if (useVoucher) {
            if (!PayWithVoucher(session, session.BeautyShop)) {
                return;
            }
        } else {
            if (!PayWithCurrency(session, session.BeautyShop.ServiceCost)) {
                return;
            }
        }

        session.Player.Value.Character.SkinColor = skinColor;
        session.Field?.Broadcast(UserSkinColorPacket.Update(session.Player, skinColor));
    }

    private void HandleRandomHair(GameSession session, IByteReader packet) {
        int shopId = packet.ReadInt();
        bool useVoucher = packet.ReadBool();

        if (session.BeautyShop == null || session.BeautyShop.Id != shopId) {
            return;
        }
        BeautyShop shop = session.BeautyShop;

        if (useVoucher) {
            if (!PayWithVoucher(session, shop)) {
                return;
            }
        } else {
            if (!PayWithCurrency(session, shop.ServiceCost)) {
                return;
            }
        }

        BeautyShopEntry entry = shop.Entries[Random.Shared.Next(shop.Entries.Count)];

        if (!ItemMetadata.TryGet(entry.ItemId, out ItemMetadata? itemMetadata)) {
            return;
        }
        DefaultHairMetadata defaultHairMetadata = itemMetadata.DefaultHairs[Random.Shared.Next(itemMetadata.DefaultHairs.Length)];

        // Grab random hair from default hair metadata
        double frontLength = Random.Shared.NextDouble() * (defaultHairMetadata.MaxScale - defaultHairMetadata.MinScale + defaultHairMetadata.MinScale);
        double backLength = Random.Shared.NextDouble() * (defaultHairMetadata.MaxScale - defaultHairMetadata.MinScale + defaultHairMetadata.MinScale);

        // Get random color
        if (!TableMetadata.ColorPaletteTable.Entries.TryGetValue(Constant.HairPaletteId, out IReadOnlyDictionary<int, ColorPaletteTable.Entry>? palette)) {
            return;
        }

        int colorIndex = Random.Shared.Next(palette.Count);
        ColorPaletteTable.Entry? colorEntry = palette.Values.ElementAtOrDefault(colorIndex);
        if (colorEntry == null) {
            return;
        }

        var hairAppearance = new HairAppearance(new EquipColor(colorEntry.Primary, colorEntry.Secondary, colorEntry.Tertiary, Constant.HairPaletteId, colorIndex),
            (float) backLength, defaultHairMetadata.BackPosition, defaultHairMetadata.BackRotation, (float) frontLength, defaultHairMetadata.FrontPosition,
            defaultHairMetadata.FrontRotation);

        Item? newHair = session.Field.ItemDrop.CreateItem(entry.ItemId);
        if (newHair == null) {
            return;
        }
        newHair.Appearance = hairAppearance;

        using GameStorage.Request db = session.GameStorage.Context();
        newHair = db.CreateItem(session.CharacterId, newHair);
        if (newHair == null) {
            return;
        }

        // Save old hair
        Item? prevHair = session.Item.Equips.Get(EquipSlot.HR);
        if (prevHair == null) {
            return;
        }
        session.Beauty.SavePreviousHair(prevHair);

        session.Item.Equips.EquipCosmetic(newHair, EquipSlot.HR);
        session.Send(BeautyPacket.RandomHair(prevHair.Id, newHair.Id));
        session.ConditionUpdate(ConditionType.beauty_random, codeLong: newHair.Id);
    }
    private void HandleWarp(GameSession session, IByteReader packet) {
        short type = packet.ReadShort();
        int mapId = type switch {
            1 => Constant.BeautyHairShopGotoFieldID,
            3 => Constant.BeautyFaceShopGotoFieldID,
            5 => Constant.BeautyColorShopGotoFieldID,
            _ => 0
        };
        int portalId = type switch {
            1 => Constant.BeautyHairShopGotoPortalID,
            3 => Constant.BeautyFaceShopGotoPortalID,
            5 => Constant.BeautyColorShopGotoPortalID,
            _ => 0
        };

        session.Send(session.PrepareField(mapId, portalId: portalId)
            ? FieldEnterPacket.Request(session.Player)
            : FieldEnterPacket.Error(MigrationError.s_move_err_default));
    }

    private void HandleConfirmRandomHair(GameSession session, IByteReader packet) {
        bool newHairSelected = packet.ReadBool();
        int voucherItemId = 0;
        if (session.BeautyShop == null) {
            return;
        }
        if (!newHairSelected) {
            session.Beauty.SelectPreviousHair();
            Item? voucher = session.Field.ItemDrop.CreateItem(session.BeautyShop.ServiceRewardItemId);
            if (voucher != null && !session.Item.Inventory.Add(voucher, true)) {
                session.Item.MailItem(voucher);
            }
        }

        session.Send(BeautyPacket.RandomHairResult(voucherItemId));
        session.Beauty.ClearPreviousHair();
    }

    private static void HandleSaveHair(GameSession session, IByteReader packet) {
        long uid = packet.ReadLong();
        if (session.BeautyShop == null) {
            return;
        }

        session.Beauty.AddHair(uid);
    }

    private void HandleAddSlots(GameSession session, IByteReader packet) {
        long uid = packet.ReadLong();
    }

    private void HandleDeleteHair(GameSession session, IByteReader packet) {
        long uid = packet.ReadLong();
        bool delete = packet.ReadBool();

        if (session.BeautyShop == null) {
            return;
        }

        if (delete) {
            session.Beauty.RemoveHair(uid);
        }
    }

    private void HandleAskAddSlots(GameSession session, IByteReader packet) {

    }

    private static void HandleApplySavedHair(GameSession session, IByteReader packet) {
        long uid = packet.ReadLong();
        byte index = packet.ReadByte();

        if (session.BeautyShop == null) {
            return;
        }

        if (!PayWithCurrency(session, session.BeautyShop.ServiceCost)) {
            return;
        }

        session.Beauty.EquipSavedCosmetic(uid);
    }

    private void HandleGearDye(GameSession session, IByteReader packet) {
        byte count = packet.ReadByte();
        if (session.BeautyShop == null) {
            return;
        }
        for (byte i = 0; i < count; i++) {
            bool isValid = packet.ReadBool();
            if (!isValid) {
                continue;
            }

            packet.ReadByte();
            bool useVoucher = packet.ReadBool();
            packet.ReadByte();
            packet.ReadInt();
            packet.ReadLong();
            long itemUid = packet.ReadLong();
            int itemId = packet.ReadInt();

            Item? item = session.Item.Equips.Get(itemUid);
            if (item == null) {
                return;
            }

            if (useVoucher) {
                if (!PayWithVoucher(session, session.BeautyShop)) {
                    return;
                }
            } else {
                if (!PayWithCurrency(session, session.BeautyShop.RecolorCost)) {
                    return;
                }
            }

            item.Appearance = item.EquipSlot() == EquipSlot.CP ? packet.ReadClass<CapAppearance>() : packet.ReadClass<ItemAppearance>();
            session.Field?.Broadcast(ItemUpdatePacket.Update(session.Player, item));
        }
    }

    private void HandleVoucher(GameSession session, IByteReader packet) {
        long uid = packet.ReadLong();

        Item? voucher = session.Item.Inventory.Get(uid);
        if (voucher == null || voucher.Metadata.Function?.Type != ItemFunction.ItemChangeBeauty) {
            return;
        }

        if (!int.TryParse(voucher.Metadata.Function.Parameters, out int shopId) ||
            !TableMetadata.ShopBeautyCouponTable.Entries.TryGetValue(shopId, out IReadOnlyList<int>? itemIds)) {
            return;
        }

        List<BeautyShopEntry> entries = itemIds.Select(itemId => new BeautyShopEntry(itemId, new BeautyShopCost(ShopCurrencyType.Item, voucher.Id, 1))).ToList();

        // Shop all must be the same type (hair, face, etc). Will not work correctly if mixed. Assumes all items are the same type, so we'll just check the first one.
        if (!ItemMetadata.TryGet(itemIds.First(), out ItemMetadata? itemEntry)) {
            return;
        }

        BeautyShopType shopType = itemEntry.SlotNames.First() switch {
            EquipSlot.HR => BeautyShopType.Hair,
            EquipSlot.FA => BeautyShopType.Face,
            EquipSlot.SK => BeautyShopType.Skin,
            EquipSlot.FD => BeautyShopType.Makeup,
            _ => BeautyShopType.Hair,
        };

        BeautyShop shop = new BeautyShop(shopId) {
            Category = BeautyShopCategory.Standard,
            Entries = entries,
            ShopType = shopType,
            RecolorCost = new BeautyShopCost(ShopCurrencyType.Item, voucher.Id, 1),
        };

        session.BeautyShop = shop;
        session.Send(BeautyPacket.BeautyShop(shop));
    }

    private static bool PayWithVoucher(GameSession session, BeautyShop shop) {
        ItemTag voucherTag = shop.ShopType switch {
            BeautyShopType.Hair when shop.Category == BeautyShopCategory.Special => Constant.BeautyHairSpecialVoucherTag,
            BeautyShopType.Hair => Constant.BeautyHairStandardVoucherTag,
            BeautyShopType.Face => Constant.BeautyFaceVoucherTag,
            BeautyShopType.Makeup => Constant.BeautyMakeupVoucherTag,
            BeautyShopType.Skin => Constant.BeautySkinVoucherTag,
            BeautyShopType.Item => Constant.BeautyItemColorVoucherTag,
            _ => ItemTag.None,
        };

        if (voucherTag == ItemTag.None) {
            return false;
        }

        var ingredient = new IngredientInfo(voucherTag, 1);
        if (!session.Item.Inventory.Consume(new[] { ingredient })) {
            session.Send(NoticePacket.Notice(NoticePacket.Flags.Alert, StringCode.s_err_invalid_item));
            return false;
        }

        session.Send(BeautyPacket.Voucher(shop.VoucherId, 1));
        return true;
    }

    private static bool PayWithCurrency(GameSession session, BeautyShopCost cost) {
        switch (cost.Type) {
            case ShopCurrencyType.Meso:
                if (session.Currency.CanAddMeso(-cost.Amount) != -cost.Amount) {
                    session.Send(BeautyPacket.Error(BeautyError.lack_currency));
                    return false;
                }
                session.Currency.Meso -= cost.Amount;
                break;
            case ShopCurrencyType.Meret:
            case ShopCurrencyType.EventMeret: // TODO: EventMeret?
            case ShopCurrencyType.GameMeret:
                if (session.Currency.CanAddMeret(-cost.Amount) != -cost.Amount) {
                    session.Send(BeautyPacket.Error(BeautyError.s_err_lack_merat_ask));
                    return false;
                }

                session.Currency.Meret -= cost.Amount;
                break;
            case ShopCurrencyType.Item:
                var ingredient = new ItemComponent(cost.ItemId, -1, cost.Amount, ItemTag.None);
                if (!session.Item.Inventory.ConsumeItemComponents(new[] { ingredient })) {
                    session.Send(BeautyPacket.Error(BeautyError.lack_currency));
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
                if (currencyType == CurrencyType.None || session.Currency[currencyType] < cost.Amount) {
                    session.Send(BeautyPacket.Error(BeautyError.lack_currency));
                    return false;
                }

                session.Currency[currencyType] -= cost.Amount;
                break;
        }
        return true;
    }

    private static bool ModifyBeauty(GameSession session, IByteReader packet, BeautyShopType type, int itemId) {
        Item? newCosmetic = session.Field.ItemDrop.CreateItem(itemId, 1, 1);
        if (newCosmetic == null) {
            return false;
        }

        newCosmetic.Appearance = type switch {
            BeautyShopType.Hair => packet.ReadClass<HairAppearance>(),
            BeautyShopType.Makeup => packet.ReadClass<DecalAppearance>(),
            BeautyShopType.Face => packet.ReadClass<ItemAppearance>(),
            BeautyShopType.Item => packet.ReadClass<ItemAppearance>(),
            BeautyShopType.Skin => packet.ReadClass<ItemAppearance>(),
            _ => ItemAppearance.Default,
        };
        using GameStorage.Request db = session.GameStorage.Context();
        newCosmetic = db.CreateItem(session.CharacterId, newCosmetic);
        if (newCosmetic == null) {
            return false;
        }

        return session.Item.Equips.EquipCosmetic(newCosmetic, newCosmetic.Metadata.SlotNames.First());
    }
}
