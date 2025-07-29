using Maple2.Tools.VectorMath;
using System.Numerics;

namespace Maple2.Server.DebugGame.Graphics.Scene;

public class Camera {
    public Transform Transform { get; init; } = new Transform();
    public Matrix4x4 ProjectionMatrix { get; private set; } = Matrix4x4.Identity;
    public Matrix4x4 ViewMatrix { get; private set; } = Matrix4x4.Identity;

    public float AspectRatio { get; private set; }
    public float NearPlane { get; private set; }
    public float FarPlane { get; private set; }
    public float FieldOfView { get; private set; }

    /// <summary>
    /// Updates the projection matrix based on window size
    /// </summary>
    public void UpdateProjectionMatrix(int windowWidth, int windowHeight) {
        float aspectRatio = (float) windowWidth / windowHeight;
        ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
            MathF.PI / 4.0f, // 45 degree field of view
            aspectRatio,
            1.0f, // near plane
            50000.0f // far plane - much larger for big fields
        );
        AspectRatio = aspectRatio;
        FieldOfView = MathF.PI / 4.0f;
        NearPlane = 1.0f;
        FarPlane = 50000.0f;
    }

    /// <summary>
    /// Updates the view matrix based on current camera transform
    /// </summary>
    public void UpdateViewMatrix() {
        // Use Transform to calculate view matrix
        Vector3 position = Transform.Position;
        Vector3 forward = Transform.FrontAxis;
        Vector3 up = Transform.UpAxis;

        ViewMatrix = Matrix4x4.CreateLookAt(position, position + forward, up);
    }

    public void SetProperties(float fieldOfView, float aspectRatio, float nearPlane, float farPlane) {
        FieldOfView = fieldOfView;
        AspectRatio = aspectRatio;
        NearPlane = nearPlane;
        FarPlane = farPlane;

        ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlane, farPlane);
    }

    public void SetProperties(float width, float height, float projectionPlane, float nearPlane, float farPlane) {
        float aspectRatio = width / height;
        SetProperties(projectionPlane, aspectRatio, nearPlane, farPlane);
    }
}

