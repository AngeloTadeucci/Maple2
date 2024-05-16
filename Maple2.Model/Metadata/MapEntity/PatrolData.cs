using System.Collections.Generic;
using System.Numerics;

namespace Maple2.Model.Metadata;

public record MS2PatrolData(
    string Uuid,
    string Name,
    bool IsAirWayPoint,
    int PatrolSpeed,
    bool IsLoop,
    List<MS2WayPoint> WayPoints
) : MapBlock;

public record MS2WayPoint(
    string Id,
    bool IsVisible,
    Vector3 Position,
    Vector3 Rotation,
    string ApproachAnimation,
    string ArriveAnimation,
    int ArriveAnimationTime
);
