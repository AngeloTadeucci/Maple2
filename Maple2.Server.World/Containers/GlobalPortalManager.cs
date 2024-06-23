using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Channel.Service;
using ChannelClient = Maple2.Server.Channel.Service.Channel.ChannelClient;

namespace Maple2.Server.World.Containers;

public class GlobalPortalManager : IDisposable {
    public required GameStorage GameStorage { get; init; }
    public required ServerTableMetadataStorage ServerTableMetadata { get; init; }
    public required ChannelClientLookup ChannelClients { get; init; }

    public readonly GlobalPortal Portal;
    public int Channel;

    public readonly int[] InstanceIds;


    public GlobalPortalManager(GlobalPortalMetadata metadata, int id, long endTick) {
        Portal = new GlobalPortal(metadata, id) {
            EndTick = endTick,
        };
        InstanceIds = new int[metadata.Entries.Length];
    }

    public void CreateFields() {
        if (!ChannelClients.TryGetClient(ChannelClients.FirstChannel(), out ChannelClient? client)) {
            return;
        }

        Channel = ChannelClients.FirstChannel();

        for (int i = 0; i < InstanceIds.Length; i++) {
            TimeEventResponse? response = client.TimeEvent(new TimeEventRequest {
                GetField = new TimeEventRequest.Types.GetField {
                    MapId = Portal.Metadata.Entries[i].MapId,
                    InstanceId = 0,
                },
            });

            if (response.Field == null) {
                continue;
            }
            InstanceIds[i] = response.Field.InstanceId;
        }

        foreach ((int channelId, ChannelClient channelClient) in ChannelClients) {
            channelClient.TimeEvent(new TimeEventRequest {
                AnnounceGlobalPortal = new TimeEventRequest.Types.AnnounceGlobalPortal {
                    MetadataId = Portal.MetadataId,
                    EventId = Portal.Id,
                },
            });
        }
    }

    public void Join(int mapId, int index) {
        if (!ChannelClients.TryGetClient(Channel, out ChannelClient? client)) {
            return;
        }

        // Check if current field has reached capacity. If so, create a new field.
        GlobalPortalMetadata.Field globalPortalMetadata = Portal.Metadata.Entries[index];
        TimeEventResponse response = client.TimeEvent(new TimeEventRequest {
            GetField = new TimeEventRequest.Types.GetField {
                MapId = globalPortalMetadata.MapId,
                InstanceId = InstanceIds[index],
            },
        });

        if (!ServerTableMetadata.InstanceFieldTable.Entries.TryGetValue(mapId, out InstanceFieldMetadata? instanceFieldMetadata)) {
            return;
        }

        if (response.Field.PlayerIds.Count >= instanceFieldMetadata.MaxCount) {
            TimeEventResponse? createResponse = client.TimeEvent(new TimeEventRequest {
                GetField = new TimeEventRequest.Types.GetField {
                    MapId = mapId,
                    InstanceId = 0,
                },
            });

            if (createResponse.Field == null) {
                return;
            }
            InstanceIds[index] = createResponse.Field.InstanceId;
        }
    }

    public void Dispose() {
        foreach ((int channelId, ChannelClient channelClient) in ChannelClients) {
            channelClient.TimeEvent(new TimeEventRequest {
                CloseGlobalPortal = new TimeEventRequest.Types.CloseGlobalPortal {
                    EventId = Portal.MetadataId,
                },
            });
        }
    }
}
