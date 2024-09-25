using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Maple2.Model.Common;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
public readonly record struct Vector3B(sbyte X, sbyte Y, sbyte Z) {
    private const float BLOCK_SIZE = 150f;

    public Vector3B(int X, int Y, int Z) : this((sbyte) X, (sbyte) Y, (sbyte) Z) { }

    public static implicit operator Vector3B(Vector3 vector) {
        return new Vector3B(
            (sbyte) MathF.Round(vector.X / BLOCK_SIZE),
            (sbyte) MathF.Round(vector.Y / BLOCK_SIZE),
            (sbyte) MathF.Round(vector.Z / BLOCK_SIZE)
        );
    }

    public static implicit operator Vector3(Vector3B vector) {
        return new Vector3(
            vector.X * BLOCK_SIZE,
            vector.Y * BLOCK_SIZE,
            vector.Z * BLOCK_SIZE
        );
    }

    public static Vector3B operator +(in Vector3B a, in Vector3B b) =>
        new((sbyte) (a.X + b.X), (sbyte) (a.Y + b.Y), (sbyte) (a.Z + b.Z));

    // We override GetHashCode because only 3/4 bytes are relevant.
    public override int GetHashCode() {
        return X << 16 | Y << 8 | (byte) Z;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 2, Size = 6)]
public readonly record struct Vector3S(short X, short Y, short Z) {
    // This offset is used to correct rounding errors due to floating point arithmetic.
    private const float OFFSET = 0.001f;

    public Vector3 Vector3 { get => new Vector3(X, Y, Z); }

    public static implicit operator Vector3S(Vector3 vector) {
        return new Vector3S(
            (short) MathF.Round(vector.X),
            (short) MathF.Round(vector.Y),
            (short) MathF.Round(vector.Z)
        );
    }

    public static implicit operator Vector3(Vector3S vector) {
        return new Vector3(
            vector.X + (vector.X >= 0 ? OFFSET : -OFFSET),
            vector.Y + (vector.Y >= 0 ? OFFSET : -OFFSET),
            vector.Z + OFFSET
        );
    }

    public static Vector3S operator +(in Vector3S a, in Vector3S b) =>
        new Vector3S((short) (a.X + b.X), (short) (a.Y + b.Y), (short) (a.Z + b.Z));
    public static Vector3S operator -(in Vector3S a, in Vector3S b) =>
        new Vector3S((short) (a.X - b.X), (short) (a.Y - b.Y), (short) (a.Z - b.Z));
    public static Vector3S operator *(in Vector3S a, in Vector3S b) =>
        new Vector3S((short) (a.X * b.X), (short) (a.Y * b.Y), (short) (a.Z * b.Z));
    public static Vector3S operator /(in Vector3S a, in Vector3S b) =>
        new Vector3S((short) (a.X / b.X), (short) (a.Y / b.Y), (short) (a.Z / b.Z));

    public override string ToString() => $"<{X}, {Y}, {Z}>";
}
