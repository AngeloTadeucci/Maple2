using Maple2.Tools.VectorMath;
using System.Numerics;

namespace Maple2.Server.DebugGame.Graphics.Scene;

public class Camera {
    public Transform Transform { get; init; } = new Transform();
    public Matrix4x4 ProjectionMatrix { get; private set; }

    public float AspectRatio { get; private set; }
    public float NearPlane { get; private set; }
    public float FarPlane {  get; private set; }
    public float FieldOfView {  get; private set; }

    public void SetProperties(float fieldOfView, float aspectRatio, float nearPlane, float farPlane) {

    }

    public void SetProperties(float width, float height, float projectionPlane, float nearPlane, float farPlane) {

    }
}

