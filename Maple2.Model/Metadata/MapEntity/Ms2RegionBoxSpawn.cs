using System.Numerics;

namespace Maple2.Model.Metadata;

public record Ms2RegionBoxSpawn(
    int Id,
    int SpawnTypeId,
    float Scale,
    Vector3 Position,
    Vector3 Rotation
) : MapBlock;
