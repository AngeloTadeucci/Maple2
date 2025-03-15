using ImGuiNET;
using Maple2.Server.DebugGame.Graphics.Assets;
using Maple2.Server.DebugGame.Graphics.Resources;
using Maple2.Server.Game.Model;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;
using static Community.CsharpSqlite.Sqlite3;
using static Maple2.Server.Game.Manager.Field.FieldManager;

namespace Maple2.Server.DebugGame.Graphics;

public class DebugFieldWindow {
    public DebugGraphicsContext Context { get; init; }
    public DebugFieldRenderer? ActiveRenderer { get; private set; }
    public IWindow? DebuggerWindow { get; private set; }
    public IInputContext? Input { get; private set; }
    public ComPtr<IDXGISwapChain1> DxSwapChain { get; private set; }
    public ImGuiController? ImGuiController { get; private set; }
    public int WindowId { get; init; }
    public string WindowName { get; init; }
    public bool IsInitialized { get; private set; }
    public bool IsClosing { get; private set; }

    private static int _windowIdCounter = 0;

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
        unsafe {
            if (IsInitialized) {
                CleanUp();
            }
        }

        IsInitialized = true;

        var windowOptions = WindowOptions.Default;
        windowOptions.Size = DebugGraphicsContext.DefaultWindowSize;
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
                Input = default;

                Log.Information("Field debugger swap chain cleaning up");
            }
        }

        if (DebuggerWindow is not null) {
            DebuggerWindow.Dispose();

            DebuggerWindow = default;

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
                quality: 0) // no antialiasing
        };

        unsafe {
            IDXGISwapChain1* swapChain = default;

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

    public unsafe void OnUpdate(double delta) {
        if (IsClosing) {
            return;
        }

        if (ActiveRenderer is not null) {
            ActiveRenderer.Update();
            DebuggerWindow!.Title = $"Maple2 - {WindowName}: {ActiveRenderer.Field.Metadata.Name} [{ActiveRenderer.Field.MapId}] ({ActiveRenderer.Field.RoomId})";
        }
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
        Context.DxDeviceContext.RSSetViewports(1, ref viewport);

        Context.DxDeviceContext.OMSetRenderTargets(1, ref renderTargetView.Handle, (ID3D11DepthStencilView*) null);

        ImGuiController!.BeginFrame((float) delta);

        #region Render code
        // Begin region for render code

        Context.VertexShader!.Bind();
        Context.PixelShader!.Bind();
        Context.SampleTexture!.Bind();
        Context.CoreModels!.Quad.Draw();

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

    private unsafe void OnFramebufferResize(Vector2D<int> newSize) {
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
}
