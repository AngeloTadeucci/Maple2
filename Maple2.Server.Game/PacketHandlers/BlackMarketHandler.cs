using Maple2.Database.Storage;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.World.Service;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;

namespace Maple2.Server.Game.PacketHandlers;

public class BlackMarketHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.BlackMarket;

    private enum Command : byte {
        MyListings = 1,
        Add = 2,
        Remove = 3,
        Search = 4,
        Purchase = 5,
        Preview = 8,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required TableMetadataStorage TableMetadata { private get; init; }
    public required WorldClient World { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.MyListings:
                HandleMyListings(session);
                break;
            case Command.Add:
                HandleAdd(session, packet);
                break;
            case Command.Remove:
                HandleRemove(session, packet);
                break;
            case Command.Search:
                HandleSearch(session, packet);
                break;
            case Command.Purchase:
                HandlePurchase(session, packet);
                break;
            case Command.Preview:
                HandlePreview(session, packet);
                break;
        }
    }

    private void HandleMyListings(GameSession session) {
        session.BlackMarket.LoadMyListings();
    }

    private void HandleAdd(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        long price = packet.ReadLong();
        int quantity = packet.ReadInt();

        session.BlackMarket.Add(itemUid, price, quantity);
    }

    private void HandleRemove(GameSession session, IByteReader packet) {
        long listingId = packet.ReadLong();

        session.BlackMarket.Remove(listingId);
    }

    private void HandleSearch(GameSession session, IByteReader packet) {
        int minCategoryId = packet.ReadInt();
        int maxCategoryId = packet.ReadInt();
        int minLevel = packet.ReadInt();
        int maxLevel = packet.ReadInt();
        int job = packet.ReadInt();
        int rarity = packet.ReadInt();
        int minEnchantLevel = packet.ReadInt();
        int maxEnchantLevel = packet.ReadInt();
        byte minSockets = packet.ReadByte();
        byte maxSockets = packet.ReadByte();
        string name = packet.ReadUnicodeString();
        int startPage = packet.ReadInt();
        byte sort = packet.ReadByte();
        packet.ReadByte();
        packet.ReadInt();
        packet.ReadInt();
        packet.ReadByte();
        packet.ReadByte(); // always 1? possibly the stat_option table id

        var statOptions = new List<StatOption>();
        for (int statIndex = 0; statIndex < 3; statIndex++) {
            int statId = packet.ReadInt();
            int value = packet.ReadInt();
            if (value == 0) {
                continue;
            }
            statOptions.Add(new StatOption {
                StatId = statId,
                Value = value,
            });
        }

        var categories = new List<string>();
        foreach (KeyValuePair<int, string[]> entry in TableMetadata.BlackMarketTable.Entries) {
            if (entry.Key >= minCategoryId && entry.Key <= maxCategoryId) {
                categories.AddRange(entry.Value);
            }
        }

        BlackMarketResponse response = World.BlackMarket(new BlackMarketRequest {
            Search = new BlackMarketRequest.Types.Search {
                Categories = {
                    categories,
                },
                JobFlag = job,
                MinEnchantLevel = minEnchantLevel,
                MaxEnchantLevel = maxEnchantLevel,
                MinLevel = minLevel,
                MaxLevel = maxLevel,
                MinSocketCount = minSockets,
                MaxSocketCount = maxSockets,
                Name = name,
                Rarity = rarity,
                StartPage = startPage,
                SortBy = sort,
                StatOptions = {
                    statOptions,
                },
            },
        });

        var error = (BlackMarketError) response.Error;
        if (error != BlackMarketError.none) {
            session.Send(BlackMarketPacket.Error(error));
            return;
        }

        using GameStorage.Request db = session.GameStorage.Context();
        List<BlackMarketListing> listings = db.GetBlackMarketListings(response.Search.ListingIds.ToArray()).ToList();
        session.Send(BlackMarketPacket.Search(listings));
    }

    private void HandlePurchase(GameSession session, IByteReader packet) {
        long listingId = packet.ReadLong();
        int amount = packet.ReadInt();

        session.BlackMarket.Purchase(listingId, amount);
    }

    private void HandlePreview(GameSession session, IByteReader packet) {
        int itemId = packet.ReadInt();
        int rarity = packet.ReadInt();

        session.BlackMarket.Preview(itemId, rarity);
    }
}
