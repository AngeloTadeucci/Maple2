using Maple2.Server.DebugGame.Graphics.Resources;
using Maple2.Server.DebugGame.Graphics.Data;
using System.Numerics;
using Maple2.Server.DebugGame.Graphics.Data.VertexBuffer;

namespace Maple2.Server.DebugGame.Graphics.Assets;
public class CoreModels {
    public DebugGraphicsContext Context { get; init; }
    public Mesh Quad { get; init; }

    public CoreModels(DebugGraphicsContext context) {
        Context = context;
        Quad = CreateQuad();
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
}

