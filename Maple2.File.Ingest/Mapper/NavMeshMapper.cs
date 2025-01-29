using System.Diagnostics;
using System.Numerics;
using DotRecast.Core;
using DotRecast.Core.Numerics;
using DotRecast.Detour;
using DotRecast.Detour.Extras.Jumplink;
using DotRecast.Detour.Io;
using DotRecast.Recast;
using DotRecast.Recast.Geom;
using DotRecast.Recast.Toolset;
using DotRecast.Recast.Toolset.Builder;
using DotRecast.Recast.Toolset.Tools;
using Maple2.Database.Context;
using Maple2.File.Flat;
using Maple2.File.Flat.maplestory2library;
using Maple2.File.Flat.physxmodellibrary;
using Maple2.File.Flat.standardmodellibrary;
using Maple2.File.Ingest.Helpers;
using Maple2.File.Ingest.Utils;
using Maple2.File.IO;
using Maple2.File.IO.Nif;
using Maple2.File.Parser.Flat;
using Maple2.File.Parser.MapXBlock;
using Maple2.Model.Game.Field;
using Maple2.Model.Metadata.FieldEntity;
using Maple2.Tools;
using Maple2.Tools.DotRecast;
using Maple2.Tools.VectorMath;

namespace Maple2.File.Ingest.Mapper;

public class NavMeshMapper {
    private readonly HashSet<string> xBlocks;
    private readonly XBlockParser mapParser;
    private readonly HashSet<int> upsidedownFaces = []; // make top faces of block that have another block on top of them non-walkable

    private readonly List<string> fileLines = []; // used for debugging

    private readonly List<Vector3> vertexBuffer = [
        new Vector3(-0.75f, 0.75f, 0.0f),
        new Vector3(-0.75f, -0.75f, 0.0f),
        new Vector3(-0.75f, -0.75f, 1.5f),
        new Vector3(-0.75f, 0.75f, 1.5f),
        new Vector3(0.75f, -0.75f, 0.0f),
        new Vector3(0.75f, -0.75f, 1.5f),
        new Vector3(0.75f, 0.75f, 0.0f),
        new Vector3(0.75f, 0.75f, 1.5f),
        // bottom face
        new Vector3(-0.75f, 0.75f, 0.75f),
        new Vector3(-0.75f, -0.75f, 0.75f),
        new Vector3(0.75f, -0.75f, 0.75f),
        new Vector3(0.75f, 0.75f, 0.75f),

    ];

    private readonly List<int> indexBuffer = [
        1,
        4,
        5,
        4,
        6,
        7,
        7,
        5,
        4,
        2,
        5,
        7,
        6,
        4,
        1,
        3,
        7,
        6,
        5,
        2,
        1,
        7,
        3,
        2,
        2,
        3,
        0,
        1,
        0,
        6,
        6,
        0,
        3,
        0,
        1,
        2,
        // bottom face
        11,
        10,
        9,
        9,
        8,
        11,
    ];

    public NavMeshMapper(MetadataContext db, M2dReader exportedReader) {
        xBlocks = db.MapMetadata.Select(metadata => metadata.XBlock).ToHashSet();
        mapParser = new XBlockParser(exportedReader, new FlatTypeIndex(exportedReader));

        Directory.CreateDirectory(Paths.NAVMESH_DIR);
        Directory.CreateDirectory(Paths.NAVMESH_HASH_DIR);

        Map();
    }

    private void Map() {
        // string xblock = "63000016_cs";
        // string xblock = "02000001_tw_tria";
        // string xblock = "82000012_survival";
        // mapParser.ParseMap(xblock, (entities) => GenerateNavMesh(xblock, entities));
        // return;

        mapParser.Parse((xblock, entities) => {
            if (!xBlocks.Contains(xblock)) {
                return;
            }

            GenerateNavMesh(xblock, entities);
        });
    }

