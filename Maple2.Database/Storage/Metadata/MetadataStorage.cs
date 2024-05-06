using System.Collections.Generic;
using Caching;
using Maple2.Database.Context;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public abstract class MetadataStorage<TK, TV>(MetadataContext context, int capacity) {
    protected readonly MetadataContext Context = context;
    protected readonly LRUCache<TK, TV> Cache = new(capacity, (int) (capacity * 0.05));

    public virtual void InvalidateCache() {
        Cache.Clear();
    }
}

public interface ISearchable<T> where T : ISearchResult {
    public List<T> Search(string name);
}
