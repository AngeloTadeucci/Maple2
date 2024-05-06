using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Maple2.Model.Common;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
public readonly record struct Color(byte Blue, byte Green, byte Red, byte Alpha) {
    public override string ToString() => $"ARGB({Alpha:X2}, {Red:X2}, {Green:X2}, {Blue:X2})";
}


[StructLayout(LayoutKind.Sequential, Size = 8)]
[method: JsonConstructor]
public readonly struct SkinColor(Color primary, Color secondary) {
    public Color Primary { get; } = primary;
    public Color Secondary { get; } = secondary;

    public SkinColor(Color color) : this(color, color) {
    }

    public override string ToString() => $"Primary:{Primary}|Secondary:{Secondary}";
}


[StructLayout(LayoutKind.Sequential, Size = 20)]
[method: JsonConstructor]
public readonly struct EquipColor(Color primary, Color secondary, Color tertiary, int paletteId, int index = -1) {
    public Color Primary { get; } = primary;
    public Color Secondary { get; } = secondary;
    public Color Tertiary { get; } = tertiary;
    public int Index { get; } = index;
    public int PaletteId { get; } = paletteId;

    public EquipColor(Color color) : this(color, color, color, 0, -1) {
    }

    public override string ToString() =>
        $"Primary:{Primary}|Secondary:{Secondary}|Tertiary:{Tertiary}|Index:{Index}";
}
