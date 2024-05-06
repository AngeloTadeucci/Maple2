using System;
using System.Numerics;

namespace Maple2.Tools.Collision;

// Point is not a prism but allows us to intersect points with polygons polymorphically.
public readonly struct PointPrism(Vector3 origin) : IPrism {
    private readonly Point point = new(origin.X, origin.Y);

    public IPolygon Polygon => point;
    public Range Height { get; } = new(origin.Z, origin.Z);

    public bool Contains(in Vector3 other) {
        return origin == other;
    }

    public bool Intersects(IPrism prism) {
        return prism.Contains(origin);
    }

    private readonly struct Point(float x, float y) : IPolygon {

        public bool Contains(in Vector2 point) {
            const float tolerance = 0.0000001f;
            return Math.Abs(x - point.X) < tolerance && Math.Abs(y - point.Y) < tolerance;
        }

        public bool Intersects(IPolygon polygon) {
            return polygon.Contains(x, y);
        }

        public Vector2[] GetAxes(Polygon? other) {
            return Array.Empty<Vector2>();
        }

        public Range AxisProjection(Vector2 axis) {
            float projection = axis.X * x + axis.Y * y;
            return new Range(projection, projection);
        }

        public override string ToString() {
            return $"<X:{x}, Y:{y}>";
        }
    }

    public override string ToString() {
        return origin.ToString();
    }
}