    private void GenerateNavMesh(string xblock, IEnumerable<IMapEntity> entities) {
        if (NavmeshHash.HasValidHash(xblock)) {
            Console.WriteLine($"Navmesh already exists for {xblock}");
            return;
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"Parsing {xblock}...");

        List<float> verts = [];
        List<int> tris = [];
        List<int> areas = [];
        foreach (IMapEntity entity in entities) {
            // Only consider entities with 'doesMakeTOK: true'
            if (entity is not IMS2PathEngineTOK { doesMakeTOK: true }) {
                continue;
            }

            NifDocument? document = null;
            if (entity is IMesh mesh) {
                document = GetMeshNifDocument(mesh);
            }

            if (entity is not IPlaceable placeable) {
                continue;
            }

            Transform transform = new() {
                Position = placeable.Position,
                RotationAnglesDegrees = placeable.Rotation
            };

            transform.Transformation *= DotRecastHelper.MapRotation;

            bool isFluid = false;

            if (entity is IPhysXWhitebox whitebox) {
                GenerateCube(whitebox, transform, verts, tris, areas);
            } else if (entity is IMS2MapProperties mapProperties) {
                if (mapProperties.CubeType == "Fluid") {
                    isFluid = true;
                }
                GenerateCube(mapProperties, transform, verts, tris, areas);
            }

            foreach (NiPhysXProp prop in document?.PhysXProps ?? []) {
                if (prop.Snapshot == null) continue;

                foreach (NiPhysXActorDesc actor in prop.Snapshot.Actors) {
                    foreach (NiPhysXShapeDesc shape in actor.ShapeDescriptions) {
                        if (shape.Mesh == null) continue;

                        PhysXMesh physXMesh = new PhysXMesh(shape.Mesh.MeshData);

                        Matrix4x4 scale = Matrix4x4.CreateScale(prop.PhysXToWorldScale);
                        Matrix4x4 matrix = shape.LocalPose * actor.Poses[0] * scale * transform.Transformation;

                        AddPhysxShape(verts, tris, areas, physXMesh, matrix, isFluid);
                    }
                }
            }
        }

        if (verts.Count == 0 || tris.Count == 0) {
            stopwatch.Stop();
            Console.WriteLine($"No mesh data found for {xblock} in {stopwatch.ElapsedMilliseconds}ms");
            return;
        }

        // Used for debugging
        // CreateObjFile(xblock);

        InputGeomProvider geomProvider = new InputGeomProvider(verts, tris);

        RcNavMeshBuildSettings settings = DotRecastHelper.NavMeshBuildSettings;

        RcConfig config = CreateRcConfig(settings, SampleAreaModifications.SAMPLE_AREAMOD_WALKABLE);

        RcBuilderConfig bcfg = new RcBuilderConfig(config, geomProvider.GetMeshBoundsMin(), geomProvider.GetMeshBoundsMax());

        try {
            RcBuilder rcBuilder = new RcBuilder();
            RcContext ctx = new RcContext();

            RcHeightfield solid = BuildSolidHeightfield(tris, areas, geomProvider, bcfg, ctx);

            RcBuilderResult results = rcBuilder.Build(ctx, bcfg.tileX, bcfg.tileZ, geomProvider, bcfg.cfg, solid, keepInterResults: true);

            if (results.SolidHeightfiled == null) {
                return;
            }

            RcJumpLinkBuilderTool jumpLinkBuilder = new();
            RcJumpLinkBuilderToolConfig jumpLinkBuilderConfig = new() {
                buildOffMeshConnections = true,
                buildTypes = JumpLinkType.EDGE_JUMP_BIT,
                groundTolerance = 1.5f,
                edgeJumpEndDistance = 1.5f,
                edgeJumpHeight = 2f,
                edgeJumpDownMaxHeight = 1f,
                edgeJumpUpMaxHeight = 2f,
            };

            // jumpLinkBuilder.Build(geomProvider, settings, [results], jumpLinkBuilderConfig);

            DtMeshData? meshData = BuildMeshData(geomProvider, config.Cs, config.Ch, config.WalkableHeightWorld, config.WalkableRadiusWorld, config.WalkableClimbWorld, results);
            if (meshData == null) {
                return;
            }

            DtNavMesh? navMesh = BuildNavMesh(meshData, DotRecastHelper.VERTS_PER_POLY);
            if (navMesh == null) {
                return;
            }

            string navmeshFilePath = Path.Combine(Paths.NAVMESH_DIR, $"{xblock}.navmesh");

            using FileStream fs = new FileStream(navmeshFilePath, FileMode.Create, FileAccess.Write);
            using BinaryWriter bw = new BinaryWriter(fs);

            DtMeshSetWriter writer = new();
            writer.Write(bw, navMesh, RcByteOrder.LITTLE_ENDIAN, true);
            bw.Close();
            fs.Close();

            NavmeshHash.WriteHash(xblock);

            stopwatch.Stop();
            Console.WriteLine($"Generated navmesh for {xblock} in {stopwatch.ElapsedMilliseconds}ms");
            return;
        } catch (Exception ex) {
            stopwatch.Stop();
            Console.WriteLine($"Failed to generate navmesh for {xblock} due to {ex.Message}");
            return;
        }
    }
    private NifDocument? GetMeshNifDocument(IMesh mesh) {
        if (string.IsNullOrEmpty(mesh.NifAsset)) {
            return null;
        }

        if (!mesh.NifAsset.StartsWith("urn:llid")) {
            Console.WriteLine($"Invalid asset: {mesh.NifAsset} for {mesh.ModelName}");
            return null;
        }

        uint llid = Convert.ToUInt32(mesh.NifAsset.Substring(mesh.NifAsset.LastIndexOf(':') + 1, 8), 16);
        if (!NifParserHelper.nifDocuments.TryGetValue(llid, out NifDocument? document)) {
            Console.WriteLine($"Failed to find asset: {mesh.NifAsset} for {mesh.ModelName}");
        }

        return document;
    }

