using System.Numerics;
using Serilog;

namespace Maple2.Server.DebugGame.Graphics.Scene;

public class FollowCameraController : ICameraController {
    private static readonly ILogger Logger = Log.Logger.ForContext<FollowCameraController>();

    public Camera Camera { get; }
    public InputState InputState { get; } = new();

    // Rendering modes
    public bool WireframeMode { get; set; } = true;

    // Camera follow system
    public bool IsFollowingPlayer { get; private set; }
    public long? FollowedPlayerId { get; private set; }
    public bool HasManuallyStopped { get; private set; }

    // Default camera follow offset
    private static readonly Vector3 CameraFollowOffset = new Vector3(-800, -1000, 1200);

    // Default camera rotation for MapleStory 2 coordinate system
    private static readonly Quaternion DefaultCameraRotation = new Quaternion(0.130f, 0.325f, 0.870f, 0.350f);

    // Field overview camera rotation
    private static readonly Quaternion FieldOverviewRotation = new Quaternion(0.000f, 0.450f, 0.900f, 0.000f);

    public Vector3 CameraTarget { get; private set; } = Vector3.Zero;

    // Camera properties that delegate to Camera.Transform
    public Vector3 CameraPosition => Camera.Transform.Position;
    public Quaternion CameraRotation => Camera.Transform.Quaternion;

    public FollowCameraController(Camera camera) {
        Camera = camera;
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
    public void UnlockCameraFollow() {
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
        Camera.Transform.Position = playerPosition + CameraFollowOffset;
        SetDefaultRotation(); // Always use default rotation when following player
    }

    /// <summary>
    /// Sets the camera target and updates camera orientation
    /// </summary>
    public void SetCameraTarget(Vector3 target) {
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
    }

    /// <summary>
    /// Sets the camera to the default rotation for MapleStory 2
    /// </summary>
    public void SetDefaultRotation() {
        Camera.Transform.Quaternion = DefaultCameraRotation;
    }

    /// <summary>
    /// Moves the camera target relative to its current position
    /// </summary>
    public void MoveCameraTargetRelative(Vector3 offset) {
        CameraTarget += offset;
    }

    // ICameraController interface methods that are not follow-specific
    // These provide minimal implementations since this controller focuses on following

    /// <summary>
    /// Updates the controller - follow controllers don't need delta-based updates
    /// </summary>
    public void Update(float delta) {
        // Follow controller doesn't need frame-based updates
        // Following is updated via UpdatePlayerFollow when player position changes
    }

    /// <summary>
    /// Sets the camera to the field overview rotation
    /// </summary>
    public void SetFieldOverviewRotation() {
        Camera.Transform.Quaternion = FieldOverviewRotation;
    }

    /// <summary>
    /// Rotates the camera around the target point - minimal implementation for follow controller
    /// </summary>
    public void RotateCamera(float yawDelta, float pitchDelta) {
        // Follow controller maintains fixed camera orientation relative to player
        // Manual rotation would break following, so this is intentionally minimal
    }

    /// <summary>
    /// Moves the camera relative to its current position - minimal implementation for follow controller
    /// </summary>
    public void MoveCameraRelative(Vector3 offset) {
        // Follow controller maintains fixed camera position relative to player
        // Manual movement would break following, so this is intentionally minimal
    }

    /// <summary>
    /// Toggles wireframe rendering mode
    /// </summary>
    public void ToggleWireframeMode() {
        WireframeMode = !WireframeMode;
    }

    /// <summary>
    /// Sets camera orientation that works well with MapleStory 2's coordinate system
    /// </summary>
    public void SetCameraOrientationForMapData() {
        // Set camera orientation that works well with the transformed map coordinate system
        Vector3 offset = new Vector3(0, -5, 5); // Look up from below and in front (inverted from typical)
        Camera.Transform.Position = CameraTarget + offset;
        SetDefaultRotation();
    }

    /// <summary>
    /// Flips the camera's up vector to fix upside-down view
    /// </summary>
    public void FlipCameraUpVector() {
        // Flip the up axis in the transform
        Matrix4x4 currentTransform = Camera.Transform.Transformation;

        // Create a flip matrix that inverts the Y axis
        Matrix4x4 flipMatrix = Matrix4x4.CreateScale(1, -1, 1);

        Camera.Transform.Transformation = currentTransform * flipMatrix;
    }
}
