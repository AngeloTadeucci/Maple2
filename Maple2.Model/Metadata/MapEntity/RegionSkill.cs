using System.Numerics;

namespace Maple2.Model.Metadata;

public record Ms2RegionSkill(
    int SkillId,
    short Level,
    int Interval,
    Vector3 Position,
    Vector3 Rotation
) : MapBlock;
