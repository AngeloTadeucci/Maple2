using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class ItemEquipHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.ItemEquip;

    private enum Command : byte {
        Equip = 0,
        Unequip = 1,
        Equip2 = 2,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Equip:
                HandleEquip(session, packet);
                return;
            case Command.Unequip:
                HandleUnequip(session, packet);
                return;
            case Command.Equip2:
                return;
        }
    }

    private void HandleEquip(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        var equipSlot = packet.Read<EquipSlot>();
        bool isSkin = packet.ReadBool();

        // Disconnect if this fails to avoid bad state.
        if (session.Item.Equips.Equip(itemUid, equipSlot, isSkin)) {
            session.Stats.Refresh();
        }
    }

    private void HandleUnequip(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();

        // Disconnect if this fails to avoid bad state.
        if (!session.Item.Equips.Unequip(itemUid)) {
            Logger.Fatal("Failed to unequip item: {Uid}", itemUid);
            session.Disconnect();
        }
    }

    // This is probably for Skin2
    private void HandleEquip2(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        var equipSlot = packet.Read<EquipSlot>();
    }
}
