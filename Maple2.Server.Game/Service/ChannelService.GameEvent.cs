using Grpc.Core;
using Maple2.Model.Game.Event;
using Maple2.Model.Metadata;
using Maple2.Server.Channel.Service;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<GameEventResponse> GameEvent(GameEventRequest request, ServerCallContext context) {
        switch (request.GameEventCase) {
            case GameEventRequest.GameEventOneofCase.Add:
                return Task.FromResult(Add(request.Add));
            case GameEventRequest.GameEventOneofCase.Update:
                return Task.FromResult(new GameEventResponse());
            case GameEventRequest.GameEventOneofCase.Remove:
                return Task.FromResult(Remove(request.Remove));
            default:
                return Task.FromResult(new GameEventResponse());
        }
    }

    private GameEventResponse Add(GameEventRequest.Types.Add add) {
        if (!serverTableMetadata.GameEventTable.Entries.TryGetValue(add.EventId, out GameEventMetadata? eventData)) {
            return new GameEventResponse();
        }

        server.AddEvent(new GameEvent(eventData));
        return new GameEventResponse();
    }

    private GameEventResponse Remove(GameEventRequest.Types.Remove remove) {
        if (!serverTableMetadata.GameEventTable.Entries.TryGetValue(remove.EventId, out GameEventMetadata? eventData)) {
            return new GameEventResponse();
        }

        server.RemoveEvent(eventData.Id);
        return new GameEventResponse();
    }
}

