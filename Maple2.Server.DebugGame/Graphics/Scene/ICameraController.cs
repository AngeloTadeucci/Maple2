using System.Numerics;

namespace Maple2.Server.DebugGame.Graphics.Scene;

/// <summary>
/// Interface for camera controllers that can manipulate a camera
/// </summary>
public interface ICameraController {
    /// <summary>
    /// The camera being controlled
    /// </summary>
    Camera Camera { get; }

    /// <summary>
    /// Input state for handling user input
    /// </summary>
    InputState InputState { get; }

    /// <summary>
    /// Whether wireframe rendering mode is enabled
    /// </summary>
    bool WireframeMode { get; set; }

    /// <summary>
    /// Current camera position
    /// </summary>
    Vector3 CameraPosition { get; }

    /// <summary>
    /// Current camera rotation
    /// </summary>
    Quaternion CameraRotation { get; }

    /// <summary>
    /// Current camera target position
    /// </summary>
    Vector3 CameraTarget { get; }

    /// <summary>
    /// Whether the controller is currently following a player
    /// </summary>
    bool IsFollowingPlayer { get; }

    /// <summary>
    /// ID of the player being followed (if any)
    /// </summary>
    long? FollowedPlayerId { get; }

    /// <summary>
    /// Whether the user has manually stopped following
    /// </summary>
    bool HasManuallyStopped { get; }

    /// <summary>
    /// Updates the controller with the given delta time
    /// </summary>
    /// <param name="delta">Time elapsed since last update</param>
    void Update(float delta);

    /// <summary>
    /// Sets the camera to the default rotation for MapleStory 2
    /// </summary>
    void SetDefaultRotation();

    /// <summary>
    /// Sets the camera to the field overview rotation
    /// </summary>
    void SetFieldOverviewRotation();

    /// <summary>
    /// Sets the camera target position
    /// </summary>
    /// <param name="target">Target position</param>
    void SetCameraTarget(Vector3 target);

    /// <summary>
    /// Rotates the camera around the target point
    /// </summary>
    /// <param name="yawDelta">Yaw rotation delta</param>
    /// <param name="pitchDelta">Pitch rotation delta</param>
    void RotateCamera(float yawDelta, float pitchDelta);

    /// <summary>
    /// Moves the camera relative to its current position
    /// </summary>
    /// <param name="offset">Position offset</param>
    void MoveCameraRelative(Vector3 offset);

    /// <summary>
    /// Starts following a specific player
    /// </summary>
    /// <param name="playerId">ID of the player to follow</param>
    void StartFollowingPlayer(long playerId);

    /// <summary>
    /// Stops following the current player
    /// </summary>
    void StopFollowingPlayer();

    /// <summary>
    /// Updates camera to follow a player at the specified position
    /// </summary>
    /// <param name="playerPosition">Player position</param>
    void UpdatePlayerFollow(Vector3 playerPosition);

    /// <summary>
    /// Toggles wireframe rendering mode
    /// </summary>
    void ToggleWireframeMode();

    /// <summary>
    /// Sets camera orientation that works well with MapleStory 2's coordinate system
    /// </summary>
    void SetCameraOrientationForMapData();

    /// <summary>
    /// Flips the camera's up vector to fix upside-down view
    /// </summary>
    void FlipCameraUpVector();
}
