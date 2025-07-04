using System.Numerics;
using System.Runtime.InteropServices;

namespace Maple2.Model.Common;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 3)]
public readonly record struct Byte3(byte X, byte Y, byte Z) {
    public static implicit operator Byte3(Vector3 vector) {
        return new Byte3((byte) MathF.Round(vector.X), (byte) MathF.Round(vector.Y), (byte) MathF.Round(vector.Z));
    }
}
