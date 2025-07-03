using System.Numerics;
using Maple2.Model.Enum;

namespace Maple2.Model.Metadata;

public record Portal(
    int Id,
    int TargetMapId,
    int TargetPortalId,
    PortalType Type,
    PortalActionType ActionType,
    Vector3 Position,
    Vector3 Rotation,
    Vector3 Dimension,
    float FrontOffset,
    int RandomRadius,
    bool Visible,
    bool MinimapVisible,
    bool Enable
) : MapBlock;