    private static RcConfig CreateRcConfig(RcNavMeshBuildSettings settings, RcAreaModification walkableAreaMod) {
        return new RcConfig(
            partitionType: (RcPartition) settings.partitioning,
            cellSize: settings.cellSize,
            cellHeight: settings.cellHeight,
            agentMaxSlope: settings.agentMaxSlope,
            agentHeight: settings.agentHeight,
            agentRadius: settings.agentRadius,
            agentMaxClimb: settings.agentMaxClimb,
            regionMinSize: settings.minRegionSize,
            regionMergeSize: settings.mergedRegionSize,
            edgeMaxLen: settings.edgeMaxLen,
            edgeMaxError: settings.edgeMaxError,
            vertsPerPoly: settings.vertsPerPoly,
            detailSampleDist: settings.detailSampleDist,
            detailSampleMaxError: settings.detailSampleMaxError,
            filterLowHangingObstacles: settings.filterLowHangingObstacles,
            filterLedgeSpans: settings.filterLedgeSpans,
            filterWalkableLowHeightSpans: settings.filterWalkableLowHeightSpans,
            walkableAreaMod: walkableAreaMod,
            buildMeshDetail: true
        );
    }

    private static RcHeightfield BuildSolidHeightfield(List<int> tris, List<int> areas, InputGeomProvider geomProvider, RcBuilderConfig bcfg, RcContext ctx) {
        // Allocate voxel heightfield where we rasterize our input data to.
        RcHeightfield solid = new RcHeightfield(bcfg.width, bcfg.height, bcfg.bmin, bcfg.bmax, bcfg.cfg.Cs, bcfg.cfg.Ch, bcfg.cfg.BorderSize);

        foreach (RcTriMesh geom in geomProvider.Meshes()) {
            float[] vertices = geom.GetVerts();

            int[] triangles = geom.GetTris();

            int numTriangles = triangles.Length / 3;
            int[] array = CalculateAreasFlags(tris, areas, bcfg.cfg, vertices, numTriangles);

            RcRasterizations.RasterizeTriangles(ctx, vertices, triangles, array, numTriangles, solid, bcfg.cfg.WalkableClimb);
        }

        return solid;
    }

    // Find triangles which are walkable based on their slope and rasterize them.
    // Also check if the triangle is water and mark it as non-walkable.
    private static int[] CalculateAreasFlags(List<int> tris, List<int> areas, RcConfig cfg, float[] verts2, int ntris) {
        int[] array = areas.ToArray();
        float num = MathF.Cos(cfg.WalkableSlopeAngle / 180f * MathF.PI);
        RcVec3f norm = default;
        for (int i = 0; i < ntris; i++) {
            // Skip water triangles.
            if ((array[i] & SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER) != 0) {
                array[i] = 0;
                continue;
            }

            int num2 = i * 3;
            RcRecast.CalcTriNormal(verts2, tris[num2], tris[num2 + 1], tris[num2 + 2], ref norm);
            if (norm.Y > num) {
                array[i] = cfg.WalkableAreaMod.Apply(array[i]);
            }
        }

        return array;
    }

