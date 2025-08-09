using System.Numerics;

namespace Maple2.Server.DebugGame.Graphics.Data;

public struct InstanceBuffer {
    public Matrix4x4 Transformation;
    public Matrix4x4 InverseTransformation;
    public Vector4 Color;
}
