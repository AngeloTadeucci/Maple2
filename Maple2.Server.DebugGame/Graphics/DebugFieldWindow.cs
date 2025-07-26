using System.Numerics;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Maple2.Server.DebugGame.Graphics;

public class DebugFieldWindow {
    private DebugGraphicsContext Context { get; }
    public DebugFieldRenderer? ActiveRenderer { get; private set; }
    public IWindow? DebuggerWindow { get; private set; }
    private IInputContext? Input { get; set; }
    private ComPtr<IDXGISwapChain1> DxSwapChain { get; set; }
    private ImGuiController? ImGuiController { get; set; }
    private int WindowId { get; }
    public string WindowName { get; }
    public bool IsInitialized { get; private set; }
    public bool IsClosing { get; private set; }

    private static int _windowIdCounter;

    // Camera input tracking
    private Vector2 lastMousePosition;

    public DebugFieldWindow(DebugGraphicsContext context) {
        Context = context;
        WindowId = _windowIdCounter++;
        WindowName = $"Window {WindowId}";
    }

    public void SetActiveRenderer(DebugFieldRenderer? renderer) {
        if (ActiveRenderer is not null) {
            ActiveRenderer.DetachWindow(this);
            UnloadField(ActiveRenderer);
        }

        ActiveRenderer = renderer;
        ActiveRenderer?.AttachWindow(this);

        if (ActiveRenderer is not null) {
            LoadField(ActiveRenderer);
        }
    }

    public void Initialize() {
        if (IsInitialized) {
            CleanUp();
        }

        IsInitialized = true;

        var windowOptions = WindowOptions.Default;
        windowOptions.Size = DebugGraphicsContext.DefaultFieldWindowSize;
        windowOptions.Title = $"Maple2 - {WindowName}";
        windowOptions.API = GraphicsAPI.None;
        windowOptions.FramesPerSecond = 60;
        windowOptions.ShouldSwapAutomatically = false;
        windowOptions.IsContextControlDisabled = true;
        windowOptions.UpdatesPerSecond = 60;
        windowOptions.IsEventDriven = false;

        DebuggerWindow = Window.Create(windowOptions);

        DebuggerWindow.FramebufferResize += OnFramebufferResize;
        DebuggerWindow.Render += OnRender;
        DebuggerWindow.Update += OnUpdate;
        DebuggerWindow.Load += OnLoad;
        DebuggerWindow.Closing += OnClose;

        Log.Information("Creating field renderer window");

        DebuggerWindow.Initialize();
    }

    public void CleanUp() {
        unsafe {
            if (DxSwapChain.Handle is not null) {
                DxSwapChain.Dispose();
                Input?.Dispose();

                DxSwapChain = default;
                Input = null;

                Log.Information("Field debugger swap chain cleaning up");
            }
        }

        if (DebuggerWindow is not null) {
            DebuggerWindow.Dispose();

            DebuggerWindow = null;

            Log.Information("Field debugger window cleaning up");
        }

        Context.FieldWindowClosed(this);

    }

    private void OnClose() {

    }

    private void OnLoad() {
        Input = DebuggerWindow!.CreateInput();

        var swapChainDescription = new SwapChainDesc1 {
            BufferCount = 2, // double buffered
            Format = Format.FormatB8G8R8A8Unorm, // 32 bit RGBA format
            BufferUsage = DXGI.UsageRenderTargetOutput,
            SwapEffect = SwapEffect.FlipDiscard, // don't keep old output from previous frames
            SampleDesc = new SampleDesc(
                count: 1, // 1 buffer sample per pixel (AA needs more)
                quality: 0), // no antialiasing
        };

        unsafe {
            IDXGISwapChain1* swapChain = null;

            SilkMarshal.ThrowHResult(Context.DxFactory.CreateSwapChainForHwnd(
                pDevice: (IUnknown*) (ID3D11Device*) Context.DxDevice,
                hWnd: DebuggerWindow!.Native!.DXHandle!.Value,
                pDesc: &swapChainDescription,
                pFullscreenDesc: (SwapChainFullscreenDesc*) null,
                pRestrictToOutput: (IDXGIOutput*) null,
                ppSwapChain: &swapChain));

            DxSwapChain = swapChain;
        }

        Log.Information("Swap chain initialized");

        ImGuiController = new ImGuiController(Context, Input, this);

        ImGuiController.Initialize(DebuggerWindow);
    }