    private static DtMeshData? BuildMeshData(InputGeomProvider geom, float cellSize, float cellHeight, float agentHeight,
                                             float agentRadius, float agentMaxClimb, RcBuilderResult result) {
        int x = result.TileX;
        int z = result.TileZ;
        RcPolyMesh pmesh = result.Mesh;
        RcPolyMeshDetail dmesh = result.MeshDetail;
        DtNavMeshCreateParams option = new();
        for (int i = 0; i < pmesh.npolys; ++i) {
            pmesh.flags[i] = 1;
        }

        option.verts = pmesh.verts;
        option.vertCount = pmesh.nverts;
        option.polys = pmesh.polys;
        option.polyAreas = pmesh.areas;
        option.polyFlags = pmesh.flags;
        option.polyCount = pmesh.npolys;
        option.nvp = pmesh.nvp;
        if (dmesh != null) {
            option.detailMeshes = dmesh.meshes;
            option.detailVerts = dmesh.verts;
            option.detailVertsCount = dmesh.nverts;
            option.detailTris = dmesh.tris;
            option.detailTriCount = dmesh.ntris;
        }

        option.walkableHeight = agentHeight;
        option.walkableRadius = agentRadius;
        option.walkableClimb = agentMaxClimb;
        option.bmin = pmesh.bmin;
        option.bmax = pmesh.bmax;
        option.cs = cellSize;
        option.ch = cellHeight;
        option.buildBvTree = true;

        List<RcOffMeshConnection>? offMeshConnections = geom.GetOffMeshConnections();
        option.offMeshConCount = offMeshConnections.Count;
        option.offMeshConVerts = new float[option.offMeshConCount * 6];
        option.offMeshConRad = new float[option.offMeshConCount];
        option.offMeshConDir = new int[option.offMeshConCount];
        option.offMeshConAreas = new int[option.offMeshConCount];
        option.offMeshConFlags = new int[option.offMeshConCount];
        option.offMeshConUserID = new int[option.offMeshConCount];
        for (int i = 0; i < option.offMeshConCount; i++) {
            RcOffMeshConnection offMeshCon = offMeshConnections[i];
            for (int j = 0; j < 6; j++) {
                option.offMeshConVerts[6 * i + j] = offMeshCon.verts[j];
            }

            option.offMeshConRad[i] = offMeshCon.radius;
            option.offMeshConDir[i] = offMeshCon.bidir ? 1 : 0;
            option.offMeshConAreas[i] = offMeshCon.area;
            option.offMeshConFlags[i] = offMeshCon.flags;
        }

        option.tileX = x;
        option.tileZ = z;
        DtMeshData? dtMeshData = DtNavMeshBuilder.CreateNavMeshData(option);
        if (dtMeshData != null) {
            return DemoNavMeshBuilder.UpdateAreaAndFlags(dtMeshData);
        }

        return null;
    }

    private static DtNavMesh? BuildNavMesh(DtMeshData meshData, int vertsPerPoly) {
        DtNavMesh navMesh = new();
        DtStatus status = navMesh.Init(meshData, vertsPerPoly, 0);
        if (status.Failed()) {
            return null;
        }
        return navMesh;
    }

    public static DtMeshData UpdateAreaAndFlags(DtMeshData meshData) {
        // Update poly flags from areas.
        for (int i = 0; i < meshData.polys.Length; ++i) {
            int area = meshData.polys[i].GetArea();
            if (area is SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WALKABLE) {
                meshData.polys[i].SetArea(SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GROUND);
            }

            if (area is SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GROUND
                or SampleAreaModifications.SAMPLE_POLYAREA_TYPE_GRASS
                or SampleAreaModifications.SAMPLE_POLYAREA_TYPE_ROAD) {
                meshData.polys[i].flags = SampleAreaModifications.SAMPLE_POLYFLAGS_WALK;
            } else if (area is SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER) {
                meshData.polys[i].flags = SampleAreaModifications.SAMPLE_POLYFLAGS_SWIM;
            } else if (area is SampleAreaModifications.SAMPLE_POLYAREA_TYPE_DOOR) {
                meshData.polys[i].flags = SampleAreaModifications.SAMPLE_POLYFLAGS_DOOR;
            }
        }

        return meshData;
    }

