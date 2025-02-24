using Maple2.File.Ingest.Helpers;
using Maple2.File.IO.Nif;
using Maple2.Model.Enum;
using Maple2.Model.Metadata;
using Maple2.Tools.VectorMath;
using System.Numerics;

namespace Maple2.File.Ingest.Mapper;

public class NifMapper : TypeMapper<NifMetadata> {
    protected override IEnumerable<NifMetadata> Map() {
        foreach ((uint llid, NifDocument document) in NifParserHelper.nifDocuments) {
            BoundingBox3 bounds = NifParserHelper.nifBounds[llid];

            yield return new NifMetadata(
                Llid: llid,
                PhysXBounds: bounds,
                Blocks: MapBlockMetadata(document).ToArray()
            );
        }
    }

    private static IEnumerable<NifMetadata.NifBlockMetadata> MapBlockMetadata(NifDocument document) {
        foreach (NifBlock item in document.Blocks) {
            int nxsMeshIndex = -1;
            if (item is NiPhysXMeshDesc meshDesc) {
                string meshDataString = Convert.ToBase64String(meshDesc.MeshData);
                if (NifParserHelper.nxsMeshIndexMap.TryGetValue(meshDataString, out int value)) {
                    nxsMeshIndex = value;
                }
            }

            NifMetadata.NifBlockMetadata nifBlockMetadata = item switch {
                NiPhysXActorDesc actorDesc => new NifMetadata.NiPhysXActorDescMetadata(
                    item.BlockIndex,
                    actorDesc.Name,
                    ActorName: actorDesc.ActorName,
                    Poses: actorDesc.Poses,
                    ShapeDescriptions: actorDesc.ShapeDescriptions.Select(shapeDesc => shapeDesc.BlockIndex).ToList()),
                NiPhysXMeshDesc meshDescBlock => new NifMetadata.NiPhysXMeshDescMetadata(item.BlockIndex, meshDescBlock.Name, MeshName: meshDescBlock.Name, MeshDataIndex: nxsMeshIndex),
                NiPhysXProp prop => new NifMetadata.NiPhysXPropMetadata(item.BlockIndex, prop.Name, PhysXToWorldScale: prop.PhysXToWorldScale, Snapshot: prop.Snapshot?.BlockIndex ?? -1),
                NiPhysXPropDesc propDesc => new NifMetadata.NiPhysXPropDescMetadata(item.BlockIndex, propDesc.Name, Actors: propDesc.Actors.Select(actor => actor.BlockIndex).ToList()),
                NiPhysXShapeDesc shapeDesc => new NifMetadata.NiPhysXShapeDescMetadata(item.BlockIndex, shapeDesc.Name, LocalPose: shapeDesc.LocalPose, ShapeType: (NxShapeType) shapeDesc.ShapeType, BoxHalfExtents: shapeDesc.BoxHalfExtents, Mesh: shapeDesc.Mesh?.BlockIndex ?? -1),
                _ => new NifMetadata.NifBlockMetadata(item.BlockIndex, item.Name)
            };
            yield return nifBlockMetadata;
        }
    }
}
