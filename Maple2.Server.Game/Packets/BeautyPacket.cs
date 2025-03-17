using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Shop;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class BeautyPacket {
    private enum Command : byte {
        BeautyShop = 0,
        DyeShop = 1,
        SaveShop = 2,
        Error = 8,
        Voucher = 9,
        RandomHair = 11,
        RandomHairResult = 12,
        StartList = 13,
        ListCount = 14,
        ListHair = 15,
        SaveHair = 16, // and 17
        DeleteHair = 18,
        SaveSlots = 20,
        ApplySavedHair = 21,
    }

    public static ByteWriter BeautyShop(BeautyShop shop) {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.BeautyShop);
        pWriter.WriteClass<BeautyShop>(shop);
        pWriter.WriteBeautyShopItems(shop.Items);

        return pWriter;
    }

    public static ByteWriter DyeShop(BeautyShop shop) {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.DyeShop);
        pWriter.WriteClass<BeautyShop>(shop);

        return pWriter;
    }

    public static ByteWriter SaveShop(BeautyShop shop) {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.SaveShop);
        pWriter.WriteClass<BeautyShop>(shop);

        return pWriter;
    }

    public static ByteWriter Error(BeautyError error) {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<BeautyError>(error);

        return pWriter;
    }

    public static ByteWriter Voucher(int itemId, int amount) {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.Voucher);
        pWriter.WriteInt(itemId);
        pWriter.WriteInt(amount);

        return pWriter;
    }

    public static ByteWriter RandomHair(int prevHairId, int newHairId) {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.RandomHair);
        pWriter.WriteInt(prevHairId);
        pWriter.WriteInt(newHairId);

        return pWriter;
    }

    public static ByteWriter RandomHairResult(int voucherItemId, bool error = false) {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.RandomHairResult);
        pWriter.WriteInt(voucherItemId);
        pWriter.WriteBool(error);

        return pWriter;
    }

    public static ByteWriter StartList() {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.StartList);

        return pWriter;
    }

    public static ByteWriter ListCount(short count) {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.ListCount);
        pWriter.WriteShort(count);

        return pWriter;
    }

    public static ByteWriter ListHair(IList<Item> hairs) {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.ListHair);
        pWriter.WriteShort((short) hairs.Count);
        for (int i = 0; i < hairs.Count; i++) {
            Item hair = hairs[i];
            if (hair.Appearance is not HairAppearance appearance) {
                throw new InvalidOperationException($"Loading invalid hair: {hair.Id},{hair.Uid}");
            }

            pWriter.WriteInt(hair.Id);
            pWriter.WriteLong(hair.Uid);
            pWriter.WriteInt(i);
            pWriter.WriteLong(hair.CreationTime);
            pWriter.WriteClass<HairAppearance>(appearance);
        }

        return pWriter;
    }

    public static ByteWriter SaveHair(Item currentHair, Item hairCopy) {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.SaveHair);
        pWriter.WriteLong(currentHair.Uid);
        pWriter.WriteLong(hairCopy.Uid);
        pWriter.WriteByte();
        pWriter.WriteLong(hairCopy.CreationTime);

        return pWriter;
    }

    public static ByteWriter DeleteHair(long uid) {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.DeleteHair);
        pWriter.WriteLong(uid);

        return pWriter;
    }

    public static ByteWriter SaveSlots(short extraSlots) {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.SaveSlots);
        pWriter.WriteByte(); // TODO: unknown
        pWriter.WriteShort(extraSlots);

        return pWriter;
    }

    public static ByteWriter ApplySavedHair() {
        var pWriter = Packet.Of(SendOp.Beauty);
        pWriter.Write<Command>(Command.ApplySavedHair);

        return pWriter;
    }

    private static void WriteBeautyShopItems(this IByteWriter pWriter, BeautyShopItem[] items) {
        pWriter.WriteByte(); // Unknown1
        pWriter.WriteByte(); // Unknown2
        pWriter.WriteLong(); // Unknown3
        pWriter.WriteShort((short) items.Length);
        foreach (BeautyShopItem item in items) {
            pWriter.WriteInt(item.Id);
            pWriter.Write<ShopItemLabel>(item.SaleTag);
            pWriter.WriteShort(item.RequiredLevel);
            pWriter.WriteInt(item.AchievementId);
            pWriter.WriteByte(item.AchievementRank);
            pWriter.WriteClass<BeautyShopCost>(item.Cost);
        }
    }
}
