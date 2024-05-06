using System.Numerics;
using Maple2.Model.Common;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class ItemAppearance(EquipColor color) : IByteSerializable, IByteDeserializable {
    public static readonly ItemAppearance Default = new ItemAppearance(default);

    public EquipColor Color = color;

    public virtual ItemAppearance Clone() {
        return (ItemAppearance) MemberwiseClone();
    }

    public virtual void WriteTo(IByteWriter writer) {
        writer.Write<EquipColor>(Color);
    }

    public virtual void ReadFrom(IByteReader reader) {
        Color = reader.Read<EquipColor>();
    }
}

public sealed class HairAppearance(
    EquipColor color,
    float backLength = default,
    Vector3 backPosition1 = default,
    Vector3 backPosition2 = default,
    float frontLength = default,
    Vector3 frontPosition1 = default,
    Vector3 frontPosition2 = default)
    : ItemAppearance(color) {
    public float BackLength { get; private set; } = backLength;
    public Vector3 BackPosition1 { get; private set; } = backPosition1;
    public Vector3 BackPosition2 { get; private set; } = backPosition2;
    public float FrontLength { get; private set; } = frontLength;
    public Vector3 FrontPosition1 { get; private set; } = frontPosition1;
    public Vector3 FrontPosition2 { get; private set; } = frontPosition2;

    public override HairAppearance Clone() {
        return (HairAppearance) MemberwiseClone();
    }

    public override void WriteTo(IByteWriter writer) {
        writer.Write<EquipColor>(Color);
        writer.WriteFloat(BackLength);
        writer.Write<Vector3>(BackPosition1);
        writer.Write<Vector3>(BackPosition2);
        writer.WriteFloat(FrontLength);
        writer.Write<Vector3>(FrontPosition1);
        writer.Write<Vector3>(FrontPosition2);
    }

    public override void ReadFrom(IByteReader reader) {
        Color = reader.Read<EquipColor>();
        BackLength = reader.ReadFloat();
        BackPosition1 = reader.Read<Vector3>();
        BackPosition2 = reader.Read<Vector3>();
        FrontLength = reader.ReadFloat();
        FrontPosition1 = reader.Read<Vector3>();
        FrontPosition2 = reader.Read<Vector3>();
    }
}

public sealed class DecalAppearance(
    EquipColor color,
    float position1 = default,
    float position2 = default,
    float position3 = default,
    float position4 = default)
    : ItemAppearance(color) {
    public float Position1 { get; private set; } = position1;
    public float Position2 { get; private set; } = position2;
    public float Position3 { get; private set; } = position3;
    public float Position4 { get; private set; } = position4;

    public override DecalAppearance Clone() {
        return (DecalAppearance) MemberwiseClone();
    }

    public override void WriteTo(IByteWriter writer) {
        writer.Write<EquipColor>(Color);
        writer.WriteFloat(Position1);
        writer.WriteFloat(Position2);
        writer.WriteFloat(Position3);
        writer.WriteFloat(Position4);
    }

    public override void ReadFrom(IByteReader reader) {
        Color = reader.Read<EquipColor>();
        Position1 = reader.ReadFloat();
        Position2 = reader.ReadFloat();
        Position3 = reader.ReadFloat();
        Position4 = reader.ReadFloat();
    }
}

public sealed class CapAppearance(
    EquipColor color,
    Vector3 position1 = default,
    Vector3 position2 = default,
    Vector3 position3 = default,
    Vector3 position4 = default,
    float unknown = default)
    : ItemAppearance(color) {
    public Vector3 Position1 { get; private set; } = position1;
    public Vector3 Position2 { get; private set; } = position2;
    public Vector3 Position3 { get; private set; } = position3;
    public Vector3 Position4 { get; private set; } = position4;
    public float Unknown { get; private set; } = unknown;

    public override CapAppearance Clone() {
        return (CapAppearance) MemberwiseClone();
    }

    public override void WriteTo(IByteWriter writer) {
        writer.Write<EquipColor>(Color);
        writer.Write<Vector3>(Position1);
        writer.Write<Vector3>(Position2);
        writer.Write<Vector3>(Position3);
        writer.Write<Vector3>(Position4);
        writer.WriteFloat(Unknown);
    }

    public override void ReadFrom(IByteReader reader) {
        Color = reader.Read<EquipColor>();
        Position1 = reader.Read<Vector3>();
        Position2 = reader.Read<Vector3>();
        Position3 = reader.Read<Vector3>();
        Position4 = reader.Read<Vector3>();
        Unknown = reader.ReadFloat();
    }
}
