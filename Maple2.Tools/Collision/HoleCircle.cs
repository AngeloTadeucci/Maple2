using System.Linq;
using System.Numerics;

namespace Maple2.Tools.Collision;

public sealed class HoleCircle : Circle {
    private readonly Circle hole;

    public HoleCircle(in Vector2 origin, float innerRadius, float outerRadius) : base(origin, outerRadius) {
        hole = new Circle(origin, innerRadius);
    }

    public override bool Contains(in Vector2 point) {
        return base.Contains(point) && !hole.Contains(point);
    }

    // Inherited GetAxes() and AxisProjection() includes hole, but is unused.

    public override bool Intersects(IPolygon other) {
        return other switch {
            Circle circle => IntersectsCircle(circle),
            Polygon polygon => IntersectsPolygon(polygon),
            _ => false,
        };
    }

    internal override bool IntersectsCircle(Circle other) {
        // First check if other is fully contained within the hole.
        float distanceSquared = Vector2.DistanceSquared(hole.Origin, other.Origin);
        if (distanceSquared + other.Radius * other.Radius < hole.Radius * hole.Radius) {
            return false;
        }

        return base.IntersectsCircle(other);
    }

    private bool IntersectsPolygon(Polygon other) {
        // First check if other is fully contained within the hole.
        if (other.Points.All(point => hole.Contains(point))) {
            return false;
        }

        return other.Intersects(this);
    }

    public override string ToString() {
        return $"Outer:{base.ToString()}, Inner:{hole}";
    }
}
