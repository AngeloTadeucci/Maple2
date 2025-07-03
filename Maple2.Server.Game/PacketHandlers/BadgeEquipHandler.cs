using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Packets;
using Maple2.Model.Game;

namespace Maple2.Server.Game.PacketHandlers;

public class BadgeEquipHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.BadgeEquip;

    private enum Command : byte {
        Equip = 0,
        Unequip = 1,
        Transparency = 3,
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
            case Command.Transparency:
                HandleTransparency(session, packet);
                return;
        }
    }

    private void HandleEquip(GameSession session, IByteReader packet) {
        long itemUid = packet.ReadLong();
        packet.Read<BadgeType>(); // We are using static slots based on badge type

        // Disconnect if this fails to avoid bad state.
        if (!session.Item.Equips.EquipBadge(itemUid)) {
            session.Disconnect();
        }
    }

    private void HandleUnequip(GameSession session, IByteReader packet) {
        var slot = packet.Read<BadgeType>();

        // Disconnect if this fails to avoid bad state.
        if (!session.Item.Equips.UnequipBadge(slot)) {
            session.Disconnect();
        }
    }

    private void HandleTransparency(GameSession session, IByteReader packet) {
        var badgeType = packet.Read<BadgeType>();

        Item? badgeItem = session.Item.Equips.Get(badgeType);

        if (badgeItem?.Badge == null || badgeItem.Badge.Type != BadgeType.Transparency) {
            session.Disconnect();
            return;
        }

        for (int i = 0; i < badgeItem.Badge.Transparency.Length; i++) {
            badgeItem.Badge.Transparency[i] = packet.ReadBool();
        }

        session.Field?.Broadcast(EquipPacket.EquipBadge(session.Player, badgeItem));
    }
}
