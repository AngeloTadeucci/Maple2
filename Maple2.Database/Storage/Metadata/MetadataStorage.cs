using Caching;
using Maple2.Database.Context;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public abstract class MetadataStorage<TK, TV> {
    protected readonly MetadataContext Context;
    protected readonly LRUCache<TK, TV> Cache;

    protected MetadataStorage(MetadataContext context, int capacity) {
        Context = context;
        Cache = new LRUCache<TK, TV>(capacity, (int) (capacity * 0.05));
    }

    public virtual void InvalidateCache() {
        Cache.Clear();
    }
}

public interface ISearchable<T> where T : ISearchResult {
    public List<T> Search(string name);
}
