using Maple2.Model.Error;
using Maple2.Model.Game.Shop;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class ShopPacket {
    private enum Command : byte {
        Open = 0,
        LoadItems = 1,
        Update = 2,
        Buy = 4,
        BuyBackItemCount = 6,
        LoadBuyBack = 7,
        RemoveBuyBack = 8,
        InstantRestock = 9,
        LoadNew = 14,
        Error = 15,
    }

    public static ByteWriter Open(Shop shop, int npcId = 0) {
        var pWriter = Packet.Of(SendOp.Shop);
        pWriter.Write<Command>(Command.Open);
        pWriter.WriteInt(npcId);
        pWriter.WriteClass<Shop>(shop);

        return pWriter;
    }

    public static ByteWriter LoadItems(ICollection<ShopItem> items) {
        var pWriter = Packet.Of(SendOp.Shop);
        pWriter.Write<Command>(Command.LoadItems);
        pWriter.WriteByte((byte) items.Count);
        foreach (ShopItem item in items) {
            pWriter.WriteClass<ShopItem>(item);
        }

        return pWriter;
    }

    public static ByteWriter Update(int id, int totalQuantityPurchased) {
        var pWriter = Packet.Of(SendOp.Shop);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteInt(id);
        pWriter.WriteInt(totalQuantityPurchased);

        return pWriter;
    }

    public static ByteWriter Buy(ShopItem shopItem, int quantity, int price, bool toGuildStorage = false) {
        var pWriter = Packet.Of(SendOp.Shop);
        pWriter.Write<Command>(Command.Buy);
        pWriter.WriteInt(shopItem.ItemId);
        pWriter.WriteInt(quantity);
        pWriter.WriteInt(price * quantity);
        pWriter.WriteByte(shopItem.Rarity);
        pWriter.WriteBool(toGuildStorage);

        return pWriter;
    }

    public static ByteWriter BuyBackItemCount(short itemCount) {
        var pWriter = Packet.Of(SendOp.Shop);
        pWriter.Write<Command>(Command.BuyBackItemCount);
        pWriter.WriteShort(itemCount);

        return pWriter;
    }

    public static ByteWriter LoadBuyBackItem(params BuyBackItem[] buyBackItems) {
        var pWriter = Packet.Of(SendOp.Shop);
        pWriter.Write<Command>(Command.LoadBuyBack);
        pWriter.WriteShort((short) buyBackItems.Length);
        foreach (BuyBackItem item in buyBackItems) {
            pWriter.WriteClass<BuyBackItem>(item);
        }

        return pWriter;
    }

    public static ByteWriter RemoveBuyBackItem(int buyBackId) {
        var pWriter = Packet.Of(SendOp.Shop);
        pWriter.Write<Command>(Command.RemoveBuyBack);
        pWriter.WriteInt(buyBackId);

        return pWriter;
    }

    public static ByteWriter InstantRestock(bool unknown = false) {
        var pWriter = Packet.Of(SendOp.Shop);
        pWriter.Write<Command>(Command.InstantRestock);
        pWriter.WriteBool(unknown);
        if (unknown) {
            pWriter.WriteInt();
            pWriter.WriteInt();
        }

        return pWriter;
    }

    public static ByteWriter Error(ShopError error, int stringId = 0) {
        var pWriter = Packet.Of(SendOp.Shop);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<ShopError>(error);
        pWriter.WriteByte();
        pWriter.WriteInt(stringId);

        return pWriter;
    }
}
