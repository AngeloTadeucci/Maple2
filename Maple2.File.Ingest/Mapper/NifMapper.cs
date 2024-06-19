using Maple2.File.IO.Nif;
using Maple2.File.Parser;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class NifMapper : TypeMapper<NifMetadata> {
    private readonly NifParser nifParser;
    public Dictionary<uint, NifDocument> nifDocuments;
    public List<NXSMeshMetadata> nxsMeshes;

    public NifMapper(List<PrefixedM2dReader> modelReaders) {
        nifParser = new NifParser(modelReaders);

        nxsMeshes = [];
        nifDocuments = [];
        foreach ((uint llid, string _, NifDocument document) in nifParser.Parse()) {
            try {
                document.Parse();
            } catch (InvalidOperationException ex) {
                if (ex.InnerException is NifVersionNotSupportedException) {
                    Console.WriteLine(ex.InnerException.Message);
                    continue;
                }
                throw;
            }

            nifDocuments[llid] = document;
        }
    }

    protected override IEnumerable<NifMetadata> Map() {
        Dictionary<string, int> nxsMeshIndexMap = [];

        foreach ((uint llid, NifDocument document) in nifDocuments) {
            List<NifMetadata.NifBlockMetadata> blocks = [];
            foreach (NifBlock item in document.Blocks) {
                int nxsMeshIndex = -1;
                if (item is NiPhysXMeshDesc meshDesc) {
                    string meshDataString = Convert.ToBase64String(meshDesc.MeshData);
                    if (!nxsMeshIndexMap.TryGetValue(meshDataString, out int value)) {
                        value = nxsMeshes.Count;
                        nxsMeshIndexMap[meshDataString] = value;
                        nxsMeshes.Add(new NXSMeshMetadata(value, meshDesc.MeshData));
                    }
                    nxsMeshIndex = value;
                }

                blocks.Add(item switch {
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
                });
            }

            yield return new NifMetadata(
                llid,
                blocks.ToArray()
            );
        }
    }
}