    public void OnUpdate(double delta) {
        if (IsClosing) {
            return;
        }

        if (ActiveRenderer is not null) {
            ActiveRenderer.Update();
            DebuggerWindow!.Title = $"Maple2 - {WindowName}: {ActiveRenderer.Field.Metadata.Name} [{ActiveRenderer.Field.MapId}] ({ActiveRenderer.Field.RoomId})";
        }

        // Handle camera input for this field window
        HandleCameraInput((float) delta);
    }

    public unsafe void OnRender(double delta) {
        if (IsClosing) {
            return;
        }

        DebuggerWindow!.MakeCurrent();

        ComPtr<ID3D11Texture2D> framebuffer = DxSwapChain.GetBuffer<ID3D11Texture2D>(0);

        ComPtr<ID3D11RenderTargetView> renderTargetView = default;
        SilkMarshal.ThrowHResult(Context.DxDevice.CreateRenderTargetView(framebuffer, null, ref renderTargetView));

        Context.DxDeviceContext.ClearRenderTargetView(renderTargetView, DebugGraphicsContext.WindowClearColor);

        Viewport viewport = new Viewport(0, 0, DebuggerWindow!.FramebufferSize.X, DebuggerWindow!.FramebufferSize.Y, 0, 1);
        Context.DxDeviceContext.RSSetViewports(1, in viewport);

        Context.DxDeviceContext.OMSetRenderTargets(1, in renderTargetView.Handle, (ID3D11DepthStencilView*) null);

        ImGuiController!.BeginFrame((float) delta);

        #region Render code
        // Begin region for render code

        // Render 3D field content first (before ImGui)
        if (ActiveRenderer is not null) {
            ActiveRenderer.RenderFieldWindow3D(delta);
        }

        // Then render ImGui content
        if (ActiveRenderer is not null) {
            ActiveRenderer.Render(delta);
        }

        // End region for render code
        #endregion

        ImGuiController!.EndFrame();

        DxSwapChain.Present(1, 0);

        renderTargetView.Dispose();
        framebuffer.Dispose();

    }

    private void OnFramebufferResize(Vector2D<int> newSize) {
        // there is currently a bug with resizing where the framebuffer positioning doesn't take into account title bar size
        SilkMarshal.ThrowHResult(DxSwapChain.ResizeBuffers(0, (uint) newSize.X, (uint) newSize.Y, Format.FormatB8G8R8A8Unorm, 0));
    }

    public void Close() {
        IsClosing = true;
    }

    private void UnloadField(DebugFieldRenderer renderer) {

    }

    private void LoadField(DebugFieldRenderer renderer) {

    }

