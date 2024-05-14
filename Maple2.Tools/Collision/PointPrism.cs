using System;
using System.Numerics;

namespace Maple2.Tools.Collision;

// Point is not a prism but allows us to intersect points with polygons polymorphically.
public readonly struct PointPrism : IPrism {
    private readonly Vector3 origin;
    private readonly Point point;

    public IPolygon Polygon => point;
    public Range Height { get; }

    public PointPrism(Vector3 origin) {
        this.origin = origin;
        point = new Point(origin.X, origin.Y);
        Height = new Range(origin.Z, origin.Z);
    }

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
            return [];
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
