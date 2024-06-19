using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class NXSMeshMapper(List<NXSMeshMetadata> nxsMeshes) : TypeMapper<NXSMeshMetadata> {
    private readonly List<NXSMeshMetadata> nxsMeshes = nxsMeshes;

    protected override IEnumerable<NXSMeshMetadata> Map() {
        foreach (NXSMeshMetadata mesh in nxsMeshes) {
            yield return mesh;
        }
    }
}