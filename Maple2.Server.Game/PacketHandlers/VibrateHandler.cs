using System.Numerics;
using Maple2.Database.Storage;
using Maple2.Model.Metadata.FieldEntity;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Model.Skill;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class VibrateHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.Vibrate;

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required SkillMetadataStorage SkillMetadata { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        string entityId = packet.ReadString();
        long skillUid = packet.ReadLong();
        int skillId = packet.ReadInt();
        short level = packet.ReadShort();
        byte motionPoint = packet.ReadByte();
        byte attackPoint = packet.ReadByte();

        SkillRecord? record = session.Player.ActiveSkills.Get(skillUid);
        if (record == null) {
            Logger.Warning("Invalid Skill {SkillUid}", skillUid);
            return;
        }

        DamageRecord damage = new(record.Metadata, record.Attack) {
            CasterId = session.Player.ObjectId,
            TargetUid = record.TargetUid,
            OwnerId = session.Player.ObjectId,
            SkillId = record.SkillId,
            Level = record.Level,
            MotionPoint = record.MotionPoint,
            AttackPoint = record.AttackPoint,
            Position = record.Position,
            Direction = record.Direction,
        };

        record.ServerTick = packet.ReadInt();
        record.Position = packet.Read<Vector3>();

        FieldVibrateEntity? vibrate = session.Field?.AccelerationStructure?.GetVibrateEntity(entityId);
        if (vibrate != null && vibrate.BreakDefense < record.Attack.BrokenOffence) {
            //TODO: Keep a record of when the vibrate was broken.
        }

        // Packet gets sent regardless
        session.Field?.Broadcast(VibratePacket.Attack(entityId, damage));
    }
}
