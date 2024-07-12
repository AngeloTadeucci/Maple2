using Maple2.File.IO.Nif;
using Maple2.File.Parser;
using Maple2.Model.Common;
using Maple2.Model.Metadata;
using Maple2.Tools.Extensions;
using Maple2.Tools.VectorMath;
using System.Numerics;

namespace Maple2.File.Ingest.Helpers;

public static class NifParserHelper {
    public static Dictionary<uint, NifDocument> nifDocuments { get; private set; } = [];
    public static Dictionary<uint, BoundingBox3> nifBounds { get; private set; } = [];
    public static Dictionary<string, int> nxsMeshIndexMap { get; private set; } = [];
    public static List<NxsMeshMetadata> nxsMeshes { get; private set; } = [];

    public static void ParseNif(List<PrefixedM2dReader> modelReaders) {
        NifParser nifParser = new(modelReaders);

        Parallel.ForEach(nifParser.Parse(), (item) => {
            ParseNifDocument(item.llid, item.document);
        });

        nifDocuments = nifDocuments.OrderBy(item => item.Key).ToDictionary(item => item.Key, item => item.Value);

        foreach (KeyValuePair<uint, NifDocument> nifDocument in nifDocuments) {
            nifBounds.Add(nifDocument.Key, GenerateNxsMeshMetadata(nifDocument.Value));
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

    private static BoundingBox3 GenerateNxsMeshMetadata(NifDocument document) {
        foreach (NiPhysXMeshDesc meshDesc in document.Blocks.OfType<NiPhysXMeshDesc>()) {
            string meshDataString = Convert.ToBase64String(meshDesc.MeshData);
            if (!nxsMeshIndexMap.ContainsKey(meshDataString)) {
                int value = nxsMeshes.Count + 1; // 1-based index
                nxsMeshIndexMap[meshDataString] = value;

                Vector3 min = new Vector3();
                Vector3 max = new Vector3();

                PhysXMesh mesh = new PhysXMesh(meshDesc.MeshData);

                nxsMeshes.Add(new NxsMeshMetadata(value, meshDesc.MeshData, BoundingBox3.Compute(mesh.Vertices)));
            }
        }

        BoundingBox3 bounds = new BoundingBox3();
        bool firstSet = true;

        foreach (NifBlock item in document.Blocks) {
            if (item is not NiPhysXProp prop) {
                continue;
            }

            if (prop.Snapshot is null) {
                continue;
            }

            foreach (NiPhysXActorDesc actorDesc in prop.Snapshot.Actors) {
                foreach (NiPhysXShapeDesc shapeDesc in actorDesc.ShapeDescriptions) {
                    if (shapeDesc.Mesh is null) {
                        continue;
                    }

                    PhysXMesh mesh = new PhysXMesh(shapeDesc.Mesh.MeshData);
                    Matrix4x4 transform = Matrix4x4.CreateScale(prop.PhysXToWorldScale) * actorDesc.Poses[0] * shapeDesc.LocalPose;
                    BoundingBox3 meshBounds = BoundingBox3.Transform(BoundingBox3.Compute(mesh.Vertices), transform);

                    if (!firstSet) {
                        bounds = bounds.Expand(meshBounds);

                        continue;
                    }

                    bounds = meshBounds;
                    firstSet = false;
                }
            }
        }

        return bounds;
    }
}
