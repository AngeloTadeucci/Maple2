using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Storage;
using Maple2.Model.Metadata;

namespace Maple2.Server.World.Containers;

public class GlobalPortalLookup : IDisposable {
    private readonly GameStorage gameStorage;
    private readonly ServerTableMetadataStorage serverTableMetadata;
    private readonly ChannelClientLookup channelClients;
    private GlobalPortalManager? globalPortalManager; // only one event max should be active at a time.

    private int nextEventId = 1;

    public GlobalPortalLookup(ChannelClientLookup channelClients, GameStorage gameStorage, ServerTableMetadataStorage serverTableMetadata) {
        this.gameStorage = gameStorage;
        this.channelClients = channelClients;
        this.serverTableMetadata = serverTableMetadata;
    }

    public bool TryGet([NotNullWhen(true)] out GlobalPortalManager? globalEvent) {
        if (globalPortalManager != null) {
            globalEvent = globalPortalManager;
            return true;
        }

        globalEvent = null;
        return false;
    }

    public void Create(GlobalPortalMetadata metadata, long endTick, out int eventId) {
        eventId = nextEventId++;
        globalPortalManager = new GlobalPortalManager(metadata, eventId, endTick) {
            ChannelClients = channelClients,
            GameStorage = gameStorage,
            ServerTableMetadata = serverTableMetadata,
        };
    }

    public void Dispose() {
        if (globalPortalManager == null) {
            return;
        }
        globalPortalManager.Dispose();
        globalPortalManager = null;
    }
}
