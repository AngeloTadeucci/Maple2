using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Maple2.Database.Extensions;
using Maple2.Database.Storage;
using Maple2.Model;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;

namespace Maple2.Server.World.Containers;

public class BlackMarketLookup : IDisposable {
    public const int BATCH_SIZE = 70;
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

            bool check1 = listing.Item.Metadata.Limit.JobRecommends.Length > 0;
            bool check2 = !listing.Item.Metadata.Limit.JobRecommends.Contains(JobCode.None);
            bool check3 = (jobFilterFlag & listing.Item.Metadata.Limit.JobRecommends.FilterFlags()) != JobFilterFlag.None;
            var itemFlag = listing.Item.Metadata.Limit.JobRecommends.FilterFlags();


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

            if (listing.Item.Stats != null) {

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

    public BlackMarketError Add(long listingId) {
        using GameStorage.Request db = gameStorage.Context();
        BlackMarketListing? listing = db.GetBlackMarketListing(listingId);
        if (listing == null) {
            return BlackMarketError.s_blackmarket_error_fail_register;
        }

        listings.TryAdd(listingId, listing);
        return BlackMarketError.none;
    }

    public void Dispose() {
    }
}
