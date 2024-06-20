using Maple2.File.Ingest.Helpers;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class NxsMeshMapper : TypeMapper<NxsMeshMetadata> {
    protected override IEnumerable<NxsMeshMetadata> Map() {
        foreach (NxsMeshMetadata mesh in NifParserHelper.nxsMeshes) {
            yield return mesh;
        }
    }
}
