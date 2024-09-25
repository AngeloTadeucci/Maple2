using Maple2.Tools.VectorMath;
using System.Numerics;

namespace Maple2.Model.Metadata.FieldEntities;

public enum FieldEntityType : byte {
    Unknown,
    Vibrate,
    SpawnTile, // for mob spawns
    BoxCollider, // for cube tiles (IsWhiteBox = false) & arbitrarily sized white boxes
    MeshCollider,
    Fluid,
    Cell // for cells culled from the grid & demoted to AABB tree
}

public record FieldEntityId(
    ulong High,
    ulong Low) {
    public bool IsNull { get => High == 0 && Low == 0; }
}

public record FieldEntity(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds);

public record FieldVibrateEntity(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds) : FieldEntity(Id, Position, Rotation, Scale, Bounds);

public record FieldSpawnTile(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds) : FieldEntity(Id, Position, Rotation, Scale, Bounds);

public record FieldBoxColliderEntity(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds,
    Vector3 Size,
    bool IsWhiteBox) : FieldEntity(Id, Position, Rotation, Scale, Bounds);

public record FieldMeshColliderEntity(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds,
    uint MeshLlid) : FieldEntity(Id, Position, Rotation, Scale, Bounds);

public record FieldFluidEntity(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds,
    uint MeshLlid) : FieldMeshColliderEntity(Id, Position, Rotation, Scale, Bounds, MeshLlid);

public record FieldCellEntities(
    FieldEntityId Id,
    Vector3 Position,
    Vector3 Rotation,
    float Scale,
    BoundingBox3 Bounds,
    List<FieldEntity> Entities) : FieldEntity(Id, Position, Rotation, Scale, Bounds);
