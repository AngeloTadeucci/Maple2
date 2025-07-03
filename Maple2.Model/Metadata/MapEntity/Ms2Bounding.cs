using System.Numerics;

namespace Maple2.Model.Metadata;

public record Ms2Bounding(
    Vector3 Position1,
    Vector3 Position2)
: MapBlock;
