using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class StateSkillHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.StateSkill;

    public override void Handle(GameSession session, IByteReader packet) {
        byte function = packet.ReadByte();
        if (function != 0 || session.Field == null) {
            return;
        }

        long skillCastUid = packet.ReadLong();
        int serverTick = packet.ReadInt();
        int skillId = packet.ReadInt();
        packet.ReadShort(); // 1
        session.Player.State = (ActorState) packet.ReadInt();
        int clientTick = packet.ReadInt();
        long itemUid = packet.ReadLong();

        if (itemUid != 0 && session.Item.Inventory.Get(itemUid) == null) {
            return; // Invalid item
        }

        session.Field.Broadcast(SkillPacket.StateSkill(session.Player, skillId, skillCastUid));
    }
}
