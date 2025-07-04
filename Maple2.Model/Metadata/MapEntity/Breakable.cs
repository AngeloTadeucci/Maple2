using System.Numerics;

namespace Maple2.Model.Metadata;

public record Breakable(
    bool Visible,
    int Id,
    int HideTime,
    int ResetTime,
    Vector3 Position,
    Vector3 Rotation)
: MapBlock;

public record BreakableActor(
    bool Visible,
    int Id,
    int HideTime,
    int ResetTime,
    int GlobalDropBoxId,
    Vector3 Position,
    Vector3 Rotation)
: MapBlock;
