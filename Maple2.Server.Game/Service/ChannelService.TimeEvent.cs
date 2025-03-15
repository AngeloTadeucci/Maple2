using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Server.Channel.Service;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<TimeEventResponse> TimeEvent(TimeEventRequest request, ServerCallContext context) {
        switch (request.TimeEventCase) {
            case TimeEventRequest.TimeEventOneofCase.AnnounceGlobalPortal:
                return Task.FromResult(AnnounceGlobalPortal(request.AnnounceGlobalPortal));
            case TimeEventRequest.TimeEventOneofCase.CloseGlobalPortal:
                return Task.FromResult(CloseGlobalPortal(request.CloseGlobalPortal));
            case TimeEventRequest.TimeEventOneofCase.GetField:
                return Task.FromResult(GetField(request.GetField));
            default:
                return Task.FromResult(new TimeEventResponse());
        }
    }

    private TimeEventResponse AnnounceGlobalPortal(TimeEventRequest.Types.AnnounceGlobalPortal portal) {
        if (!serverTableMetadata.TimeEventTable.GlobalPortal.TryGetValue(portal.MetadataId, out GlobalPortalMetadata? metadata)) {
            return new TimeEventResponse();
        }
        foreach (GameSession session in server.GetSessions()) {
            if (session.Field.Metadata.Property.Type is >= MapType.None and <= MapType.Telescope or >= MapType.Alikar and <= MapType.Shelter) {
                session.Send(GlobalPortalPacket.Announce(metadata, portal.EventId));
            }
        }
        return new TimeEventResponse();
    }

    private TimeEventResponse CloseGlobalPortal(TimeEventRequest.Types.CloseGlobalPortal portal) {
        foreach (GameSession session in server.GetSessions()) {
            session.Send(GlobalPortalPacket.Close(portal.EventId));
        }
        return new TimeEventResponse();
    }

    private TimeEventResponse GetField(TimeEventRequest.Types.GetField field) {
        FieldManager? manager = server.GetField(field.MapId, field.RoomId);
        if (manager == null) {
            return new TimeEventResponse();
        }

        return new TimeEventResponse {
            Field = new FieldInfo {
                MapId = manager.MapId,
                RoomId = manager.RoomId,
                OwnerId = manager is HomeFieldManager homeManager ? homeManager.OwnerId : 0,
                PlayerIds = {
                    manager.Players.Values.Select(player => player.Value.Character.Id),
                },
            },
        };
    }
}

