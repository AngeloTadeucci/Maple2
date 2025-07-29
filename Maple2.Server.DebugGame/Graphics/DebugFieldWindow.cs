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

    private void OnClose() { }

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

        // Update camera input state and let FreeCameraController handle input
        UpdateCameraInputState();
        Context.CameraController.Update((float) delta);
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

    private void UnloadField(DebugFieldRenderer renderer) { }

    private void LoadField(DebugFieldRenderer renderer) { }

    private void UpdateCameraInputState() {
        if (Input == null) return;

        InputState inputState = Context.CameraController.InputState;
        IKeyboard keyboard = Input.Keyboards[0];
        IMouse mouse = Input.Mice[0];

        // Update InputFocused - camera should be active when window is focused
        inputState.InputFocused = true;

        // Update keyboard state for specific keys we care about
        Key[] keysToTrack = {
            Key.W, Key.A, Key.S, Key.D, Key.Q, Key.E,
            Key.ShiftLeft, Key.ShiftRight, Key.ControlLeft, Key.ControlRight,
            Key.Space, Key.Escape, Key.Tab, Key.Enter,
            Key.Up, Key.Down, Key.Left, Key.Right
        };

        foreach (Key key in keysToTrack) {
            int keyIndex = (int) key;
            if (keyIndex >= 0 && keyIndex < inputState.KeyStates.Length) {
                inputState.KeyStates[keyIndex].LastInput = inputState.KeyStates[keyIndex].IsDown;
                inputState.KeyStates[keyIndex].IsDown = keyboard.IsKeyPressed(key);
            }
        }

        // Update mouse button states
        inputState.MouseLeft.LastInput = inputState.MouseLeft.IsDown;
        inputState.MouseLeft.IsDown = mouse.IsButtonPressed(MouseButton.Left);

        inputState.MouseMiddle.LastInput = inputState.MouseMiddle.IsDown;
        inputState.MouseMiddle.IsDown = mouse.IsButtonPressed(MouseButton.Middle);

        inputState.MouseRight.LastInput = inputState.MouseRight.IsDown;
        inputState.MouseRight.IsDown = mouse.IsButtonPressed(MouseButton.Right);

        // Update mouse position
        inputState.MousePosition.LastPosition = inputState.MousePosition.Position;
        inputState.MousePosition.Position = new Vector3(mouse.Position.X, mouse.Position.Y, 0);

        // Update mouse wheel
        inputState.MouseWheel.LastPosition = inputState.MouseWheel.Position;
        if (mouse.ScrollWheels.Count > 0) {
            inputState.MouseWheel.Position = new Vector3(mouse.ScrollWheels[0].X, mouse.ScrollWheels[0].Y, 0);
        }
    }
}