    private void GenerateCube(IMS2MapProperties mapProperties, Transform transform, List<float> verts, List<int> tris, List<int> areas) {
        if (!mapProperties.GeneratePhysX) {
            return;
        }

        Vector3 generatePhysXDimension = mapProperties.GeneratePhysXDimension * (1 / 1.5f);
        if (generatePhysXDimension == Vector3.Zero) {
            generatePhysXDimension = new Vector3(100f, 100f, 100f);
        }

        GenerateCube(generatePhysXDimension, Vector3.Zero, transform, verts, tris, areas);
    }

    private void GenerateCube(IPhysXWhitebox physXWhitebox, Transform transform, List<float> verts, List<int> tris, List<int> areas) {
        Vector3 offset = new Vector3(0, 0, -0.5f * physXWhitebox.ShapeDimensions.Z);
        GenerateCube(physXWhitebox.ShapeDimensions, offset, transform, verts, tris, areas);
    }

    private void GenerateCube(Vector3 size, Vector3 offset, Transform transform, List<float> verts, List<int> tris, List<int> areas) {
        Matrix4x4 matrix = Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(offset) * transform.Transformation;

        int currentVerticeCount = verts.Count / 3;

        foreach (Vector3 vertex in vertexBuffer) {
            Vector3 transformed = Vector3.Transform(vertex, matrix);
            verts.AddRange([transformed.X, transformed.Y, transformed.Z]);
            fileLines.Add($"v {transformed.X} {transformed.Y} {transformed.Z}");
        }

        for (int i = 0; i < indexBuffer.Count; i += 3) {
            tris.AddRange([indexBuffer[i] + currentVerticeCount, indexBuffer[i + 1] + currentVerticeCount, indexBuffer[i + 2] + currentVerticeCount]);
            fileLines.Add($"f {indexBuffer[i] + 1 + currentVerticeCount} {indexBuffer[i + 1] + 1 + currentVerticeCount} {indexBuffer[i + 2] + 1 + currentVerticeCount}");
            areas.Add(0);
        }
    }

    private void AddPhysxShape(List<float> verts, List<int> tris, List<int> areas, PhysXMesh physXMesh, Matrix4x4 matrix, bool isFluid) {
        int currentVerticeCount = verts.Count / 3;

        List<Vector3> vertexBuffer2 = [];

        List<int> indexBuffer2 = [];

        Vector3 offset = new Vector3(0, 0.125f, 0.0f);

        foreach (Vector3 vertex in physXMesh.Vertices) {
            Vector3 transformed = Vector3.Transform(vertex, matrix);
            verts.AddRange([transformed.X, transformed.Y, transformed.Z]);
            fileLines.Add($"v {transformed.X} {transformed.Y} {transformed.Z}");
        }

        foreach (PhysXMeshFace face in physXMesh.Faces) {
            tris.AddRange([(int) face.Vert0 + currentVerticeCount, (int) face.Vert1 + currentVerticeCount, (int) face.Vert2 + currentVerticeCount]);
            fileLines.Add($"f {face.Vert0 + 1 + currentVerticeCount} {face.Vert1 + 1 + currentVerticeCount} {face.Vert2 + 1 + currentVerticeCount}");

            if (isFluid) {
                areas.Add(SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER);
            } else {
                areas.Add(0);
            }

            Vector3 vert0 = Vector3.Transform(physXMesh.Vertices[(int) face.Vert0], matrix);
            Vector3 vert1 = Vector3.Transform(physXMesh.Vertices[(int) face.Vert1], matrix);
            Vector3 vert2 = Vector3.Transform(physXMesh.Vertices[(int) face.Vert2], matrix);

            Vector3 normal = Vector3.Cross(vert1 - vert0, vert2 - vert0);
            normal = Vector3.Normalize(normal);

            if (normal.Y >= -Math.Cos(Math.PI / 2)) {
                continue;
            }

            int faceStart = vertexBuffer2.Count + (verts.Count / 3);
            upsidedownFaces.Add(faceStart);

            indexBuffer2.AddRange([faceStart, faceStart + 1, faceStart + 2]);
            vertexBuffer2.AddRange([vert0 + offset, vert1 + offset, vert2 + offset]);
        }

        foreach (Vector3 vertex in vertexBuffer2) {
            verts.AddRange([vertex.X, vertex.Y, vertex.Z]);
            fileLines.Add($"v {vertex.X} {vertex.Y} {vertex.Z}");
        }

        for (int i = 0; i < indexBuffer2.Count; i += 3) {
            tris.AddRange([indexBuffer2[i], indexBuffer2[i + 1], indexBuffer2[i + 2]]);
            fileLines.Add($"f {indexBuffer2[i] + 1} {indexBuffer2[i + 1] + 1} {indexBuffer2[i + 2] + 1}");
            if (isFluid) {
                areas.Add(SampleAreaModifications.SAMPLE_POLYAREA_TYPE_WATER);
            } else {
                areas.Add(0);
            }
        }
    }

