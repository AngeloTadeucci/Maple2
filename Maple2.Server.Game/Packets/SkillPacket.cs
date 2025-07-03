using System.Numerics;
using Maple2.Model.Common;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Model.Skill;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class SkillPacket {
    public static ByteWriter Use(SkillRecord skill) {
        var pWriter = Packet.Of(SendOp.SkillUse);
        pWriter.WriteLong(skill.CastUid);
        pWriter.WriteInt(skill.ServerTick);
        pWriter.WriteInt(skill.Caster.ObjectId);
        pWriter.WriteInt(skill.SkillId);
        pWriter.WriteShort(skill.Level);
        pWriter.WriteByte(skill.MotionPoint);
        pWriter.Write<Vector3S>(skill.Position);
        pWriter.Write<Vector3>(skill.Direction);
        pWriter.Write<Vector3>(skill.Rotation);
        pWriter.WriteShort((short) (skill.Rotate2Z * 10));
        pWriter.WriteBool(skill.Unknown);
        pWriter.WriteBool(skill.IsHold);
        if (skill.IsHold) {
            pWriter.WriteInt(skill.HoldInt);
            pWriter.WriteUnicodeString(skill.HoldString);
        }

        return pWriter;
    }

    public static ByteWriter Sync(SkillRecord skill) {
        var pWriter = Packet.Of(SendOp.SkillSync);
        pWriter.WriteLong(skill.CastUid);
        pWriter.WriteInt(skill.Caster.ObjectId);
        pWriter.WriteInt(skill.SkillId);
        pWriter.WriteShort(skill.Level);
        pWriter.WriteByte(skill.MotionPoint);
        pWriter.Write<Vector3>(skill.Position);
        pWriter.Write<Vector3>(skill.Direction);
        pWriter.Write<Vector3>(skill.Rotation);
        pWriter.Write<Vector3>(default);
        pWriter.WriteByte();
        pWriter.WriteByte(skill.AttackPoint);
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter Cancel(SkillRecord skill) {
        var pWriter = Packet.Of(SendOp.SkillCancel);
        pWriter.WriteLong(skill.CastUid);
        pWriter.WriteInt(skill.Caster.ObjectId);

        return pWriter;
    }

    public static ByteWriter Fail(SkillRecord skill) {
        var pWriter = Packet.Of(SendOp.SkillUseFailed);
        pWriter.WriteByte(); // Type?
        pWriter.WriteLong(skill.CastUid);
        pWriter.WriteInt(skill.Caster.ObjectId);
        pWriter.WriteByte(); // Unknown

        // TODO: Handle Type=5, Type=6

        pWriter.WriteInt();         // EndTick?
        pWriter.WriteShort(0 * 10); // Unk * 10
        pWriter.WriteInt();         // Same CooldownUnk in SkillCooldown packet?

        return pWriter;
    }

    public static ByteWriter StateSkill(FieldPlayer player, int skillId, long skillCastUid) {
        var pWriter = Packet.Of(SendOp.StateSkill);
        pWriter.WriteByte(0x06);
        pWriter.WriteInt(player.ObjectId);
        pWriter.WriteInt(); // ItemId??
        pWriter.WriteLong(skillCastUid);
        pWriter.WriteInt(skillId);
        pWriter.WriteInt((int) player.State);

        return pWriter;
    }

    public static ByteWriter Cooldown(params SkillCooldown[] cooldowns) {
        var pWriter = Packet.Of(SendOp.SkillCooldown);
        pWriter.WriteByte((byte) cooldowns.Length);
        foreach (SkillCooldown cooldown in cooldowns) {
            pWriter.WriteClass<SkillCooldown>(cooldown);
        }

        return pWriter;
    }

    public static ByteWriter ResetCooldown() {
        var pWriter = Packet.Of(SendOp.SkillResetCooldown);

        return pWriter;
    }

    public static ByteWriter InBattle(FieldPlayer player) {
        var pWriter = Packet.Of(SendOp.UserBattle);
        pWriter.WriteInt(player.ObjectId);
        pWriter.WriteBool(player.InBattle);

        return pWriter;
    }
}
