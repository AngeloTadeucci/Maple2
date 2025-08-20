using Silk.NET.Input;
using System.Numerics;
using Serilog;

namespace Maple2.Server.DebugGame.Graphics.Scene;

public class FreeCameraController : ICameraController {
    private static readonly ILogger Logger = Log.Logger.ForContext<FreeCameraController>();

    const float MAX_PITCH = (float.Pi / 180) * 89;

    public InputState InputState { get; } = new();
    public Camera Camera { get; }

    public Vector3 CameraTarget { get; private set; } = Vector3.Zero;

    private const Key MoveForward = Key.W;
    private const Key MoveBackward = Key.S;
    private const Key MoveLeft = Key.A;
    private const Key MoveRight = Key.D;
    private const Key MoveUp = Key.E;
    private const Key MoveDown = Key.Q;

    public readonly float FlySpeed = 500;
    public readonly float BaseRotationSpeed = (1 / 1000.0f) * float.Pi; // 500 pixels for 1/4 of a rotation
    public float RotationSpeed = 1;
    public float RotationSpeedDegrees {
        get => (180.0f / float.Pi) * RotationSpeed;
        set => RotationSpeed = (float.Pi / 180.0f) * value;
    }

    // Default camera rotation for MapleStory 2 coordinate system
    private static readonly Matrix4x4 DefaultCameraRotation = new Matrix4x4(
        -0.5347f, 0.8451f, 0.0000f, 0.0000f,
        -0.6053f, -0.3830f, 0.6978f, 0.0000f,
        0.5897f, 0.3731f, 0.7163f, 0.0000f,
        0.0000f, 0.0000f, 0.0000f, 1.0000f
    );

    // Field overview camera rotation
    private static readonly Matrix4x4 FieldOverviewRotation = new Matrix4x4(
        -1.0000f, 0.0018f, 0.0000f, 0.0000f,
        -0.0013f, -0.6994f, 0.7147f, 0.0000f,
        0.0013f, 0.7147f, 0.6994f, 0.0000f,
        0.0000f, 0.0000f, 0.0000f, 1.0000f
    );

    public FreeCameraController(Camera camera) {
        Camera = camera;
    }

    /// <summary>
    /// Sets the camera to the default rotation for MapleStory 2
    /// </summary>
    public void SetDefaultRotation() {
        Camera.Transform.RotationMatrix = DefaultCameraRotation;
    }

    /// <summary>
    /// Sets the camera to the field overview rotation
    /// </summary>
    public void SetFieldOverviewRotation() {
        Camera.Transform.RotationMatrix = FieldOverviewRotation;
    }

    public void Update(float delta) {
        if (!InputState.InputFocused) {
            return;
        }

        Vector3 moveDirection = new();

        if (InputState.GetState(MoveForward).IsDown) {
            moveDirection.Y += 1;
        }

        if (InputState.GetState(MoveBackward).IsDown) {
            moveDirection.Y -= 1;
        }

        if (InputState.GetState(MoveRight).IsDown) {
            moveDirection.X += 1;
        }

        if (InputState.GetState(MoveLeft).IsDown) {
            moveDirection.X -= 1;
        }

        if (InputState.GetState(MoveUp).IsDown) {
            moveDirection.Z += 1;
        }

        if (InputState.GetState(MoveDown).IsDown) {
            moveDirection.Z -= 1;
        }

        // Apply speed modifier with Shift key
        float currentSpeed = FlySpeed;
        if (InputState.GetState(Key.ShiftLeft).IsDown || InputState.GetState(Key.ShiftRight).IsDown) {
            currentSpeed *= 5.0f; // 5x speed with Shift
        }

        Camera.Transform.Position += delta * currentSpeed * (moveDirection.X * Camera.Transform.RightAxis + moveDirection.Y * Camera.Transform.FrontAxis + moveDirection.Z * Camera.Transform.UpAxis);

        if (!InputState.MouseRight.IsDown) {
            return;
        }

        Vector3 mouseDelta = InputState.MousePosition.Delta;

        float currentPitch = -float.Asin(Camera.Transform.FrontAxis.Z);
        float currentYaw = -float.Atan2(-Camera.Transform.FrontAxis.X, -Camera.Transform.FrontAxis.Y);

        currentPitch = float.Max(-MAX_PITCH, float.Min(MAX_PITCH, currentPitch + RotationSpeed * BaseRotationSpeed * mouseDelta.Y));
        currentYaw -= RotationSpeed * BaseRotationSpeed * mouseDelta.X;

        Matrix4x4 rotation = Matrix4x4.Multiply(Matrix4x4.CreateRotationX(currentPitch), Matrix4x4.CreateRotationZ(currentYaw));
        Vector3 position = Camera.Transform.Position;

        Camera.Transform.Transformation = rotation;
        Camera.Transform.Position = position;

        InputState.MousePosition.Lock = true;
    }

    /// <summary>
    /// Rotates the camera around the target point
    /// </summary>
    public void RotateCamera(float yawDelta, float pitchDelta) {
        // Rotate camera around target
        Vector3 direction = Camera.Transform.Position - CameraTarget;
        float distance = direction.Length();

        if (distance < 0.01f) return; // Avoid division by zero

        // Convert to spherical coordinates
        float currentYaw = MathF.Atan2(direction.Z, direction.X);
        float currentPitch = MathF.Asin(Math.Clamp(direction.Y / distance, -1.0f, 1.0f));

        // Apply rotation
        currentYaw += yawDelta;
        currentPitch = Math.Clamp(currentPitch + pitchDelta, -MathF.PI * 0.4f, MathF.PI * 0.4f);

        // Convert back to cartesian
        float x = distance * MathF.Cos(currentPitch) * MathF.Cos(currentYaw);
        float y = distance * MathF.Sin(currentPitch);
        float z = distance * MathF.Cos(currentPitch) * MathF.Sin(currentYaw);

        Camera.Transform.Position = CameraTarget + new Vector3(x, y, z);
    }

    /// <summary>
    /// Moves the camera relative to its current position
    /// </summary>
    public void MoveCameraRelative(Vector3 offset) {
        Camera.Transform.Position += offset;
        CameraTarget += offset;
    }

    /// <summary>
    /// Sets the camera target
    /// </summary>
    public void SetCameraTarget(Vector3 target) {
        CameraTarget = target;
        Vector3 direction = Vector3.Normalize(target - Camera.Transform.Position);

        // Use Transform's LookTo method instead of manual matrix calculation
        Camera.Transform.LookTo(direction, Vector3.UnitZ, snapToGroundPlane: false);
    }

    /// <summary>
    /// Sets camera orientation that works well with MapleStory 2's coordinate system
    /// </summary>
    public void SetCameraOrientationForMapData() {
        // Set camera orientation that works well with the transformed map coordinate system
        Vector3 offset = new Vector3(0, -5, 5); // Look up from below and in front (inverted from typical)
        Camera.Transform.Position = CameraTarget + offset;
    }

    /// <summary>
    /// Flips the camera's up vector to fix upside-down view
    /// </summary>
    public void FlipCameraUpVector() {
        // Flip the up axis in the transform
        Matrix4x4 currentTransform = Camera.Transform.Transformation;
        Matrix4x4 flipMatrix = Matrix4x4.CreateScale(1, 1, -1);
        Camera.Transform.Transformation = currentTransform * flipMatrix;
    }
}
