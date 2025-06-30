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
        Animation = 8,
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
    public int Flag1Unknown;
    #endregion

    #region Flag2
    public int Flag2Unknown;
    #endregion

    #region Flag3
    public short Flag3Unknown;
    #endregion

    #region AnimationFlag
    public string? AnimationName;
    #endregion

    #region Flag5
    public Vector3 Flag5Unknown1;
    public string? Flag5Unknown2;
    #endregion

    #region Flag6
    public int Flag6Unknown1;
    public string? Flag6Unknown2;
    #endregion

    public virtual void WriteTo(IByteWriter writer) {
        writer.Write<ActorState>(State);
        writer.Write<ActorSubState>(SubState);
        writer.Write<Vector3S>(Position);
        writer.WriteShort(Rotation);
        writer.WriteByte(Animation);

        if (Animation == 128) {
            writer.WriteFloat(UnknownFloat1);
            writer.WriteFloat(UnknownFloat2);
        }

        writer.Write<Vector3S>(Speed);
        writer.WriteByte(Unknown1);
        writer.WriteShort(Rotation2);
        writer.WriteShort(Unknown3);

        writer.Write<Flag>(Flags);
        if (Flags.HasFlag(Flag.Flag1)) {
            writer.WriteInt(Flag1Unknown);
        }
        if (Flags.HasFlag(Flag.Flag2)) {
            writer.WriteInt(Flag2Unknown);
        }
        if (Flags.HasFlag(Flag.Flag3)) {
            writer.WriteShort(Flag3Unknown);
        }
        if (Flags.HasFlag(Flag.Animation)) {
            writer.WriteUnicodeString(AnimationName ?? "");
        }
        if (Flags.HasFlag(Flag.Flag5)) {
            writer.Write<Vector3>(Flag5Unknown1);
            writer.WriteUnicodeString(Flag5Unknown2 ?? "");
        }
        if (Flags.HasFlag(Flag.Flag6)) {
            writer.WriteInt(Flag6Unknown1);
            writer.WriteUnicodeString(Flag6Unknown2 ?? "");
        }
        writer.WriteInt(SyncNumber);
    }

    public virtual void ReadFrom(IByteReader reader) {
        State = reader.Read<ActorState>();
        SubState = reader.Read<ActorSubState>();

        Position = reader.Read<Vector3S>();
        Rotation = reader.ReadShort(); // CoordS / 10 (Rotation?)
        Animation = reader.ReadByte();
        if (Animation == 128) {
            UnknownFloat1 = reader.ReadFloat();
            UnknownFloat2 = reader.ReadFloat();
        }
        Speed = reader.Read<Vector3S>(); // XYZ Speed?
        Unknown1 = reader.ReadByte();
        Rotation2 = reader.ReadShort(); // CoordS / 10
        Unknown3 = reader.ReadShort(); // CoordS / 1000

        Flags = reader.Read<Flag>();
        if (Flags.HasFlag(Flag.Flag1)) {
            Flag1Unknown = reader.ReadInt();
        }
        if (Flags.HasFlag(Flag.Flag2)) {
            Flag2Unknown = reader.ReadInt();
        }
        if (Flags.HasFlag(Flag.Flag3)) {
            Flag3Unknown = reader.ReadShort();
        }
        if (Flags.HasFlag(Flag.Animation)) {
            AnimationName = reader.ReadUnicodeString();
        }
        if (Flags.HasFlag(Flag.Flag5)) {
            Flag5Unknown1 = reader.Read<Vector3>();
            Flag5Unknown2 = reader.ReadUnicodeString();
        }
        if (Flags.HasFlag(Flag.Flag6)) {
            Flag6Unknown1 = reader.ReadInt();
            Flag6Unknown2 = reader.ReadUnicodeString();
        }

        SyncNumber = reader.ReadInt();
    }

    public override string ToString() {
        var builder = new StringBuilder();
        builder.AppendLine($"State:{State}, SubState:{SubState}, SyncNumber{SyncNumber}");
        builder.AppendLine($" Position:{Position}, Rotation:{Rotation}, Speed:{Speed}");
        builder.AppendLine($" Animation:{Animation} ({UnknownFloat1}, {UnknownFloat2}), Unknown1:{Unknown1}, Rotation2:{Rotation2}, Unknown3:{Unknown3}");
        if (Flags.HasFlag(Flag.Flag1)) {
            builder.Append($"Flag1: {Flag1Unknown}");
        }
        if (Flags.HasFlag(Flag.Flag2)) {
            builder.Append($"Flag2: {Flag2Unknown}");
        }
        if (Flags.HasFlag(Flag.Flag3)) {
            builder.Append($"Flag3: {Flag3Unknown}");
        }
        if (Flags.HasFlag(Flag.Animation)) {
            builder.Append($"Animation: {AnimationName}");
        }
        if (Flags.HasFlag(Flag.Flag5)) {
            builder.Append($"Flag5: {Flag5Unknown1}, {Flag5Unknown2}");
        }
        if (Flags.HasFlag(Flag.Flag6)) {
            builder.Append($"Flag6: {Flag6Unknown1}, {Flag6Unknown2}");
        }

        return builder.ToString();
    }
}
