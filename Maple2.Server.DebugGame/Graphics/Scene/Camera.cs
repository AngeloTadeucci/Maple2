using Maple2.Tools.VectorMath;
using System.Numerics;

namespace Maple2.Server.DebugGame.Graphics.Scene;

public class Camera {
    public Transform Transform { get; init; } = new Transform();

    public float AspectRatio { get; private set; }
    public float NearPlane { get; private set; }
    public float FarPlane { get; private set; }
    public float FieldOfView { get; private set; }

    /// <summary>
    /// Gets the current view matrix based on camera transform
    /// </summary>
    public Matrix4x4 ViewMatrix {
        get {
            Vector3 position = Transform.Position;
            Vector3 forward = Transform.FrontAxis;
            Vector3 up = Transform.UpAxis;
            return Matrix4x4.CreateLookAt(position, position + forward, up);
        }
    }

    /// <summary>
    /// Gets the current projection matrix based on camera properties
    /// </summary>
    public Matrix4x4 ProjectionMatrix {
        get {
            return Matrix4x4.CreatePerspectiveFieldOfView(FieldOfView, AspectRatio, NearPlane, FarPlane);
        }
    }

    /// <summary>
    /// Updates the projection matrix based on window size
    /// </summary>
    public void UpdateProjectionMatrix(int windowWidth, int windowHeight) {
        float aspectRatio = (float) windowWidth / windowHeight;
        AspectRatio = aspectRatio;
        FieldOfView = MathF.PI / 4.0f;
        NearPlane = 1.0f;
        FarPlane = 50000.0f;
    }

    public void SetProperties(float fieldOfView, float aspectRatio, float nearPlane, float farPlane) {
        FieldOfView = fieldOfView;
        AspectRatio = aspectRatio;
        NearPlane = nearPlane;
        FarPlane = farPlane;
    }

    public void SetProperties(float width, float height, float projectionPlane, float nearPlane, float farPlane) {
        float aspectRatio = width / height;
        SetProperties(projectionPlane, aspectRatio, nearPlane, farPlane);
    }
}

