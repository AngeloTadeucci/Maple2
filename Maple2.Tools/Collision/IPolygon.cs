using System;
using System.Numerics;

namespace Maple2.Tools.Collision;

public interface IPolygon {
    public static readonly IPolygon Null = new NullPolygon();

    public bool Contains(float x, float y) => Contains(new Vector2(x, y), 0.001f);

    public bool Contains(in Vector2 point, float epsilon = 1e-5f);

    public Vector2[] GetAxes(Polygon? other);

    public Range AxisProjection(Vector2 axis);

    public bool Intersects(IPolygon other);

    private class NullPolygon : Polygon {
        public override Vector2[] Points => [];
        public override bool Contains(in Vector2 point, float epsilon = 1e-5f) => false;
    }
}
