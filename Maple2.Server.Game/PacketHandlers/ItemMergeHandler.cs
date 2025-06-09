using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class ItemMergeHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.ItemMerge;

    private enum Command : byte {
        Stage = 0,
        SelectCrystal = 1,
        Empower = 2,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Stage:
                HandleStage(session, packet);
                break;
            case Command.SelectCrystal:
                HandleSelectCrystal(session, packet);
                break;
            case Command.Empower:
                HandleEmpower(session, packet);
                break;
        }
    }

    private void HandleStage(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();

        session.ItemMerge.Stage(itemUid);
    }

    private void HandleSelectCrystal(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        long catalystUid = packet.ReadLong();

        session.ItemMerge.SelectCrystal(itemUid, catalystUid);
    }

    private void HandleEmpower(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        long catalystUid = packet.ReadLong();

        session.ItemMerge.Empower(itemUid, catalystUid);

    }
}
