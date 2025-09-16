using System.Numerics;
using Serilog;

namespace Maple2.Server.DebugGame.Graphics.Scene;

public class FollowCameraController : ICameraController {
    private static readonly ILogger Logger = Log.Logger.ForContext<FollowCameraController>();

    public Camera Camera { get; }
    public InputState InputState { get; } = new();

    // Camera follow system
    public bool IsFollowingPlayer { get; private set; }
    public long? FollowedPlayerId { get; private set; }

    // Default camera follow offset
    private static readonly Vector3 CameraFollowOffset = new Vector3(-800, -1000, 1200);

    public Vector3 CameraTarget { get; private set; } = Vector3.Zero;

    public FollowCameraController(Camera camera) {
        Camera = camera;
    }

    /// <summary>
    /// Starts following a specific player
    /// </summary>
    public void StartFollowingPlayer(long playerId, Vector3 playerPosition) {
        IsFollowingPlayer = true;
        FollowedPlayerId = playerId;

        // Set initial camera position and look at player only once
        Camera.Transform.Position = playerPosition + CameraFollowOffset;
        SetCameraTarget(playerPosition);

        Logger.Information("Started following player {PlayerId}", playerId);
    }

    /// <summary>
    /// Stops following the current player
    /// </summary>
    public void StopFollowingPlayer() {
        IsFollowingPlayer = false;
        FollowedPlayerId = null;
        Logger.Information("Stopped following player");
    }

    /// <summary>
    /// Updates camera to follow a player at the specified position
    /// </summary>
    public void UpdatePlayerFollow(Vector3 playerPosition) {
        if (!IsFollowingPlayer) return;

        // Only update camera position, keep rotation fixed from initial setup
        Camera.Transform.Position = playerPosition + CameraFollowOffset;
    }

    /// <summary>
    /// Sets the camera target and updates camera orientation
    /// </summary>
    public void SetCameraTarget(Vector3 target) {
        CameraTarget = target;
        Vector3 direction = Vector3.Normalize(target - Camera.Transform.Position);

        // Use Transform's LookTo method instead of manual matrix calculation
        Camera.Transform.LookTo(direction, Vector3.UnitZ, snapToGroundPlane: false);
    }

    /// <summary>
    /// Updates the controller - follow controllers don't need delta-based updates
    /// </summary>
    public void Update(float delta) {
        // Follow controller doesn't need frame-based updates
        // Following is updated via UpdatePlayerFollow when player position changes
    }
}
