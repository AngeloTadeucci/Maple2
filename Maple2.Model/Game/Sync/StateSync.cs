using System;
using System.Numerics;
using System.Text;
using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class StateSync : IByteSerializable, IByteDeserializable {
    [Flags]
    public enum Flag : byte {
        None = 0,
        Flag1 = 1,
        Flag2 = 2,
        Flag3 = 4,
        Flag4 = 8,
        Flag5 = 16,
        Flag6 = 32,
    }

    public ActorState State;
    public ActorSubState SubState;
    public Flag Flags;

    public Vector3 Position;
    public short Rotation;
    public byte Animation;
    public float UnknownFloat1;
    public float UnknownFloat2;
    public Vector3 Speed;
    public byte Unknown1; // Always 2...
    public short Rotation2; // Rotation * 10
    public short Unknown3;
    public int SyncNumber;

    #region Flag1
    public int EmotionId; // Skill ID for emotes + flying mounts (not ground)
    public short Flag1Unknown2; // Goes to 1 for flying mounts
    #endregion

    #region Flag2
    public Vector3 Flag2Unknown1;
    public string? Flag2Unknown2;
    #endregion

    #region Flag3
    public int Flag3Unknown1;
    public string? Flag3Unknown2;
    #endregion

    #region Flag4
    public string? Flag4Animation;
    #endregion

    #region Flag5
    public int Flag5Unknown1;
    public string? Flag5Unknown2;
    #endregion

    #region Flag6
    public int Flag6Unknown1;
    public int Flag6Unknown2;
    public byte Flag6Unknown3;
    public Vector3 Flag6Position;
    public Vector3 Flag6Rotation;
    #endregion

    public virtual void WriteTo(IByteWriter writer) {
        writer.Write<ActorState>(State);
        writer.Write<ActorSubState>(SubState);
        writer.Write<Flag>(Flags);

        if (Flags.HasFlag(Flag.Flag1)) {
            writer.WriteInt(EmotionId);
            writer.WriteShort(Flag1Unknown2);
        }

        writer.Write<Vector3S>(Position);
        writer.WriteShort(Rotation);
        writer.WriteByte(Animation);

        if (Animation > 127) {
            writer.WriteFloat(UnknownFloat1);
            writer.WriteFloat(UnknownFloat2);
        }

        writer.Write<Vector3S>(Speed);
        writer.WriteByte(Unknown1);
        writer.WriteShort(Rotation2);
        writer.WriteShort(Unknown3);
        if (Flags.HasFlag(Flag.Flag2)) {
            writer.Write<Vector3>(Flag2Unknown1);
            writer.WriteUnicodeString(Flag2Unknown2 ?? "");
        }
        if (Flags.HasFlag(Flag.Flag3)) {
            writer.WriteInt(Flag3Unknown1);
            writer.WriteUnicodeString(Flag3Unknown2 ?? "");
        }
        if (Flags.HasFlag(Flag.Flag4)) {
            writer.WriteUnicodeString(Flag4Animation ?? "");
        }
        if (Flags.HasFlag(Flag.Flag5)) {
            writer.WriteInt(Flag5Unknown1);
            writer.WriteUnicodeString(Flag5Unknown2 ?? "");
        }
        if (Flags.HasFlag(Flag.Flag6)) {
            writer.WriteInt(Flag6Unknown1);
            writer.WriteInt(Flag6Unknown2);
            writer.WriteByte(Flag6Unknown3);
            writer.Write<Vector3>(Flag6Position);
            writer.Write<Vector3>(Flag6Rotation);
        }

        writer.WriteInt(SyncNumber);
    }

    public virtual void ReadFrom(IByteReader reader) {
        State = reader.Read<ActorState>();
        SubState = reader.Read<ActorSubState>();
        Flags = reader.Read<Flag>();

        if (Flags.HasFlag(Flag.Flag1)) {
            EmotionId = reader.ReadInt();
            Flag1Unknown2 = reader.ReadShort();
        }

        Position = reader.Read<Vector3S>();
        Rotation = reader.ReadShort(); // CoordS / 10 (Rotation?)
        Animation = reader.ReadByte();
        if (Animation > 127) { // if animation < 0 (signed)
            UnknownFloat1 = reader.ReadFloat();
            UnknownFloat2 = reader.ReadFloat();
        }
        Speed = reader.Read<Vector3S>(); // XYZ Speed?
        Unknown1 = reader.ReadByte();
        Rotation2 = reader.ReadShort(); // CoordS / 10
        Unknown3 = reader.ReadShort(); // CoordS / 1000

        if (Flags.HasFlag(Flag.Flag2)) {
            Flag2Unknown1 = reader.Read<Vector3>();
            Flag2Unknown2 = reader.ReadUnicodeString();
        }
        if (Flags.HasFlag(Flag.Flag3)) {
            Flag3Unknown1 = reader.ReadInt();
            Flag3Unknown2 = reader.ReadUnicodeString();
        }
        if (Flags.HasFlag(Flag.Flag4)) {
            Flag4Animation = reader.ReadUnicodeString();
        }
        if (Flags.HasFlag(Flag.Flag5)) {
            Flag5Unknown1 = reader.ReadInt();
            Flag5Unknown2 = reader.ReadUnicodeString();
        }
        if (Flags.HasFlag(Flag.Flag6)) {
            Flag6Unknown1 = reader.ReadInt();
            Flag6Unknown2 = reader.ReadInt();
            Flag6Unknown3 = reader.ReadByte();
            Flag6Position = reader.Read<Vector3>();
            Flag6Rotation = reader.Read<Vector3>();
        }

        SyncNumber = reader.ReadInt();
    }

    public override string ToString() {
        var builder = new StringBuilder();
        builder.AppendLine($"State:{State}, SubState:{SubState}, SyncNumber{SyncNumber}");
        builder.AppendLine($" Position:{Position}, Rotation:{Rotation}, Speed:{Speed}");
        builder.AppendLine($" Animation:{Animation} ({UnknownFloat1}, {UnknownFloat2}), Unknown1:{Unknown1}, Rotation2:{Rotation2}, Unknown3:{Unknown3}");
        if (Flags.HasFlag(Flag.Flag1)) {
            builder.Append($"Flag1: {EmotionId}, {Flag1Unknown2}");
        }
        if (Flags.HasFlag(Flag.Flag2)) {
            builder.Append($"Flag2: {Flag2Unknown1}, {Flag2Unknown2}");
        }
        if (Flags.HasFlag(Flag.Flag3)) {
            builder.Append($"Flag3: {Flag3Unknown1}, {Flag3Unknown2}");
        }
        if (Flags.HasFlag(Flag.Flag4)) {
            builder.Append($"Flag4: {Flag4Animation}");
        }
        if (Flags.HasFlag(Flag.Flag5)) {
            builder.Append($"Flag5: {Flag5Unknown1}, {Flag5Unknown2}");
        }
        if (Flags.HasFlag(Flag.Flag6)) {
            builder.Append($"Flag6: {Flag6Unknown1}, {Flag6Unknown2}, {Flag6Unknown3}");
            builder.Append($"- Position:{Flag6Position}, Rotation:{Flag6Rotation}");
        }

        return builder.ToString();
    }
}
