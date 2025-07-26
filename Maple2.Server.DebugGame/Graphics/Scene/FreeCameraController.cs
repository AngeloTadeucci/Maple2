using Silk.NET.Input;
using System.Numerics;

namespace Maple2.Server.DebugGame.Graphics.Scene;

public class FreeCameraController {
    const float MAX_PITCH = (float.Pi / 180) * 89;

    public InputState? InputState;
    public Camera? Camera;

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

    public void Update(float delta) {
        if (InputState is null || Camera is null || !InputState.InputFocused) {
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

        Camera.Transform.Position += delta * FlySpeed * (moveDirection.X * Camera.Transform.RightAxis + moveDirection.Y * Camera.Transform.FrontAxis + moveDirection.Z * Camera.Transform.UpAxis);

        if (!InputState.MouseRight.IsDown) {
            return;
        }

        float currentPitch = -float.Asin(Camera.Transform.FrontAxis.Z);
        float currentYaw = -float.Atan2(-Camera.Transform.FrontAxis.X, -Camera.Transform.FrontAxis.Y);

        Vector3 mouseDelta = InputState.MousePosition.Delta;

        currentPitch = float.Max(-MAX_PITCH, float.Min(MAX_PITCH, currentPitch + RotationSpeed * BaseRotationSpeed * mouseDelta.Y));
        currentYaw -= RotationSpeed * BaseRotationSpeed * mouseDelta.X;

        Matrix4x4 rotation = Matrix4x4.Multiply(Matrix4x4.CreateRotationX(currentPitch), Matrix4x4.CreateRotationZ(currentYaw));
        Vector3 position = Camera.Transform.Position;

        Camera.Transform.Transformation = rotation;
        Camera.Transform.Position = position;

        InputState.MousePosition.Lock = true;
    }
}
