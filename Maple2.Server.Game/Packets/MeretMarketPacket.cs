using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class MeretMarketPacket {
    private enum Command : byte {
        LoadPersonalListings = 11,
        LoadSales = 12,
        ListItem = 13,
        RemoveListing = 14,
        UnlistItem = 15,
        RelistItem = 18,
        CollectProfit = 20,
        UpdateExpiration = 21,
        LoadDesigners = 22,
        AddDesigner = 23,
        LoadDesignerItems = 25,
        RemoveDesigner = 24,
        UpdateProfit = 26, // unconfirmed
        LoadItems = 27,
        LoadBlueprints = 28,
        Purchase = 30,
        FailPurchase = 31,
        FeaturedPremium = 101,
        FeaturedUgc = 102,
        LoadCart = 107,
        Unknown201 = 201,
    }

    public static ByteWriter LoadPersonalListings(ICollection<UgcMarketItem> items) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.LoadPersonalListings);
        pWriter.WriteLong();
        pWriter.WriteInt(items.Count);

        foreach (UgcMarketItem item in items) {
            pWriter.WriteBool(true); // Is Market Item
            pWriter.WriteBool(true); // Is UGC Item
            pWriter.WriteClass<UgcMarketItem>(item);
        }

        return pWriter;
    }

    public static ByteWriter LoadSales(ICollection<SoldUgcMarketItem> items) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.LoadSales);
        pWriter.WriteInt(items.Count);
        foreach (SoldUgcMarketItem item in items) {
            pWriter.WriteClass<SoldUgcMarketItem>(item);
        }

        return pWriter;
    }

    public static ByteWriter ListItem(MarketItem item) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.ListItem);
        pWriter.WriteMarketItem(item);

        return pWriter;
    }

    public static ByteWriter RemoveListing(long id) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.RemoveListing);
        pWriter.WriteInt();
        pWriter.WriteLong(id);
        pWriter.WriteLong(id);

        return pWriter;
    }

    public static ByteWriter RelistItem(UgcMarketItem item) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.RelistItem);
        pWriter.WriteBool(true); // Is Market Item
        pWriter.WriteClass<UgcMarketItem>(item);

        return pWriter;
    }

    public static ByteWriter UpdateProfit(long saleId) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.UpdateProfit);
        pWriter.WriteLong(saleId);
        pWriter.WriteLong(saleId);

        return pWriter;
    }

    public static ByteWriter UpdateExpiration(UgcMarketItem item) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.UpdateExpiration);
        pWriter.WriteInt();
        pWriter.WriteLong(item.Id);
        pWriter.WriteLong(item.Id);
        pWriter.Write<UgcMarketListingStatus>(item.Status);
        pWriter.WriteByte();
        pWriter.WriteLong(item.ListingEndTime);

        return pWriter;
    }

    public static ByteWriter LoadDesigners(ICollection<PlayerInfo> designers) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.LoadDesigners);
        pWriter.WriteByte((byte) designers.Count);
        foreach (PlayerInfo designer in designers) {
            pWriter.WriteLong(designer.AccountId);
            pWriter.WriteLong(designer.CharacterId);
            pWriter.WriteUnicodeString(designer.Name);
            pWriter.WriteUnicodeString(designer.Picture);
        }

        return pWriter;
    }

    public static ByteWriter AddDesigner(PlayerInfo playerInfo, IList<UgcMarketItem> items) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.AddDesigner);
        pWriter.WriteLong(playerInfo.AccountId);
        pWriter.WriteLong(playerInfo.CharacterId);
        pWriter.WriteUnicodeString(playerInfo.Name);
        pWriter.WriteUnicodeString(playerInfo.Picture);
        pWriter.WriteInt(items.Count);
        foreach (UgcMarketItem item in items) {
            pWriter.WriteMarketItem(item);
        }

        return pWriter;
    }

    public static ByteWriter LoadDesignerItems(PlayerInfo playerInfo, IList<UgcMarketItem> items) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.LoadDesignerItems);
        pWriter.WriteLong(playerInfo.AccountId);
        pWriter.WriteLong(playerInfo.CharacterId);
        pWriter.WriteUnicodeString(playerInfo.Name);
        pWriter.WriteUnicodeString(playerInfo.Picture);
        pWriter.WriteInt(items.Count);
        foreach (UgcMarketItem item in items) {
            pWriter.WriteMarketItem(item);
        }

        return pWriter;
    }

    public static ByteWriter RemoveDesigner(long characterId) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.RemoveDesigner);
        pWriter.WriteLong(characterId);

        return pWriter;
    }

    public static ByteWriter LoadItems(ICollection<MarketItem> marketItems, int totalItems, int startPage) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.LoadItems);
        pWriter.WriteInt(marketItems.Count);
        pWriter.WriteInt(totalItems);
        pWriter.WriteInt(startPage);
        foreach (MarketItem entry in marketItems) {
            pWriter.WriteBool(true);
            pWriter.WriteMarketItem(entry);
        }

        return pWriter;
    }

    public static ByteWriter LoadBlueprints(ICollection<MarketItem> marketItems, int totalItems, byte itemsPerPage, int startPage) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.LoadBlueprints);
        pWriter.WriteInt(marketItems.Count);
        pWriter.WriteInt(totalItems);
        pWriter.WriteByte(itemsPerPage);
        pWriter.WriteInt(startPage);
        foreach (MarketItem entry in marketItems) {
            pWriter.WriteBool(true);
            pWriter.WriteMarketItem(entry);
        }

        return pWriter;
    }

    public static ByteWriter Purchase(int totalQuantity, int itemIndex, long price, int premiumMarketId, long ugcMarketItemId) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.Purchase);
        pWriter.WriteByte((byte) totalQuantity);
        pWriter.WriteInt(premiumMarketId);
        pWriter.WriteLong(ugcMarketItemId);
        pWriter.WriteInt();
        pWriter.WriteInt();
        pWriter.WriteLong();
        pWriter.WriteInt(itemIndex);
        pWriter.WriteInt(totalQuantity);
        pWriter.WriteInt();
        pWriter.WriteByte(); // true if gift
        pWriter.WriteUnicodeString(); // player name
        pWriter.WriteUnicodeString(); // message
        pWriter.WriteLong(price);
        pWriter.WriteByte();
        pWriter.WriteByte();
        pWriter.WriteInt();
        pWriter.WriteInt();
        pWriter.WriteInt();
        pWriter.WriteInt(); // error code?
        return pWriter;
    }

    public static ByteWriter FeaturedPremium(byte section, byte tabId, byte marketSlots, IList<MarketItem> marketItems) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.FeaturedPremium);
        pWriter.WriteByte(section);
        pWriter.WriteByte(tabId);
        pWriter.WriteByte();
        pWriter.WriteByte(marketSlots);
        for (int i = 0; i < marketSlots; i++) {
            MarketItem? item = marketItems.ElementAtOrDefault(i);
            pWriter.WriteBool(item != null);
            if (item == null) {
                break;
            }
            pWriter.WriteMarketItem(item);
        }
        return pWriter;
    }

    public static ByteWriter FeaturedUgc(ICollection<UgcMarketItem> promoItems, ICollection<UgcMarketItem> newItems) {
        var pWriter = Packet.Of(SendOp.MeretMarket);
        pWriter.Write<Command>(Command.FeaturedUgc);
        pWriter.WriteInt(promoItems.Count + newItems.Count);
        foreach (UgcMarketItem promoItem in promoItems) {
            pWriter.WriteBool(true);
            pWriter.WriteMarketItem(promoItem);
        }
        foreach (UgcMarketItem newItem in newItems) {
            pWriter.WriteBool(true);
            pWriter.WriteMarketItem(newItem);
        }
        return pWriter;
    }

    #region Helper
    private static void WriteMarketItem(this IByteWriter pWriter, MarketItem item) {
        pWriter.WriteBool(item is UgcMarketItem);
        switch (item) {
            case PremiumMarketItem premium:
                pWriter.WriteInt(premium.Id);
                pWriter.WriteLong();
                pWriter.WriteClass<PremiumMarketItem>(premium);
                pWriter.WriteBool(premium.PromoData != null);
                if (premium.PromoData != null) {
                    pWriter.WriteClass<PremiumMarketPromoData>(premium.PromoData);
                }
                pWriter.WriteByte();
                pWriter.WriteBool(premium.Metadata.ShowSaleTime);
                pWriter.WriteInt();
                pWriter.WriteByte((byte) premium.AdditionalQuantities.Count);
                foreach (PremiumMarketItem addEntry in premium.AdditionalQuantities) {
                    pWriter.WriteBool(true);
                    pWriter.WriteClass<PremiumMarketItem>(addEntry);
                    bool unk = false;
                    pWriter.WriteBool(unk);
                    if (unk) {
                        pWriter.WriteString();
                        pWriter.WriteLong();
                        pWriter.WriteUnicodeString();
                        pWriter.WriteLong();
                    }
                    pWriter.WriteByte();
                    pWriter.WriteByte();
                }
                break;
            case UgcMarketItem ugc:
                pWriter.WriteClass<UgcMarketItem>(ugc);
                break;
        }
    }
    #endregion
}
