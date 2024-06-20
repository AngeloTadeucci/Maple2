using Maple2.File.IO.Nif;
using Maple2.File.Parser;
using Maple2.Model.Metadata;

namespace Maple2.File.Ingest.Helpers;

public static class NifParserHelper {
    public static Dictionary<uint, NifDocument> nifDocuments { get; private set; } = [];
    public static Dictionary<string, int> nxsMeshIndexMap { get; private set; } = [];
    public static List<NxsMeshMetadata> nxsMeshes { get; private set; } = [];

    public static void ParseNif(List<PrefixedM2dReader> modelReaders) {
        NifParser nifParser = new(modelReaders);

        Parallel.ForEach(nifParser.Parse(), (item) => {
            ParseNifDocument(item.llid, item.document);
        });

        nifDocuments = nifDocuments.OrderBy(item => item.Key).ToDictionary(item => item.Key, item => item.Value);

        foreach (KeyValuePair<uint, NifDocument> nifDocument in nifDocuments) {
            GenerateNxsMeshMetadata(nifDocument.Value);
        }
    }

    private static void ParseNifDocument(uint llid, NifDocument document) {
        try {
            document.Parse();
        } catch (InvalidOperationException ex) {
            if (ex.InnerException is NifVersionNotSupportedException) {
#if DEBUG
                Console.WriteLine(ex.InnerException.Message);
#endif
                return;
            }
            throw;
        }

        lock (nifDocuments) {
            nifDocuments[llid] = document;
        }
    }

    private static void GenerateNxsMeshMetadata(NifDocument document) {
        foreach (NiPhysXMeshDesc meshDesc in document.Blocks.OfType<NiPhysXMeshDesc>()) {
            string meshDataString = Convert.ToBase64String(meshDesc.MeshData);
            if (!nxsMeshIndexMap.ContainsKey(meshDataString)) {
                int value = nxsMeshes.Count + 1; // 1-based index
                nxsMeshIndexMap[meshDataString] = value;
                nxsMeshes.Add(new NxsMeshMetadata(value, meshDesc.MeshData));
            }
        }
    }
}
