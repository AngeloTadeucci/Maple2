using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Maple2.Tools.Extensions;

public static class VectorExtensions {
    public const int BLOCK_SIZE = 150;
    private const float DEG_TO_RAD = MathF.PI / 180f;
    private const float RAD_TO_DEG = 180f / MathF.PI;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Align(this in Vector3 position, float interval = BLOCK_SIZE) {
        return new Vector3(
            MathF.Round(position.X / interval) * interval,
            MathF.Round(position.Y / interval) * interval,
            MathF.Floor(MathF.Round(position.Z) / interval) * interval
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 AlignHeight(this in Vector3 position, float interval = BLOCK_SIZE) {
        return position with { Z = MathF.Floor(MathF.Round(position.Z) / interval) * interval };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 AlignRotation(this in Vector3 position, float interval = 90f) {
        return new Vector3(
            MathF.Round(position.X / interval) * interval,
            MathF.Round(position.Y / interval) * interval,
            MathF.Round(position.Z / interval) * interval
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Offset(this in Vector3 position, float distance, float rotation) {
        return position + new Vector3(0, distance, 0).Rotate(new Vector3(0, 0, rotation));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Offset(this in Vector3 position, float distance, in Vector3 rotation) {
        return position + new Vector3(0, distance, 0).Rotate(rotation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Offset(this in Vector3 position, in Vector3 offset, float rotation) {
        return position + offset.Rotate(new Vector3(0, 0, rotation));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Offset(this in Vector3 position, in Vector3 offset, in Vector3 rotation) {
        return position + offset.Rotate(rotation);
    }

    // Rotates a vector by specified euler angles.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Rotate(this in Vector3 magnitude, in Vector3 eulerAngle) {
        // Extra MathF.PI on X/Z are because Y-axis is mirrored.
        var rotation = Quaternion.CreateFromYawPitchRoll(
            MathF.PI - eulerAngle.X * DEG_TO_RAD,
            eulerAngle.Y * DEG_TO_RAD,
            MathF.PI - eulerAngle.Z * DEG_TO_RAD
        );

        return Vector3.Transform(magnitude, rotation);
    }

    // Computes the rotation angle from src(x, y) to dst(x, y).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Angle2D(this in Vector3 src, in Vector3 dst) {
        return (float) Math.Atan2(dst.Y - src.Y, dst.X - src.X) * RAD_TO_DEG + 90f;
    }

    // Clamps the length of a vector between min and max.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ClampLength(this in Vector3 length, float min, float max) {
        float magnitude = length.Length();
        if (magnitude < min) {
            return min * Vector3.Normalize(length);
        }
        if (magnitude > max) {
            return max * Vector3.Normalize(length);
        }

        return length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Normal(this Vector2 vector) => new(vector.Y, -vector.X);

    // Returns all blocks within range in a 2D grid around position
    // position should be aligned here
    public static IEnumerable<Vector3> BlocksInRange(this Vector3 position, int range) {
        int xBase = (int) position.X / BLOCK_SIZE;
        int yBase = (int) position.Y / BLOCK_SIZE;
        int xMin = xBase - range;
        int xMax = xBase + range;
        for (int x = xMin; x <= xMax; x++) {
            int xMag = Math.Abs(x - xBase); // X-Magnitude
            int yMin = yBase - range + xMag;
            int yMax = yBase + range - xMag;
            for (int y = yMin; y <= yMax; y++) {
                yield return new Vector3(x * BLOCK_SIZE, y * BLOCK_SIZE, position.Z);
            }
        }
    }
}
