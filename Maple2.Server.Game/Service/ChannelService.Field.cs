using Grpc.Core;
using Maple2.Model.Error;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<FieldResponse> Field(FieldRequest request, ServerCallContext context) {
        switch (request.FieldCase) {
            case FieldRequest.FieldOneofCase.CreateDungeon:
                return Task.FromResult(CreateDungeon(request.CreateDungeon, request.RequesterId));
            case FieldRequest.FieldOneofCase.DestroyDungeon:
                return Task.FromResult(DestroyDungeon(request.DestroyDungeon));
            default:
                return Task.FromResult(new FieldResponse());
        }
    }

    private FieldResponse CreateDungeon(FieldRequest.Types.CreateDungeon create, long requestorId) {
        if (!tableMetadata.DungeonRoomTable.Entries.TryGetValue(create.DungeonId, out DungeonRoomMetadata? dungeonRoom)) {
            return new FieldResponse {
                Error = (int) MigrationError.s_move_err_dungeon_not_exist,
            };
        }

        DungeonFieldManager? dungeonField = server.CreateDungeon(dungeonRoom, requestorId, create.Size, create.PartyId);
        if (dungeonField == null) {
            return new FieldResponse {
                Error = (int) MigrationError.s_move_err_FailCreateDungeon,
            };
        }

        return new FieldResponse {
            Error = (int) MigrationError.ok,
            RoomId = dungeonField.RoomId,
        };
    }

    private FieldResponse DestroyDungeon(FieldRequest.Types.DestroyDungeon destroy) {
        MigrationError error = server.DestroyDungeon(destroy.RoomId);

        return new FieldResponse {
            Error = (int) error,
        };
    }
}
