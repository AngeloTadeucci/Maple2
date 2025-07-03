using System.Diagnostics.CodeAnalysis;
using Caching;
using Maple2.Database.Context;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Microsoft.EntityFrameworkCore;

namespace Maple2.Database.Storage;

public class MapMetadataStorage : MetadataStorage<int, MapMetadata>, ISearchable<MapMetadata> {
    private const int CACHE_SIZE = 1500; // ~1.1k total Maps
    private const int UGC_CACHE_SIZE = 200;

    protected readonly LRUCache<int, UgcMapMetadata> UgcCache;
    protected readonly Dictionary<string, ExportedUgcMapMetadata> ExportedUgcCache; // only 17 entries

    public MapMetadataStorage(MetadataContext context) : base(context, CACHE_SIZE) {
        UgcCache = new LRUCache<int, UgcMapMetadata>(UGC_CACHE_SIZE, (int) (UGC_CACHE_SIZE * 0.05));
        ExportedUgcCache = [];

        foreach (ExportedUgcMapMetadata exportedUgcMap in context.ExportedUgcMapMetadata) {
            ExportedUgcCache.Add(exportedUgcMap.Id, exportedUgcMap);
        }
    }

    public bool TryGet(int id, [NotNullWhen(true)] out MapMetadata? map) {
        if (Cache.TryGet(id, out map)) {
            return true;
        }

        lock (Context) {
            // Double-checked locking
            if (Cache.TryGet(id, out map)) {
                return true;
            }

            map = Context.MapMetadata.Find(id);

            if (map == null) {
                return false;
            }

            Cache.AddReplace(id, map);
        }

        return true;
    }

    public bool TryGetUgc(int id, [NotNullWhen(true)] out UgcMapMetadata? map) {
        if (UgcCache.TryGet(id, out map)) {
            return true;
        }

        lock (Context) {
            map = Context.UgcMapMetadata.Find(id);
        }

        if (map == null) {
            return false;
        }

        UgcCache.AddReplace(id, map);
        return true;
    }

    public IList<MapMetadata> GetMapsByType(Continent continent, MapType mapType) {
        lock (Context) {
            return Context.MapMetadata.FromSqlRaw($"SELECT * FROM `map` WHERE JSON_EXTRACT(Property, '$.Type')={(int) mapType} AND JSON_EXTRACT(Property, '$.Continent')={(int) continent}")
                .ToList();
        }
    }

    public IEnumerable<UgcMapMetadata> GetAllUgc() {
        lock (Context) {
            foreach (UgcMapMetadata map in Context.UgcMapMetadata) {
                UgcCache.AddReplace(map.Id, map);
                yield return map;
            }
        }
    }

    public List<MapMetadata> Search(string name) {
        lock (Context) {
            return Context.MapMetadata
                .Where(map => EF.Functions.Like(map.Name!, $"%{name}%"))
                .ToList();
        }
    }

    public bool TryGetExportedUgc(string id, [NotNullWhen(true)] out ExportedUgcMapMetadata? map) {
        return ExportedUgcCache.TryGetValue(id, out map);
    }
}
