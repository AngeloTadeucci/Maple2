using Maple2.Model.Common;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model.Skill;

namespace Maple2.Server.Game.Packets;

public static class VibratePacket {
    private enum Command : byte {
        Attack = 1,
        Invoke = 2,
    }

    public static ByteWriter Attack(string entityId, SkillRecord skill) {
        var pWriter = Packet.Of(SendOp.Vibrate);
        pWriter.Write<Command>(Command.Attack);
        pWriter.WriteString(entityId);
        pWriter.WriteLong(skill.CastUid);
        pWriter.WriteInt(skill.SkillId);
        pWriter.WriteShort(skill.Level);
        pWriter.WriteByte(skill.MotionPoint);
        pWriter.WriteByte(skill.AttackPoint);
        pWriter.Write<Vector3S>(skill.Position);
        pWriter.WriteInt(Environment.TickCount);
        pWriter.WriteString();
        pWriter.WriteByte();

        return pWriter;
    }

    public static ByteWriter Invoke(string entityId) {
        var pWriter = Packet.Of(SendOp.Vibrate);
        pWriter.Write<Command>(Command.Invoke);
        pWriter.WriteString(entityId);
        pWriter.WriteString();
        pWriter.WriteByte();

        return pWriter;
    }

    public static ByteWriter Hide(string entityId) {
        var pWriter = Packet.Of(SendOp.HideVibrate);
        pWriter.WriteString(entityId);

        return pWriter;
    }

    public static ByteWriter Show(string entityId) {
        var pWriter = Packet.Of(SendOp.ShowVibrate);
        pWriter.WriteString(entityId);

        return pWriter;
    }
}
