using System.Numerics;

namespace Maple2.Model.Metadata;

public record Ms2RegionSpawn(
    int Id,
    bool UseRotAsSpawnDir,
    Vector3 Position,
    Vector3 Rotation
) : MapBlock;
