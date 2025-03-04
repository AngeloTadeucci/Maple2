using System.Net;
using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Party;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.Game.Model.Room;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.World.Service;
using Serilog;
using MigrationType = Maple2.Server.World.Service.MigrationType;

namespace Maple2.Server.Game.Manager;

public class DungeonManager : IDisposable {
    private readonly GameSession session;

    public DungeonFieldManager? Field { get; set; }
    public DungeonRoomTable.DungeonRoomMetadata? Metadata;
    private int LobbyRoomId { get; set; }

    private Party? Party => session.Party.Party;

    private readonly CancellationTokenSource tokenSource;

    private readonly ILogger logger = Log.Logger.ForContext<DungeonManager>();

    public DungeonManager(GameSession session) {
        this.session = session;
        tokenSource = new CancellationTokenSource();
    }

    public void SetDungeon(DungeonFieldManager field) {
        Field = field;
        Metadata = field.Metadata;
    }

    public void CreateDungeonRoom(int dungeonId, bool withParty) {
        if (!session.TableMetadata.DungeonRoomTable.Entries.TryGetValue(dungeonId, out DungeonRoomTable.DungeonRoomMetadata? metadata)) {
            session.Send(DungeonRoomPacket.Error(DungeonRoomError.s_room_dungeon_notOpenTimeDungeon));
            return;
        }

        if (withParty) {
            if (Party == null) {
                session.Send(DungeonRoomPacket.Error(DungeonRoomError.s_room_dungeon_error_invalidPartyOID));
                return;
            }
            if (Party.LeaderCharacterId != session.CharacterId) {
                session.Send(DungeonRoomPacket.Error(DungeonRoomError.s_room_party_err_not_chief));
                return;
            }
        }


        DungeonFieldManager? dungeonField = session.FieldFactory.CreateDungeon(metadata, session.CharacterId, Party);
        if (dungeonField == null) {
            session.Send(DungeonRoomPacket.Error(DungeonRoomError.s_room_dungeon_NotAllowTime));
            return;
        }
        LobbyRoomId = dungeonField.RoomId;

        SetDungeon(dungeonField);

        if (withParty) {
            var request = new PartyRequest {
                RequestorId = session.CharacterId,
                SetDungeon = new PartyRequest.Types.SetDungeon {
                    PartyId = Party!.Id,
                    DungeonId = dungeonId,
                    DungeonRoomId = dungeonField.RoomId,
                },
            };

            try {
                session.World.Party(request);
            } catch (RpcException) { }
        }

        MigrateToDungeon();
    }

    public void EnterLobby() {
        if (Party == null) {
            return;
        }
        FieldManager? field = session.Field.FieldFactory.Get(roomId: Party.DungeonLobbyRoomId);
        if (field is not DungeonFieldManager dungeonField) {
            return;
        }

        SetDungeon(dungeonField);
        MigrateToDungeon();
    }

    public void EnterInitField() {
        if (Field == null || Metadata == null) {
            logger.Error("Field is null, cannot enter dungeon");
            return;
        }

        if (!Field.RoomFields.TryGetValue(Metadata.FieldIds[0], out DungeonFieldManager? firstField)) {
            logger.Error("First field is null, cannot enter dungeon");
            return;
        }

        session.Send(session.PrepareField(firstField.MapId, roomId: firstField.RoomId) ? FieldEnterPacket.Request(session.Player) :
            FieldEnterPacket.Error(MigrationError.s_move_err_default));
    }

    public void SetDungeon(int dungeonId, int roomId) {
        if (!session.TableMetadata.DungeonRoomTable.Entries.TryGetValue(dungeonId, out DungeonRoomTable.DungeonRoomMetadata? metadata)) {
            logger.Error("Dungeon metadata not found for dungeonId {dungeonId}", dungeonId);
            return;
        }

        Metadata = metadata;
        LobbyRoomId = roomId;
    }

    private void MigrateToDungeon() {
        if (Metadata == null) {
            logger.Error("Dungeon metadata is null, cannot migrate to dungeon");
            return;
        }
        try {
            var request = new MigrateOutRequest {
                AccountId = session.AccountId,
                CharacterId = session.CharacterId,
                MachineId = session.MachineId.ToString(),
                Server = Server.World.Service.Server.Game,
                MapId = Metadata.LobbyFieldId,
                RoomId = LobbyRoomId,
                Type = MigrationType.Dungeon,
            };

            MigrateOutResponse response = session.World.MigrateOut(request);
            var endpoint = new IPEndPoint(IPAddress.Parse(response.IpAddress), response.Port);
            session.Send(MigrationPacket.GameToGame(endpoint, response.Token, Metadata.LobbyFieldId));
            session.State = SessionState.ChangeMap;
        } catch (RpcException ex) {
            session.Send(MigrationPacket.GameToGameError(MigrationError.s_move_err_default));
            session.Send(NoticePacket.Disconnect(new InterfaceText(ex.Message)));
        } finally {
            session.Disconnect();
        }
    }


    public void Dispose() {
        session.Dispose();
        tokenSource.Dispose();
    }
}
