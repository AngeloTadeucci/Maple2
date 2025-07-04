using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Context;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class RideMetadataStorage(MetadataContext context) : MetadataStorage<int, RideMetadata>(context, CACHE_SIZE) {
    private const int CACHE_SIZE = 500; // ~500 total items

    public bool TryGet(int id, [NotNullWhen(true)] out RideMetadata? ride) {
        if (Cache.TryGet(id, out ride)) {
            return true;
        }

        lock (Context) {
            // Double-checked locking
            if (Cache.TryGet(id, out ride)) {
                return true;
            }

            ride = Context.RideMetadata.Find(id);

            if (ride == null) {
                return false;
            }

            Cache.AddReplace(id, ride);
        }

        return true;
    }
}
