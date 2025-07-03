using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class BlackMarketPacket {
    private enum Command : byte {
        Error = 0,
        MyListings = 1,
        Add = 2,
        Remove = 3,
        Search = 4,
        Purchase = 5,
        PurchaseResponse = 7,
        Preview = 8,
    }

    public static ByteWriter Error(BlackMarketError error, int arg1 = 0, int arg2 = 0) {
        var pWriter = Packet.Of(SendOp.BlackMarket);
        pWriter.Write<Command>(Command.Error);
        pWriter.WriteByte();
        pWriter.Write<BlackMarketError>(error);
        pWriter.WriteLong();
        pWriter.WriteInt(arg1);
        pWriter.WriteInt(arg2);

        return pWriter;
    }

    public static ByteWriter MyListings(ICollection<BlackMarketListing> listings) {
        var pWriter = Packet.Of(SendOp.BlackMarket);
        pWriter.Write<Command>(Command.MyListings);
        pWriter.WriteInt(listings.Count);
        foreach (BlackMarketListing listing in listings) {
            pWriter.WriteClass<BlackMarketListing>(listing);
        }

        return pWriter;
    }

    public static ByteWriter Add(BlackMarketListing listing) {
        var pWriter = Packet.Of(SendOp.BlackMarket);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteClass<BlackMarketListing>(listing);

        return pWriter;
    }

    public static ByteWriter Remove(long listingId) {
        var pWriter = Packet.Of(SendOp.BlackMarket);
        pWriter.Write<Command>(Command.Remove);
        pWriter.WriteLong(listingId);
        pWriter.WriteByte();

        return pWriter;
    }

    public static ByteWriter Search(ICollection<BlackMarketListing> listings) {
        var pWriter = Packet.Of(SendOp.BlackMarket);
        pWriter.Write<Command>(Command.Search);
        pWriter.WriteInt(listings.Count);
        foreach (BlackMarketListing listing in listings) {
            pWriter.WriteClass<BlackMarketListing>(listing);
        }

        return pWriter;
    }

    public static ByteWriter Purchase(long listingId, int amount) {
        var pWriter = Packet.Of(SendOp.BlackMarket);
        pWriter.Write<Command>(Command.Purchase);
        pWriter.WriteLong(listingId);
        pWriter.WriteInt(amount);

        return pWriter;
    }

    public static ByteWriter PurchaseResponse() {
        var pWriter = Packet.Of(SendOp.BlackMarket);
        pWriter.Write<Command>(Command.PurchaseResponse);

        return pWriter;
    }

    public static ByteWriter Preview(int itemId, int rarity, long npcPrice) {
        var pWriter = Packet.Of(SendOp.BlackMarket);
        pWriter.Write<Command>(Command.Preview);
        pWriter.WriteInt(itemId);
        pWriter.WriteInt(rarity);
        pWriter.WriteLong(npcPrice);

        return pWriter;
    }
}
