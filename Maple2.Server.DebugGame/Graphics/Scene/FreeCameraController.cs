using Silk.NET.Input;
using System.Numerics;
using Serilog;

namespace Maple2.Server.DebugGame.Graphics.Scene;

public class FreeCameraController {
    private static readonly ILogger Logger = Log.Logger.ForContext<FreeCameraController>();

    const float MAX_PITCH = (float.Pi / 180) * 89;

    public InputState? InputState;
    public Camera? Camera;

    // 3D rendering matrices
    public Matrix4x4 ViewMatrix { get; private set; } = Matrix4x4.Identity;
    public Matrix4x4 ProjectionMatrix { get; private set; } = Matrix4x4.Identity;

    // Rendering modes
    public bool WireframeMode { get; set; } = true; // Start in wireframe mode

    // Camera follow system
    public bool IsFollowingPlayer { get; private set; }
    public long? FollowedPlayerId { get; private set; }
    public bool HasManuallyStopped { get; private set; }

    // Camera properties that delegate to Camera.Transform
    public Vector3 CameraPosition => Camera?.Transform.Position ?? Vector3.Zero;
    public Vector3 CameraTarget { get; private set; } = Vector3.Zero;
    public Quaternion CameraRotation => Camera?.Transform.Quaternion ?? Quaternion.Identity;

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

    // Default camera follow offset
    private static readonly Vector3 CameraFollowOffset = new Vector3(-800, -1000, 1200);

    // Default camera rotation for MapleStory 2 coordinate system
    private static readonly Quaternion DefaultCameraRotation = new Quaternion(0.130f, 0.325f, 0.870f, 0.350f);

    // Field overview camera rotation
    private static readonly Quaternion FieldOverviewRotation = new Quaternion(0.000f, 0.450f, 0.900f, 0.000f);

    public FreeCameraController() {
        UpdateViewMatrix();
    }

    /// <summary>
    /// Sets the camera to the default rotation for MapleStory 2
    /// </summary>
    public void SetDefaultRotation() {
        if (Camera != null) {
            Camera.Transform.Quaternion = DefaultCameraRotation;
            UpdateViewMatrix();
        }
    }

    /// <summary>
    /// Sets the camera to the field overview rotation
    /// </summary>
    public void SetFieldOverviewRotation() {
        if (Camera != null) {
            Camera.Transform.Quaternion = FieldOverviewRotation;
            UpdateViewMatrix();
        }
    }