    // used for debugging
    private void CreateObjFile(string xblock) {
        // create a new file if it doesn't exist
        if (System.IO.File.Exists($"{xblock}.obj")) {
            System.IO.File.Delete($"{xblock}.obj");
        }

        var file = System.IO.File.Create($"{xblock}.obj");
        using StreamWriter streamWriter = new StreamWriter(file);
        foreach (string line in fileLines) {
            streamWriter.WriteLine(line);
        }
        streamWriter.Close();
        fileLines.Clear();
    }

    #region TileMesh configuration
    // tiling configuration
    //     var config = new RcConfig(
    //             useTiles: true,
    //             tileSizeX: TileSize,
    //             tileSizeZ: TileSize,
    //             borderSize: RcConfig.CalcBorder(0.3f, CellSize),
    //             partition: RcPartition.WATERSHED,
    //             cellSize: CellSize,
    //             cellHeight: CellSize,
    //             agentMaxSlope: 47f, // generally 45 degrees, but we add a bit more to account for floating point errors
    //             agentMaxClimb: 0.7f,
    //             agentHeight: 1.4f, // approximation of character height
    //             agentRadius: 0.3f, // approximation of character radius
    //             minRegionArea: 8 * 8 * CellSize * CellSize,
    //             mergeRegionArea: 20 * 20 * CellSize * CellSize,
    //             edgeMaxLen: 12.0f,
    //             edgeMaxError: 1.3f,
    //             vertsPerPoly: VertsPerPoly,
    //             detailSampleDist: 6.0f,
    //             detailSampleMaxError: 1.0f,
    //             filterLowHangingObstacles: true,
    //             filterLedgeSpans: true,
    //             filterWalkableLowHeightSpans: true,
    //             walkableAreaMod: new RcAreaModification(0x3f),
    //             buildMeshDetail: true
    //         );

    //         try {
    //             RcBuilder rcBuilder = new();
    //     List<RcBuilderResult> results = rcBuilder.BuildTiles(geomProvider, config, true, true, Environment.ProcessorCount + 1, Task.Factory);

    //     List<DtMeshData> tileMeshData = BuildMeshData(geomProvider, config.Cs, config.Ch, config.WalkableHeightWorld, config.WalkableRadiusWorld, config.WalkableClimbWorld, results);
    //     DtNavMesh tileNavMesh = BuildNavMesh(geomProvider, tileMeshData, config.Cs, TileSize, VertsPerPoly);

    //     string navmeshFilePath = $"navmeshes/{xblock}.navmesh";

    //             using var fs = new FileStream(navmeshFilePath, FileMode.Create, FileAccess.Write);
    //             using var bw = new BinaryWriter(fs);

    // DtMeshSetWriter writer = new();
    // writer.Write(bw, tileNavMesh, RcByteOrder.LITTLE_ENDIAN, true);
    //         } catch (Exception ex) {
    //             Console.WriteLine($"Failed to generate navmesh for {xblock} due to {ex.Message}");
    // return;
    //         }
    // public static List<DtMeshData> BuildMeshData(IInputGeomProvider geom, float cellSize, float cellHeight, float agentHeight,
    //        float agentRadius, float agentMaxClimb, IList<RcBuilderResult> results) {
    //     List<DtMeshData> meshData = [];
    //     foreach (RcBuilderResult result in results) {
    //         int x = result.TileX;
    //         int z = result.TileZ;
    //         RcPolyMesh pmesh = result.Mesh;
    //         RcPolyMeshDetail dmesh = result.MeshDetail;
    //         DtNavMeshCreateParams option = new();
    //         for (int i = 0; i < pmesh.npolys; ++i) {
    //             pmesh.flags[i] = 1;
    //         }