    private void HandleCameraInput(float deltaTime) {
        if (Input == null) return;

        IKeyboard keyboard = Input.Keyboards[0];
        IMouse mouse = Input.Mice[0];

        float moveSpeed = 500.0f * deltaTime; // Units per second
        float rotateSpeed = 2.0f * deltaTime; // Radians per second

        // Faster movement when holding Shift
        if (keyboard.IsKeyPressed(Key.ShiftLeft) || keyboard.IsKeyPressed(Key.ShiftRight)) {
            moveSpeed *= 5.0f; // 5x faster with Shift
        }

        // Calculate camera directions from quaternion rotation
        Vector3 forward = Vector3.Transform(Vector3.UnitX, Context.CameraRotation); // X is forward in our coordinate system
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitZ)); // Use Z as world up for transformed coords
        Vector3 up = Vector3.UnitZ; // Use Z as up direction for Q/E movement

        // WASD movement
        Vector3 movement = Vector3.Zero;
        if (keyboard.IsKeyPressed(Key.W)) movement += forward;
        if (keyboard.IsKeyPressed(Key.S)) movement -= forward;
        if (keyboard.IsKeyPressed(Key.A)) movement -= right;
        if (keyboard.IsKeyPressed(Key.D)) movement += right;
        if (keyboard.IsKeyPressed(Key.Q)) movement += up;
        if (keyboard.IsKeyPressed(Key.E)) movement -= up;

        // Arrow keys for camera rotation (quaternion-based)
        bool rotated = false;
        Quaternion rotationDelta = Quaternion.Identity;

        if (keyboard.IsKeyPressed(Key.Left)) {
            rotationDelta *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, rotateSpeed); // Yaw left
            rotated = true;
        }
        if (keyboard.IsKeyPressed(Key.Right)) {
            rotationDelta *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -rotateSpeed); // Yaw right
            rotated = true;
        }
        if (keyboard.IsKeyPressed(Key.Up)) {
            rotationDelta *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, -rotateSpeed); // Pitch up (inverted)
            rotated = true;
        }
        if (keyboard.IsKeyPressed(Key.Down)) {
            rotationDelta *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, rotateSpeed); // Pitch down (inverted)
            rotated = true;
        }

        if (rotated) {
            // Apply rotation delta to camera
            Context.CameraRotation = rotationDelta * Context.CameraRotation;
            Context.CameraRotation = Quaternion.Normalize(Context.CameraRotation);
            Context.UpdateViewMatrix();
        }

        if (movement != Vector3.Zero) {
            movement = Vector3.Normalize(movement) * moveSpeed;
            // Move camera position directly (free camera style)
            Context.CameraPosition += movement;
            Context.UpdateViewMatrix();

            // Unlock camera follow when manually moving
            if (Context.IsFollowingPlayer && movement.LengthSquared() > 0.01f) {
                Context.StopFollowingPlayer();
            }
        }

        // Mouse look (only when right mouse button is held)
        if (mouse.IsButtonPressed(MouseButton.Right)) {
            Vector2 mouseDelta = mouse.Position - lastMousePosition;

            if (mouseDelta.X != 0 || mouseDelta.Y != 0) {
                float sensitivity = 0.002f; // Mouse sensitivity

                // Create rotation quaternions for yaw and pitch
                Quaternion yawRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -mouseDelta.X * sensitivity); // Yaw around Z-axis
                Quaternion pitchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, mouseDelta.Y * sensitivity); // Pitch around Y-axis (inverted)

                // Apply rotations: first yaw (world space), then pitch (local space)
                Context.CameraRotation = yawRotation * Context.CameraRotation * pitchRotation;

                // Normalize to prevent drift
                Context.CameraRotation = Quaternion.Normalize(Context.CameraRotation);

                Context.UpdateViewMatrix();
            }
        }

        // Mouse wheel movement (with same modifiers as WASD)
        if (mouse is { ScrollWheels.Count: > 0 }) {
            float scroll = mouse.ScrollWheels[0].Y;
            if (scroll != 0) {
                // Use same speed modifiers as WASD movement
                float wheelMoveSpeed = moveSpeed * scroll;

                // Move forward/backward along camera's forward direction (like W/S)
                Vector3 wheelMovement = forward * wheelMoveSpeed;

                Context.CameraPosition += wheelMovement;
                Context.UpdateViewMatrix();

                // Unlock camera follow when manually moving
                if (Context.IsFollowingPlayer && wheelMovement.LengthSquared() > 0.01f) {
                    Context.StopFollowingPlayer();
                }
            }

            lastMousePosition = mouse.Position;
        }
    }
}
