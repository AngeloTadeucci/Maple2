using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Context;
using Maple2.Model.Metadata;

namespace Maple2.Database.Storage;

public class TriggerScriptMetadata(MetadataContext context) : MetadataStorage<(string, string), TriggerMetadata>(context, CACHE_SIZE) {
    private const int CACHE_SIZE = 5000; // ~5k total triggers

    public bool TryGet(string mapXBlock, string triggerName, [NotNullWhen(true)] out TriggerMetadata? trigger) {
        if (Cache.TryGet((mapXBlock, triggerName), out trigger)) {
            return true;
        }

        lock (Context) {
            // Double-checked locking
            if (Cache.TryGet((mapXBlock, triggerName), out trigger)) {
                return true;
            }

            trigger = Context.TriggerMetadata.Find(mapXBlock, triggerName);

            if (trigger == null) {
                return false;
            }

            Cache.AddReplace((mapXBlock, triggerName), trigger);
        }

        return true;
    }
}
