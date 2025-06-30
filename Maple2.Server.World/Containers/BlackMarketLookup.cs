using System.Collections.Concurrent;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;

namespace Maple2.Server.World.Containers;

public class BlackMarketLookup : IDisposable {
    private readonly GameStorage gameStorage;

    private readonly ConcurrentDictionary<long, BlackMarketListing> listings;

    public BlackMarketLookup(GameStorage gameStorage) {
        this.gameStorage = gameStorage;
        listings = new ConcurrentDictionary<long, BlackMarketListing>();

        // Load all listings from the database.
        using GameStorage.Request db = gameStorage.Context();
        List<BlackMarketListing> allListings = db.GetAllBlackMarketListings().ToList();
        foreach (BlackMarketListing listing in allListings) {
            listings.TryAdd(listing.Id, listing);
        }

    }

    public ICollection<long> Search(string[] categories, int minLevel, int maxLevel, JobFilterFlag jobFilterFlag, int rarity, int minEnchantLevel, int maxEnchantLevel, int minSocketCount,
                                    int maxSocketCount, string name, int startPage, BlackMarketSort sortBy, Dictionary<BasicAttribute, BasicOption> basicOptions, Dictionary<SpecialAttribute, SpecialOption> specialOptions) {
        IList<BlackMarketListing> results = new List<BlackMarketListing>();

        foreach (BlackMarketListing listing in listings.Values) {
            if (listing.ExpiryTime < DateTime.UtcNow.ToEpochSeconds()) {
                continue;
            }

            if (!categories.Contains(listing.Item.Metadata.Property.BlackMarketCategory)) {
                continue;
            }

            if (!string.IsNullOrEmpty(name) && !listing.Item.Metadata.Name?.Contains(name) == true) {
                continue;
            }

            if (listing.Item.Metadata.Limit.Level < minLevel || listing.Item.Metadata.Limit.Level > maxLevel) {
                continue;
            }

            if (listing.Item.Metadata.Limit.JobRecommends.Length != 0 &&
                !listing.Item.Metadata.Limit.JobRecommends.Contains(JobCode.None) &&
                (jobFilterFlag & listing.Item.Metadata.Limit.JobRecommends.FilterFlags()) == JobFilterFlag.None) {
                continue;
            }

            if (rarity > 0 && listing.Item.Rarity < rarity) {
                continue;
            }

            if (minEnchantLevel > 0 && listing.Item.Enchant?.Enchants < minEnchantLevel) {
                continue;
            }

            if (maxEnchantLevel > 0 && listing.Item.Enchant?.Enchants > maxEnchantLevel) {
                continue;
            }

            if (minSocketCount > 0 && listing.Item.Socket?.Sockets.Length < minSocketCount) {
                continue;
            }

            if (maxSocketCount > 0 && listing.Item.Socket?.Sockets.Length > maxSocketCount) {
                continue;
            }

            if (listing.Item.Stats != null && !ItemStatCheck(listing.Item.Stats, basicOptions, specialOptions)) {
                continue;
            }

            results.Add(listing);
        }

        // Sort
        results = sortBy switch {
            BlackMarketSort.PriceAscending => results.OrderBy(listing => listing.Price).ToList(),
            BlackMarketSort.PriceDescending => results.OrderByDescending(listing => listing.Price).ToList(),
            BlackMarketSort.LevelAscending => results.OrderBy(listing => listing.Item.Metadata.Limit.Level).ToList(),
            BlackMarketSort.LevelDescending => results.OrderByDescending(listing => listing.Item.Metadata.Limit.Level).ToList(),
            _ => results,
        };

        // Paging
        const int itemsPerPage = 7;
        const int numPages = 10;
        int offset = startPage * itemsPerPage - itemsPerPage;
        return results
            .Skip(offset)
            .Take(numPages * itemsPerPage + Math.Min(0, offset))
            .Select(listing => listing.Id)
            .ToList();
    }

    private bool ItemStatCheck(ItemStats stats, Dictionary<BasicAttribute, BasicOption> basicOptions, Dictionary<SpecialAttribute, SpecialOption> specialOptions) {
        foreach (KeyValuePair<BasicAttribute, BasicOption> basicOption in basicOptions) {
            float rate = 0f;
            int value = 0;

            foreach (ItemStats.Type type in Enum.GetValues(typeof(ItemStats.Type))) {
                if (stats[type].Basic.TryGetValue(basicOption.Key, out BasicOption option)) {
                    value += option.Value;
                }
            }

            if (value < basicOption.Value.Value) {
                return false;
            }
        }

        foreach (KeyValuePair<SpecialAttribute, SpecialOption> specialOption in specialOptions) {
            float rate = 0f;
            float value = 0;

            foreach (ItemStats.Type type in Enum.GetValues(typeof(ItemStats.Type))) {
                if (stats[type].Special.TryGetValue(specialOption.Key, out SpecialOption option)) {
                    rate += option.Rate;
                }
            }

            if (rate < specialOption.Value.Rate) {
                return false;
            }
        }

        return true;
    }

    public BlackMarketError Add(long listingId) {
        using GameStorage.Request db = gameStorage.Context();
        BlackMarketListing? listing = db.GetBlackMarketListing(listingId);
        if (listing == null) {
            return BlackMarketError.s_blackmarket_error_fail_register;
        }

        listings.TryAdd(listingId, listing);
        return BlackMarketError.none;
    }

    public BlackMarketError Remove(long listingId) {
        if (!listings.TryRemove(listingId, out _)) {
            return BlackMarketError.s_blackmarket_error_close;
        }

        using GameStorage.Request db = gameStorage.Context();
        if (!db.DeleteBlackMarketListing(listingId)) {
            return BlackMarketError.s_blackmarket_error_close;
        }

        return BlackMarketError.none;
    }

    public BlackMarketError Purchase(long listingId) {
        using GameStorage.Request db = gameStorage.Context();

        BlackMarketListing? listing = db.GetBlackMarketListing(listingId);

        if (listing == null) {
            listings.TryRemove(listingId, out _);
            return BlackMarketError.none;
        }

        listings[listingId] = listing;
        return BlackMarketError.none;
    }

    public void Dispose() {
        foreach (BlackMarketListing listing in listings.Values) {
            using GameStorage.Request db = gameStorage.Context();
            db.SaveItems(listing.Id, listing.Item);
        }
    }
}
