using System.Numerics;

namespace Maple2.Server.DebugGame.Graphics.Scene;

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
    /// Updates the controller with the given delta time
    /// </summary>
    /// <param name="delta">Time elapsed since last update</param>
    void Update(float delta);
}
