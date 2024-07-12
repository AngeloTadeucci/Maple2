using Maple2.Tools.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Maple2.Tools.VectorMath;

public struct BoundingBox3 {
    public Vector3 Min;
    public Vector3 Max;

    public Vector3 Size { get => Max - Min; }
    public Vector3 Center { get => 0.5f * (Max + Min); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BoundingBox3(Vector3 min = new Vector3()) {
        Min = min;
        Max = min;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BoundingBox3(Vector3 min, Vector3 max) {
        Min = min;
        Max = max;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BoundingBox3 Expand(Vector3 point) {
        return new BoundingBox3(
            Vector3.Min(Min, point),
            Vector3.Max(Max, point)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BoundingBox3 Expand(BoundingBox3 box) {
        return this.Expand(box.Min).Expand(box.Max);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BoundingBox3 Fatten(float amount) {
        return new BoundingBox3(
            Min - new Vector3(amount, amount, amount),
            Max + new Vector3(amount, amount, amount)
        );
    }

    public static BoundingBox3 Transform(BoundingBox3 box, Matrix4x4 matrix) {
        Vector3 translation = matrix.Translation;

        BoundingBox3 result = new BoundingBox3(translation);

        for (int i = 0; i < 3; ++i) {
            for (int j = 0; j < 3; ++j) {
                float a = matrix[j, i] * box.Min[j];
                float b = matrix[j, i] * box.Max[j];

                if (a > b) {
                    (a, b) = (b, a);
                }

                result.Min[i] += a;
                result.Max[i] += b;
            }
        }

        Vector3 size = box.Max - box.Min;
        List<Vector3> vertices = new List<Vector3>() {
            box.Min,
            box.Max,
            box.Min + new Vector3(size.X, 0, 0),
            box.Min + new Vector3(size.X, size.Y, 0),
            box.Min + new Vector3(size.X, 0, size.Z),
            box.Min + new Vector3(0, size.Y, 0),
            box.Min + new Vector3(0, size.Y, size.Z),
            box.Min + new Vector3(0, 0, size.Z)
        };

        for (int i = 0; i < 8; ++i) {
            vertices[i] = Vector3.Transform(vertices[i], matrix);
        }

        BoundingBox3 result2 = Compute(vertices);

        if (!result.IsNearlyEqual(result2, 1e-2f)) {
            throw new System.Exception("possibly wrong axis");
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Vector3 point, float epsilon = 0) {
        bool withinMinBounds = point.X >= Min.X - epsilon && point.Y >= Min.Y - epsilon && point.Z >= Min.Z - epsilon;
        bool withinMaxBounds = point.X <= Max.X + epsilon && point.Y <= Max.Y + epsilon && point.Z <= Max.Z + epsilon;

        return withinMinBounds && withinMaxBounds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(BoundingBox3 box, float epsilon = 0) {
        bool withinMinBounds = box.Min.X >= Min.X - epsilon && box.Min.Y >= Min.Y - epsilon && box.Min.Z >= Min.Z - epsilon;
        bool withinMaxBounds = box.Max.X <= Max.X + epsilon && box.Max.Y <= Max.Y + epsilon && box.Max.Z <= Max.Z + epsilon;

        return withinMinBounds && withinMaxBounds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Intersects(BoundingBox3 box, float epsilon = 0) {
        BoundingBox3 compoundBox = new BoundingBox3(Min, Max + box.Size);

        return compoundBox.Contains(box.Max, epsilon);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNearlyEqual(BoundingBox3 box, float epsilon = 1e-5f) {
        return Min.IsNearlyEqual(box.Min, epsilon) && Max.IsNearlyEqual(box.Max, epsilon);
    }

    public static BoundingBox3 Compute(List<Vector3> points) {
        if (points.Count == 0) {
            return new BoundingBox3();
        }

        BoundingBox3 box = new BoundingBox3(points[0]);

        foreach (Vector3 point in points) {
            box = box.Expand(point);
        }

        return box;
    }
}

