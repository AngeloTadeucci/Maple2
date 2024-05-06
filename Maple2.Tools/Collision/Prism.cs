using System.Numerics;

namespace Maple2.Tools.Collision;

public readonly struct Prism(IPolygon shape, float baseHeight, float height) : IPrism {
    public IPolygon Polygon { get; } = shape;
    public Range Height { get; } = new(baseHeight, baseHeight + height);

    public bool Contains(in Vector3 point) {
        return Height.Min <= point.Z && point.Z <= Height.Max && Polygon.Contains(point.X, point.Y);
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
