using Maple2.Server.DebugGame.Graphics.Resources;
using Maple2.Server.DebugGame.Graphics.Data;
using System.Numerics;
using Maple2.Server.DebugGame.Graphics.Data.VertexBuffer;

namespace Maple2.Server.DebugGame.Graphics.Assets;
public class CoreModels {
    public DebugGraphicsContext Context { get; init; }
    public Mesh Quad { get; init; }
    public Mesh Cube { get; init; }
    public Mesh WireCube { get; init; }

    public CoreModels(DebugGraphicsContext context) {
        Context = context;
        Quad = CreateQuad();

        var cubes = CreateCubes();

        Cube = cubes.solid;
        WireCube = cubes.wire;
    }

    private Mesh CreateQuad() {
        Ms2MeshData meshData = new Ms2MeshData();

        meshData.PrimitiveCount = 2;

        meshData.SetPositionBinding(new Data.VertexBuffer.PositionBinding[] {
            new PositionBinding(new Vector3(-1,  1, 0)),
            new PositionBinding(new Vector3( 1,  1, 0)),
            new PositionBinding(new Vector3(-1, -1, 0)),
            new PositionBinding(new Vector3( 1, -1, 0))
        });

        meshData.SetAttributeBinding(new Data.VertexBuffer.AttributeBinding[] {
            new AttributeBinding(new Vector3(0, 0, -1), 0xFFFFFFFF, new Vector2(0, 0)),
            new AttributeBinding(new Vector3(0, 0, -1), 0xFFFFFFFF, new Vector2(1, 0)),
            new AttributeBinding(new Vector3(0, 0, -1), 0xFFFFFFFF, new Vector2(0, 1)),
            new AttributeBinding(new Vector3(0, 0, -1), 0xFFFFFFFF, new Vector2(1, 1))
        });

        meshData.SetIndexBuffer(new uint[] {
            0, 1, 3,
            0, 3, 2
        });

        Mesh mesh = new Mesh(Context);

        mesh.UploadData(meshData);

        return mesh;
    }

    private (Mesh solid, Mesh wire) CreateCubes() {
        Ms2MeshData meshData = new Ms2MeshData();

        meshData.PrimitiveCount = 2;

        PositionBinding[] cubeVertices = {
            new PositionBinding(new Vector3(-0.5f,  0.5f, 0)),
            new PositionBinding(new Vector3( 0.5f,  0.5f, 0)),
            new PositionBinding(new Vector3(-0.5f, -0.5f, 0)),
            new PositionBinding(new Vector3( 0.5f, -0.5f, 0)),
            new PositionBinding(new Vector3(-0.5f,  0.5f, 1)),
            new PositionBinding(new Vector3( 0.5f,  0.5f, 1)),
            new PositionBinding(new Vector3(-0.5f, -0.5f, 1)),
            new PositionBinding(new Vector3( 0.5f, -0.5f, 1))
        };

        uint[] cubeSolidIndices = {
            0, 1, 3,
            0, 3, 2,
            4, 7, 5,
            4, 6, 7,
            0, 4, 5,
            0, 5, 1,
            1, 5, 7,
            1, 7, 3,
            3, 7, 6,
            3, 6, 2,
            0, 2, 6,
            0, 6, 4
        };

        meshData.SetPositionBinding(cubeVertices);

        meshData.SetAttributeBinding(new Data.VertexBuffer.AttributeBinding[] {
            new AttributeBinding(new Vector3(0, 0, -1), 0xFFFFFFFF, new Vector2(0, 0)),
            new AttributeBinding(new Vector3(0, 0, -1), 0xFFFFFFFF, new Vector2(0, 0)),
            new AttributeBinding(new Vector3(0, 0, -1), 0xFFFFFFFF, new Vector2(0, 0)),
            new AttributeBinding(new Vector3(0, 0, -1), 0xFFFFFFFF, new Vector2(0, 0)),
            new AttributeBinding(new Vector3(0, 0, -1), 0xFFFFFFFF, new Vector2(0, 0)),
            new AttributeBinding(new Vector3(0, 0, -1), 0xFFFFFFFF, new Vector2(0, 0)),
            new AttributeBinding(new Vector3(0, 0, -1), 0xFFFFFFFF, new Vector2(0, 0)),
            new AttributeBinding(new Vector3(0, 0, -1), 0xFFFFFFFF, new Vector2(0, 0))
        });

        meshData.IsTriangleMesh = false;

        meshData.SetIndexBuffer(new uint[] {
            0, 1,
            1, 3,
            3, 2,
            2, 0,
            0, 4,
            1, 5,
            3, 7,
            2, 6,
            4, 5,
            5, 7,
            7, 6,
            6, 4
        });

        Mesh cubeWireMesh = new Mesh(Context);

        cubeWireMesh.UploadData(meshData);

        List<PositionBinding> positionBinding = new List<PositionBinding>();
        List<AttributeBinding> attributeBinding = new List<AttributeBinding>();

        for (int i = 0; i < cubeSolidIndices.Length; i += 3) {
            PositionBinding vertA = cubeVertices[i + 0];
            PositionBinding vertB = cubeVertices[i + 1];
            PositionBinding vertC = cubeVertices[i + 2];

            positionBinding.Add(vertA);
            positionBinding.Add(vertB);
            positionBinding.Add(vertC);

            Vector3 normal = Vector3.Normalize(Vector3.Cross(vertC.Position - vertA.Position, vertB.Position - vertA.Position));

            attributeBinding.Add(new AttributeBinding(normal, 0xFFFFFFFF, new Vector2(0, 0)));
            attributeBinding.Add(new AttributeBinding(normal, 0xFFFFFFFF, new Vector2(0, 0)));
            attributeBinding.Add(new AttributeBinding(normal, 0xFFFFFFFF, new Vector2(0, 0)));
        }

        meshData.IsTriangleMesh = true;
        meshData.SetPositionBinding(positionBinding.ToArray());
        meshData.SetIndexBuffer(cubeSolidIndices);

        Mesh cubeSolidMesh = new Mesh(Context);

        cubeSolidMesh.UploadData(meshData);

        return (cubeSolidMesh, cubeWireMesh);
    }
}

