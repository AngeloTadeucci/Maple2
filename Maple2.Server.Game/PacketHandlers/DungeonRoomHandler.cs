using Maple2.Database.Storage;
using Maple2.Model.Error;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class DungeonRoomHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.RoomDungeon;

    private enum Command : byte {
        Reset = 1,
        Create = 2,
        EnterLobby = 3,
        EnterField = 10,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required TableMetadataStorage TableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Reset:
                HandleReset(session);
                break;
            case Command.Create:
                HandleCreate(session, packet);
                break;
            case Command.EnterLobby:
                HandleEnter(session, packet);
                break;
            case Command.EnterField:
                HandleEnterField(session);
                break;
        }
    }

    private void HandleReset(GameSession session) {
    }

    private void HandleCreate(GameSession session, IByteReader packet) {
        int dungeonId = packet.ReadInt();
        bool withParty = packet.ReadBool();
        int unknown = packet.ReadInt();
        byte unknown2 = packet.ReadByte();

        session.Dungeon.CreateDungeonRoom(dungeonId, withParty);
    }

    private void HandleEnter(GameSession session, IByteReader packet) {
        int unknown = packet.ReadInt();
        session.Dungeon.EnterLobby();
    }

    private void HandleEnterField(GameSession session) {
        // enter first field of dungeon
        session.Dungeon.EnterInitField();
    }
}