    //         option.verts = pmesh.verts;
    //         option.vertCount = pmesh.nverts;
    //         option.polys = pmesh.polys;
    //         option.polyAreas = pmesh.areas;
    //         option.polyFlags = pmesh.flags;
    //         option.polyCount = pmesh.npolys;
    //         option.nvp = pmesh.nvp;
    //         if (dmesh != null) {
    //             option.detailMeshes = dmesh.meshes;
    //             option.detailVerts = dmesh.verts;
    //             option.detailVertsCount = dmesh.nverts;
    //             option.detailTris = dmesh.tris;
    //             option.detailTriCount = dmesh.ntris;
    //         }

    //         option.walkableHeight = agentHeight;
    //         option.walkableRadius = agentRadius;
    //         option.walkableClimb = agentMaxClimb;
    //         option.bmin = pmesh.bmin;
    //         option.bmax = pmesh.bmax;
    //         option.cs = cellSize;
    //         option.ch = cellHeight;
    //         option.buildBvTree = true;

    //         // TODO: Off-mesh connections
    //         // var offMeshConnections = geom.GetOffMeshConnections();
    //         // option.offMeshConCount = offMeshConnections.Count;
    //         // option.offMeshConVerts = new float[option.offMeshConCount * 6];
    //         // option.offMeshConRad = new float[option.offMeshConCount];
    //         // option.offMeshConDir = new int[option.offMeshConCount];
    //         // option.offMeshConAreas = new int[option.offMeshConCount];
    //         // option.offMeshConFlags = new int[option.offMeshConCount];
    //         // option.offMeshConUserID = new int[option.offMeshConCount];
    //         // for (int i = 0; i < option.offMeshConCount; i++) {
    //         //     RcOffMeshConnection offMeshCon = offMeshConnections[i];
    //         //     for (int j = 0; j < 6; j++) {
    //         //         option.offMeshConVerts[6 * i + j] = offMeshCon.verts[j];
    //         //     }

    //         //     option.offMeshConRad[i] = offMeshCon.radius;
    //         //     option.offMeshConDir[i] = offMeshCon.bidir ? 1 : 0;
    //         //     option.offMeshConAreas[i] = offMeshCon.area;
    //         //     option.offMeshConFlags[i] = offMeshCon.flags;
    //         //     // option.offMeshConUserID[i] = offMeshCon.userId;
    //         // }

    //         option.tileX = x;
    //         option.tileZ = z;
    //         var dtMeshData = DtNavMeshBuilder.CreateNavMeshData(option);
    //         if (dtMeshData != null) {
    //             meshData.Add(DemoNavMeshBuilder.UpdateAreaAndFlags(dtMeshData));
    //         }
    //     }

    //     return meshData;
    // }

    // public static DtNavMesh BuildNavMesh(IInputGeomProvider geom, List<DtMeshData> meshData, float cellSize, int tileSize, int vertsPerPoly) {
    //     DtNavMeshParams navMeshParams = new() {
    //         orig = geom.GetMeshBoundsMin(),
    //         tileWidth = tileSize * cellSize,
    //         tileHeight = tileSize * cellSize,

    //         maxTiles = GetMaxTiles(geom, cellSize, tileSize),
    //         maxPolys = GetMaxPolysPerTile(geom, cellSize, tileSize)
    //     };

    //     DtNavMesh navMesh = new();
    //     navMesh.Init(navMeshParams, vertsPerPoly);
    //     meshData.ForEach(md => navMesh.AddTile(md, 0, 0, out long _));
    //     return navMesh;
    // }
    // public static int GetMaxTiles(IInputGeomProvider geom, float cellSize, int tileSize) {
    //     int tileBits = GetTileBits(geom, cellSize, tileSize);
    //     return 1 << tileBits;
    // }

    // public static int GetMaxPolysPerTile(IInputGeomProvider geom, float cellSize, int tileSize) {
    //     int polyBits = 22 - GetTileBits(geom, cellSize, tileSize);
    //     return 1 << polyBits;
    // }

    // private static int GetTileBits(IInputGeomProvider geom, float cellSize, int tileSize) {
    //     RcRecast.CalcGridSize(geom.GetMeshBoundsMin(), geom.GetMeshBoundsMax(), cellSize, out int gw, out int gh);
    //     int tw = (gw + tileSize - 1) / tileSize;
    //     int th = (gh + tileSize - 1) / tileSize;
    //     int tileBits = Math.Min(DtUtils.Ilog2(DtUtils.NextPow2(tw * th)), 14);
    //     return tileBits;
    // }
    #endregion
}
