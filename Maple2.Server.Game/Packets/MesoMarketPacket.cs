using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class MesoMarketPacket {
    private enum Command : byte {
        Error = 0,
        Load = 1,
        Quota = 2,
        MyListings = 4,
        Create = 5,
        Cancel = 6,
        Search = 7,
        Purchase = 8,
    }

    public static ByteWriter Error(MesoMarketError error) {
        var pWriter = Packet.Of(SendOp.MesoMarket);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<MesoMarketError>(error);

        return pWriter;
    }

    public static ByteWriter Load(long averagePrice) {
        var pWriter = Packet.Of(SendOp.MesoMarket);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteFloat(Constant.MesoMarketTaxRate);
        pWriter.WriteFloat(Constant.MesoMarketRangeRate);
        pWriter.WriteLong(averagePrice);
        pWriter.WriteInt(Constant.MesoMarketListLimit);
        pWriter.WriteInt(Constant.MesoMarketListLimitDay);
        pWriter.WriteInt(Constant.MesoMarketPurchaseLimitMonth);
        pWriter.WriteInt(Constant.MesoMarketSellEndDay);
        pWriter.WriteInt(Constant.MesoMarketPageSize);
        pWriter.WriteInt(Constant.MesoMarketMinToken);
        pWriter.WriteInt(Constant.MesoMarketMaxToken);

        return pWriter;
    }

    public static ByteWriter Quota(int dailyListed, int monthlyPurchased) {
        var pWriter = Packet.Of(SendOp.MesoMarket);
        pWriter.Write<Command>(Command.Quota);
        pWriter.WriteInt(dailyListed);
        pWriter.WriteInt(monthlyPurchased);

        return pWriter;
    }

    public static ByteWriter MyListings(ICollection<MesoListing> listings) {
        var pWriter = Packet.Of(SendOp.MesoMarket);
        pWriter.Write<Command>(Command.MyListings);
        pWriter.WriteInt(listings.Count);
        foreach (MesoListing listing in listings) {
            pWriter.WriteLong(listing.Id);
            pWriter.WriteClass<MesoListing>(listing);
            pWriter.WriteBool(true);
        }

        return pWriter;
    }

    public static ByteWriter Create(MesoListing listing) {
        var pWriter = Packet.Of(SendOp.MesoMarket);
        pWriter.Write<Command>(Command.Create);
        pWriter.WriteClass<MesoListing>(listing);
        pWriter.WriteBool(true); // Self listing
        pWriter.WriteInt(1); // Amount? always 1

        return pWriter;
    }

    public static ByteWriter Cancel(long listingId, MesoMarketError error = MesoMarketError.none) {
        var pWriter = Packet.Of(SendOp.MesoMarket);
        pWriter.Write<Command>(Command.Cancel);
        pWriter.Write<MesoMarketError>(error);
        pWriter.WriteLong(listingId);

        return pWriter;
    }

    public static ByteWriter Search(long accountId, ICollection<MesoListing> listings) {
        var pWriter = Packet.Of(SendOp.MesoMarket);
        pWriter.Write<Command>(Command.Search);
        pWriter.WriteInt(listings.Count);
        foreach (MesoListing listing in listings) {
            pWriter.WriteClass<MesoListing>(listing);
            pWriter.WriteBool(listing.AccountId == accountId);
        }

        return pWriter;
    }

    public static ByteWriter Purchase(long listingId, MesoMarketError error = MesoMarketError.none) {
        var pWriter = Packet.Of(SendOp.MesoMarket);
        pWriter.Write<Command>(Command.Purchase);
        pWriter.Write<MesoMarketError>(error);
        pWriter.WriteLong(listingId);
        pWriter.WriteInt(1); // Amount? always 1

        return pWriter;
    }
}
