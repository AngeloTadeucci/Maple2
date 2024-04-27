using Maple2.Database.Storage;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class BonusGameHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.BonusGame;

    private enum Command : byte {
        Load = 0,
        Spin = 2,
        Close = 3,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required TableMetadataStorage TableMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Load:
                HandleLoad(session, packet);
                break;
        }
    }

    private void HandleLoad(GameSession session, IByteReader packet) {
        int wheelId = packet.ReadInt();

        //TODO: Fetch the rewards for the specific wheel and send to player
        session.Send(BonusGamePacket.Load());
    }
}
