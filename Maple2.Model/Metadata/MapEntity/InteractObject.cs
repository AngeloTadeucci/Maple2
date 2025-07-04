using System.Numerics;

namespace Maple2.Model.Metadata;

public abstract record InteractObject(
    int InteractId,
    Vector3 Position,
    Vector3 Rotation) : MapBlock;

public record Ms2InteractActor(
    int InteractId,
    Vector3 Position,
    Vector3 Rotation)
: InteractObject(InteractId, Position, Rotation);

public record Ms2InteractDisplay(
    int InteractId,
    Vector3 Position,
    Vector3 Rotation)
: InteractObject(InteractId, Position, Rotation);

public record Ms2InteractMesh(
    int InteractId,
    Vector3 Position,
    Vector3 Rotation)
: InteractObject(InteractId, Position, Rotation);

// These are unused in XML
// public record Ms2InteractWebActor(
//     Vector3 Position,
//     Vector3 Rotation)
// : InteractObject(InteractId, Position, Rotation);
//
// public record Ms2InteractWebMesh(
//     int InteractId,
//     Vector3 Position,
//     Vector3 Rotation)
// : InteractObject(InteractId, Position, Rotation);

public record Ms2SimpleUiObject(
    int InteractId,
    Vector3 Position,
    Vector3 Rotation)
: InteractObject(InteractId, Position, Rotation);

public record Ms2Telescope(
    int InteractId,
    Vector3 Position,
    Vector3 Rotation)
: InteractObject(InteractId, Position, Rotation);
