using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class StateSkillHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.StateSkill;

    public override void Handle(GameSession session, IByteReader packet) {
        if (session.Field is null) return;
        byte function = packet.ReadByte();
        if (function != 0) {
            Logger.Warning("Unhandled StateSkill function: {Function}", function);
            return;
        }

        long skillCastUid = packet.ReadLong();
        int serverTick = packet.ReadInt();
        int skillId = packet.ReadInt();
        short skillLevel = packet.ReadShort();
        var state = (ActorState) packet.ReadInt();
        int clientTick = packet.ReadInt();
        long itemUid = packet.ReadLong();

        if (itemUid != 0 && session.Item.Inventory.Get(itemUid) == null) {
            return; // Invalid item
        }

        if (!session.Field.SkillMetadata.TryGet(skillId, skillLevel, out SkillMetadata? metadata)) {
            return;
        }

        var cast = new SkillRecord(metadata, skillCastUid, session.Player);
        if (!session.Player.SkillCastConsume(cast)) {
            return;
        }

        cast.StateNextTick = session.Field.FieldTick + (int) TimeSpan.FromSeconds(cast.Motion.MotionProperty.SequenceSpeed).TotalMilliseconds;
        session.Player.ActiveSkills.Add(cast);
        session.Field.Broadcast(SkillPacket.StateSkill(session.Player, skillId, skillCastUid));
    }
}
