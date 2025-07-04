using System.Numerics;

namespace Maple2.Tools.Collision;

public readonly struct Prism : IPrism {
    public IPolygon Polygon { get; }
    public Range Height { get; }

    public Prism(IPolygon shape, float baseHeight, float height) {
        Polygon = shape;
        Height = new Range(baseHeight, baseHeight + height);
    }

    public bool Contains(in Vector3 point, float epsilon = 1e-5f) {
        return Height.Min <= point.Z && point.Z <= Height.Max && Polygon.Contains(point.X, point.Y, epsilon);
    }

    public bool Intersects(IPrism prism) {
        if (Height.Overlaps(prism.Height)) {
            return Polygon.Intersects(prism.Polygon);
        }

        return false;
    }

    public override string ToString() {
        return $"{Polygon}, [{Height.Min}, {Height.Max}]";
    }
}
