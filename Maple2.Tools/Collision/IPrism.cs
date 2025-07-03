using System.Numerics;

namespace Maple2.Tools.Collision;

public interface IPrism {
    public IPolygon Polygon { get; }
    public Range Height { get; }

    public bool Contains(in Vector3 point, float epsilon = 1e-5f);

    public bool Intersects(IPrism prism);
}
