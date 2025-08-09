using System;
using System.Numerics;

namespace Maple2.Tools.Collision;

/// <summary>
/// Provides analytic (allocation-free) ray intersection tests for IPrism shapes.
/// Supports PointPrism, circular Prism (treated as cylinder) and generic convex Polygon prism.
/// </summary>
public static class PrismRaycastExtensions {
    /// <summary>
    /// Perform analytic ray intersection with a prism. Direction need not be normalized.
    /// Returns true if the ray intersects, providing distance along the ray in world units to first hit (>=0).
    /// </summary>
    public static bool Raycast(this IPrism prism, in Vector3 origin, in Vector3 direction, out float distance) {
        distance = 0f;
        float dirLen = direction.Length();
        if (dirLen < 1e-6f) return false;
        Vector3 d = direction / dirLen;
        Vector3 o = origin;

        if (prism.Contains(o)) return true; // already inside

        // PointPrism: solve param t for a single point; fall back to Contains verification.
        if (prism is PointPrism) {
            // We cannot access internal point directly; test with three axes selection.
            int axis = 0;
            float absX = MathF.Abs(d.X);
            float absY = MathF.Abs(d.Y);
            float absZ = MathF.Abs(d.Z);
            if (absY > absX && absY >= absZ) axis = 1;
            else if (absZ > absX && absZ > absY) axis = 2;
            // Choose a target coordinate via polygon/height; pattern-match for Circle/Polygon
            float targetCoord;
            float originCoord;
            float dirCoord;
            switch (axis) {
                case 1:
                    targetCoord = prism.Polygon switch { Circle c => c.Origin.Y, Polygon ppoly => ppoly.Points[0].Y, _ => o.Y };
                    originCoord = o.Y;
                    dirCoord = d.Y;
                    break;
                case 2:
                    targetCoord = prism.Height.Min;
                    originCoord = o.Z;
                    dirCoord = d.Z;
                    break; // Height.Min == Height.Max
                default:
                    targetCoord = prism.Polygon switch { Circle c => c.Origin.X, Polygon ppoly => ppoly.Points[0].X, _ => o.X };
                    originCoord = o.X;
                    dirCoord = d.X;
                    break;
            }
            if (MathF.Abs(dirCoord) < 1e-6f) return false;
            float t = (targetCoord - originCoord) / dirCoord;
            if (t < 0) return false;
            Vector3 p = o + d * t;
            if (prism.Contains(p)) {
                distance = t * dirLen;
                return true;
            }
            return false;
        }

        if (prism is Prism prismStruct && prismStruct.Polygon is Circle circle) {
            // Cylinder intersection (circle extruded along Z)
            float zMin = prism.Height.Min;
            float zMax = prism.Height.Max;
            float ox = o.X - circle.Origin.X;
            float oy = o.Y - circle.Origin.Y;
            float dx = d.X;
            float dy = d.Y;
            float a = dx * dx + dy * dy;
            float b = 2f * (dx * ox + dy * oy);
            float c = ox * ox + oy * oy - circle.Radius * circle.Radius;
            float tCylEnter, tCylExit;
            if (a < 1e-8f) { // Parallel to cylinder axis
                if (c > 0f) return false; // Outside
                tCylEnter = 0f;
                tCylExit = float.PositiveInfinity;
            } else {
                float disc = b * b - 4f * a * c;
                if (disc < 0f) return false;
                float sqrt = MathF.Sqrt(disc);
                float inv2a = 0.5f / a;
                float t0 = (-b - sqrt) * inv2a;
                float t1 = (-b + sqrt) * inv2a;
                if (t0 > t1) (t0, t1) = (t1, t0);
                tCylEnter = t0;
                tCylExit = t1;
            }
            // Z slab
            float tZEnter, tZExit;
            if (MathF.Abs(d.Z) < 1e-8f) {
                if (o.Z < zMin || o.Z > zMax) return false;
                tZEnter = 0f;
                tZExit = float.PositiveInfinity;
            } else {
                float tz0 = (zMin - o.Z) / d.Z;
                float tz1 = (zMax - o.Z) / d.Z;
                if (tz0 > tz1) (tz0, tz1) = (tz1, tz0);
                tZEnter = tz0;
                tZExit = tz1;
            }
            float tEnter = MathF.Max(MathF.Max(tCylEnter, tZEnter), 0f);
            float tExit = MathF.Min(tCylExit, tZExit);
            if (tEnter <= tExit && tExit >= 0f) {
                distance = tEnter * dirLen;
                return true;
            }
            return false;
        }

        if (prism is Prism genericPrism && genericPrism.Polygon is Polygon poly) {
            float zMin = prism.Height.Min;
            float zMax = prism.Height.Max;
            // Z interval
            float tZEnter, tZExit;
            if (MathF.Abs(d.Z) < 1e-8f) {
                if (o.Z < zMin || o.Z > zMax) return false;
                tZEnter = 0f;
                tZExit = float.PositiveInfinity;
            } else {
                float tz0 = (zMin - o.Z) / d.Z;
                float tz1 = (zMax - o.Z) / d.Z;
                if (tz0 > tz1) (tz0, tz1) = (tz1, tz0);
                tZEnter = tz0;
                tZExit = tz1;
            }
            Vector2[] points = poly.Points;
            int count = points.Length;
            if (count < 3) return false;
            // Orientation via twice-area
            float area2 = 0f;
            for (int i = 0; i < count; i++) {
                Vector2 p0 = points[i];
                Vector2 p1 = points[(i + 1) % count];
                area2 += p0.X * p1.Y - p1.X * p0.Y;
            }
            bool ccw = area2 >= 0f;
            float tEnterXY = 0f;
            float tExitXY = float.PositiveInfinity;
            Vector2 dxy = new(d.X, d.Y);
            if (dxy.LengthSquared() < 1e-10f) { // vertical ray
                if (!poly.Contains(new Vector2(o.X, o.Y))) return false;
            } else {
                for (int i = 0; i < count; i++) {
                    Vector2 v0 = points[i];
                    Vector2 v1 = points[(i + 1) % count];
                    Vector2 e = v1 - v0;
                    Vector2 o0 = new(o.X - v0.X, o.Y - v0.Y);
                    float A = e.X * d.Y - e.Y * d.X; // cross(e,d)
                    float B = e.X * o0.Y - e.Y * o0.X; // cross(e,o-v0)
                    if (!ccw) {
                        A = -A;
                        B = -B;
                    }
                    if (MathF.Abs(A) < 1e-9f) {
                        if (B < 0f) return false;
                        continue;
                    }
                    float tEdge = -B / A;
                    if (A > 0f) {
                        if (tEdge > tEnterXY) tEnterXY = tEdge;
                    } else {
                        if (tEdge < tExitXY) tExitXY = tEdge;
                    }
                    if (tEnterXY - tExitXY > 1e-6f) return false;
                }
            }
            float tEnter = MathF.Max(MathF.Max(tEnterXY, tZEnter), 0f);
            float tExit = MathF.Min(tExitXY, tZExit);
            if (tEnter <= tExit && tExit >= 0f) {
                distance = tEnter * dirLen;
                return true;
            }
            return false;
        }

        // Fallback for unknown prism implementation: sparse exponential sampling.
        const float fallbackMax = 10000f;
        float tS = 0f;
        while (tS < fallbackMax) {
            Vector3 p = o + d * tS;
            if (prism.Contains(p)) {
                distance = tS * dirLen;
                return true;
            }
            tS = tS == 0f ? 10f : tS * 1.5f;
        }
        return false;
    }
}
