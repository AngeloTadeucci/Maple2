using System.Numerics;

namespace Maple2.Model.Metadata;

public record TaxiStation(
    Vector3 Position,
    Vector3 Rotation
) : MapBlock;
