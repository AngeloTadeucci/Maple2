
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;
using Maple2.Model.Enum;
using Maple2.Tools.VectorMath;

namespace Maple2.Model.Metadata;

public record NifMetadata(
    uint Llid,
    BoundingBox3 PhysXBounds,
    NifMetadata.NifBlockMetadata[] Blocks
) {

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "!")]
    [JsonDerivedType(typeof(NiPhysXActorDescMetadata), nameof(NiPhysXActorDescMetadata))]
    [JsonDerivedType(typeof(NiPhysXMeshDescMetadata), nameof(NiPhysXMeshDescMetadata))]
    [JsonDerivedType(typeof(NiPhysXPropMetadata), nameof(NiPhysXPropMetadata))]
    [JsonDerivedType(typeof(NiPhysXPropDescMetadata), nameof(NiPhysXPropDescMetadata))]
    [JsonDerivedType(typeof(NiPhysXShapeDescMetadata), nameof(NiPhysXShapeDescMetadata))]
    public record NifBlockMetadata(
        int Index,
        string Name
    );

    public record NiPhysXActorDescMetadata(
        int Index,
        string Name,
        string ActorName,
        List<Matrix4x4> Poses,
        List<int> ShapeDescriptions // NiPhysXShapeDesc
    ) : NifBlockMetadata(Index, Name);

    public record NiPhysXMeshDescMetadata(
        int Index,
        string Name,
        string MeshName,
        int MeshDataIndex // NXSMeshMetadata
    ) : NifBlockMetadata(Index, Name);

    public record NiPhysXPropMetadata(
        int Index,
        string Name,
        float PhysXToWorldScale,
        int Snapshot // NiPhysXPropDesc
    ) : NifBlockMetadata(Index, Name);

    public record NiPhysXPropDescMetadata(
        int Index,
        string Name,
        List<int> Actors // NiPhysXActorDesc
    ) : NifBlockMetadata(Index, Name);

    public record NiPhysXShapeDescMetadata(
     int Index,
     string Name,
     Matrix4x4 LocalPose,
     NxShapeType ShapeType,
     Vector3 BoxHalfExtents,
     int Mesh // NiPhysXMeshDesc
 ) : NifBlockMetadata(Index, Name);
}

public record NxsMeshMetadata(
    int Index,
    byte[] Data,
    BoundingBox3 Bounds
);
