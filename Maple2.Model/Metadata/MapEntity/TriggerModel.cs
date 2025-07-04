using System.Numerics;

namespace Maple2.Model.Metadata;

public record TriggerModel(
    int Id,
    string Name,
    Vector3 Position,
    Vector3 Rotation)
: MapBlock;