    public void Update(float delta) {
        if (InputState is null || Camera is null || !InputState.InputFocused) {
            return;
        }

        Vector3 moveDirection = new();
        bool hasMovementInput = false;

        if (InputState.GetState(MoveForward).IsDown) {
            moveDirection.Y += 1;
            hasMovementInput = true;
        }

        if (InputState.GetState(MoveBackward).IsDown) {
            moveDirection.Y -= 1;
            hasMovementInput = true;
        }

        if (InputState.GetState(MoveRight).IsDown) {
            moveDirection.X += 1;
            hasMovementInput = true;
        }

        if (InputState.GetState(MoveLeft).IsDown) {
            moveDirection.X -= 1;
            hasMovementInput = true;
        }

        if (InputState.GetState(MoveUp).IsDown) {
            moveDirection.Z += 1;
            hasMovementInput = true;
        }

        if (InputState.GetState(MoveDown).IsDown) {
            moveDirection.Z -= 1;
            hasMovementInput = true;
        }

        // Apply speed modifier with Shift key
        float currentSpeed = FlySpeed;
        if (InputState.GetState(Key.ShiftLeft).IsDown || InputState.GetState(Key.ShiftRight).IsDown) {
            currentSpeed *= 5.0f; // 5x speed with Shift
        }

        if (hasMovementInput) {
            Camera.Transform.Position += delta * currentSpeed * (moveDirection.X * Camera.Transform.RightAxis + moveDirection.Y * Camera.Transform.FrontAxis + moveDirection.Z * Camera.Transform.UpAxis);

            // Disable follow when manual movement is detected
            if (IsFollowingPlayer) {
                StopFollowingPlayer();
            }
        }

        if (!InputState.MouseRight.IsDown) {
            return;
        }

        Vector3 mouseDelta = InputState.MousePosition.Delta;

        // Check if there's actual mouse movement
        if (mouseDelta.X != 0 || mouseDelta.Y != 0) {
            // Disable follow when mouse rotation is detected
            if (IsFollowingPlayer) {
                StopFollowingPlayer();
            }
        }

        float currentPitch = -float.Asin(Camera.Transform.FrontAxis.Z);
        float currentYaw = -float.Atan2(-Camera.Transform.FrontAxis.X, -Camera.Transform.FrontAxis.Y);

        currentPitch = float.Max(-MAX_PITCH, float.Min(MAX_PITCH, currentPitch + RotationSpeed * BaseRotationSpeed * mouseDelta.Y));
        currentYaw -= RotationSpeed * BaseRotationSpeed * mouseDelta.X;

        Matrix4x4 rotation = Matrix4x4.Multiply(Matrix4x4.CreateRotationX(currentPitch), Matrix4x4.CreateRotationZ(currentYaw));
        Vector3 position = Camera.Transform.Position;

        Camera.Transform.Transformation = rotation;
        Camera.Transform.Position = position;

        InputState.MousePosition.Lock = true;

        // Update matrices after camera transform changes
        UpdateViewMatrix();
    }

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
    }

    /// <summary>
    /// Updates the view matrix based on current camera transform
    /// </summary>
    public void UpdateViewMatrix() {
        if (Camera == null) return;

        // Use Camera.Transform to calculate view matrix
        Vector3 position = Camera.Transform.Position;
        Vector3 forward = Camera.Transform.FrontAxis;
        Vector3 up = Camera.Transform.UpAxis;

        CameraTarget = position + forward;
        ViewMatrix = Matrix4x4.CreateLookAt(position, CameraTarget, up);
    }

    /// <summary>
    /// Sets the camera position using Camera.Transform
    /// </summary>
    public void SetCameraPosition(Vector3 position) {
        if (Camera == null) return;
        Camera.Transform.Position = position;
        UpdateViewMatrix();
    }

    /// <summary>
    /// Sets the camera target and updates camera orientation
    /// </summary>
    public void SetCameraTarget(Vector3 target) {
        if (Camera == null) return;

        CameraTarget = target;
        Vector3 direction = Vector3.Normalize(target - Camera.Transform.Position);

        // Calculate rotation to look at target
        Vector3 up = Vector3.UnitZ; // Z is up in our coordinate system
        Vector3 right = Vector3.Normalize(Vector3.Cross(direction, up));
        up = Vector3.Cross(right, direction);

        Matrix4x4 lookAtMatrix = new Matrix4x4(
            right.X, up.X, -direction.X, 0,
            right.Y, up.Y, -direction.Y, 0,
            right.Z, up.Z, -direction.Z, 0,
            0, 0, 0, 1
        );

        Camera.Transform.Transformation = lookAtMatrix;
        UpdateViewMatrix();
    }

    /// <summary>
    /// Rotates the camera around the target point
    /// </summary>
    public void RotateCamera(float yawDelta, float pitchDelta) {
        if (Camera == null) return;

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

        SetCameraPosition(CameraTarget + new Vector3(x, y, z));
    }

    /// <summary>
    /// Moves the camera relative to its current position
    /// </summary>
    public void MoveCameraRelative(Vector3 offset) {
        if (Camera == null) return;

        Camera.Transform.Position += offset;
        CameraTarget += offset;
        UpdateViewMatrix();

        // Unlock camera follow when manually moving
        if (IsFollowingPlayer && offset.LengthSquared() > 0.01f) {
            UnlockCameraFollow();
        }
    }

    /// <summary>
    /// Starts following a specific player
    /// </summary>
    public void StartFollowingPlayer(long playerId) {
        IsFollowingPlayer = true;
        FollowedPlayerId = playerId;
        HasManuallyStopped = false; // Reset manual stop flag when starting to follow
        SetDefaultRotation(); // Set default rotation when starting to follow
        Logger.Information("Started following player {PlayerId}", playerId);
    }

    /// <summary>
    /// Stops following the current player
    /// </summary>
    public void StopFollowingPlayer() {
        IsFollowingPlayer = false;
        FollowedPlayerId = null;
        HasManuallyStopped = true; // Remember that user manually stopped
        Logger.Information("Stopped following player");
    }

    /// <summary>
    /// Unlocks camera follow when manual movement is detected
    /// </summary>
    private void UnlockCameraFollow() {
        if (IsFollowingPlayer) {
            IsFollowingPlayer = false;
            Logger.Information("Camera follow unlocked - manual movement detected");
        }
    }

    /// <summary>
    /// Updates camera to follow a player at the specified position
    /// </summary>
    public void UpdatePlayerFollow(Vector3 playerPosition) {
        if (!IsFollowingPlayer) return;

        SetCameraTarget(playerPosition);
        SetCameraPosition(playerPosition + CameraFollowOffset);
        SetDefaultRotation(); // Always use default rotation when following player
    }

    /// <summary>
    /// Toggles wireframe rendering mode
    /// </summary>
    public void ToggleWireframeMode() {
        WireframeMode = !WireframeMode;
        Logger.Information("Wireframe mode: {Mode}", WireframeMode ? "ON" : "OFF");
    }

    /// <summary>
    /// Sets camera orientation that works well with MapleStory 2's coordinate system
    /// </summary>
    public void SetCameraOrientationForMapData() {
        // Set camera orientation that works well with the transformed map coordinate system
        Vector3 offset = new Vector3(0, -5, 5); // Look up from below and in front (inverted from typical)
        SetCameraPosition(CameraTarget + offset);
    }

    /// <summary>
    /// Flips the camera's up vector to fix upside-down view
    /// </summary>
    public void FlipCameraUpVector() {
        if (Camera == null) return;

        // Flip the up axis in the transform
        Matrix4x4 currentTransform = Camera.Transform.Transformation;
        Matrix4x4 flipMatrix = Matrix4x4.CreateScale(1, 1, -1);
        Camera.Transform.Transformation = currentTransform * flipMatrix;
        UpdateViewMatrix();
    }
}
