using Maple2.File.IO;
using Maple2.File.IO.Crypto.Common;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Mapper;

public class NavMeshMapper(M2dReader terrainReader) : TypeMapper<NavMesh> {

    protected override IEnumerable<NavMesh> Map() {
        foreach (PackFileEntry entry in terrainReader.Files) {
            string xblock = Path.GetFileNameWithoutExtension(entry.Name);
            yield return new NavMesh(xblock, terrainReader.GetBytes(entry));
        }
    }
}
