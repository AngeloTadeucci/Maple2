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
    public bool IsClosing {
        get {
            fieldWindowsMutex.WaitOne();
            bool isClosing = shouldClose;
            fieldWindowsMutex.ReleaseMutex();

            return isClosing;
        }
    }
    private bool shouldClose = false;
    private bool shouldForceClose = false;
    private bool shouldResize = false;
    private bool isCurrentlyRendering = false;
    private bool initialized = false;
    private Vector2D<int> newSize = default;
    private Mutex fieldWindowsMutex = new();
    private int updateTimeoutCount = 0;

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
            if (DebuggerWindow is not null) {
                CleanUp();
            }
        }

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
        //DebuggerWindow.Render += OnRender;
        DebuggerWindow.Update += OnWindowUpdate;
        DebuggerWindow.Load += OnLoad;
        DebuggerWindow.Closing += OnClose;

        Log.Information("Creating field renderer window");

        new Thread(DebuggerWindow.Run).Start();
    }

    public void CleanUp() {
        initialized = false;

        unsafe {
            if (DxSwapChain.Handle is not null) {
                DxSwapChain.Dispose();
                Input?.Dispose();

                DxSwapChain = default;
                Input = default;

                Log.Information("Field debugger swap chain cleaning up");
            }
        }

        try {
            if (DebuggerWindow is not null) {
                DebuggerWindow.Dispose();

                DebuggerWindow = default;

                Log.Information("Field debugger window cleaning up");
            }

            Context.FieldWindowClosed(this);
        }
        catch (InvalidOperationException ex) {
            // hit a weird race condition: 'You cannot call `Reset` inside of the render loop!'; try again later
        }

    }

    private void OnClose() {
        fieldWindowsMutex.WaitOne();

        shouldClose = true;

        fieldWindowsMutex.ReleaseMutex();
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

        ImGuiController = new ImGuiController(Context, Input);

        ImGuiController.Initialize(DebuggerWindow);

        initialized = true;
    }

    public void OnWindowUpdate(double delta) {
        fieldWindowsMutex.WaitOne();
        if (shouldForceClose) {
            DebuggerWindow!.Close();
            CleanUp();
        }
        fieldWindowsMutex.ReleaseMutex();
    }

    public unsafe void OnUpdate(double delta) {
        fieldWindowsMutex.WaitOne();
        if (shouldClose && !isCurrentlyRendering) {
            // Preventing window closing from happening at the same time as a render call is difficult, so only close after 3 updates with no render call
            if (updateTimeoutCount > 3) {
                DebuggerWindow!.Close();
                CleanUp();
            }

            updateTimeoutCount++;
        }
        fieldWindowsMutex.ReleaseMutex();

        if (!initialized) {
            return;
        }

        if (ActiveRenderer is not null) {
            ActiveRenderer.Update();
            DebuggerWindow!.Title = $"Maple2 - {WindowName}: {ActiveRenderer.Field.Metadata.Name} [{ActiveRenderer.Field.MapId}] ({ActiveRenderer.Field.InstanceId})";
        }
    }

    public unsafe void OnRender(double delta) {
        if (!initialized) {
            return;
        }

        fieldWindowsMutex.WaitOne();

        if (shouldClose) {
            fieldWindowsMutex.ReleaseMutex();

            return;
        }

        updateTimeoutCount = 0;
        isCurrentlyRendering = true;

        fieldWindowsMutex.ReleaseMutex();

        if (shouldResize) {
            // there is currently a bug with resizing where the framebuffer positioning doesn't take into account title bar size
            SilkMarshal.ThrowHResult(DxSwapChain.ResizeBuffers(0, (uint) newSize.X, (uint) newSize.Y, Format.FormatB8G8R8A8Unorm, 0));
        }

        shouldResize = false;

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

        RenderMapInfoWindow();

        // End region for render code
        #endregion

        ImGuiController!.EndFrame();

        DxSwapChain.Present(1, 0);

        renderTargetView.Dispose();
        framebuffer.Dispose();

        fieldWindowsMutex.WaitOne();

        isCurrentlyRendering = false;

        fieldWindowsMutex.ReleaseMutex();
    }
    private unsafe void OnFramebufferResize(Vector2D<int> newSize) {
        shouldResize = true;
        this.newSize = newSize;
    }

    public void ForceClose() {
        fieldWindowsMutex.WaitOne();

        shouldForceClose = true;

        fieldWindowsMutex.ReleaseMutex();
    }

    public void Close() {
        DebuggerWindow!.Close();
    }

    private void UnloadField(DebugFieldRenderer renderer) {

    }

    private void LoadField(DebugFieldRenderer renderer) {

    }

    private void RenderMapInfoWindow() {
        ImGui.Begin("Field properties");

        if (ImGui.BeginTable("Map properties", 2)) {
            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Property");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text("Value");

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Map Name");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text(ActiveRenderer?.Field.Metadata.Name ?? "");

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Map Id");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text(ActiveRenderer?.Field.MapId.ToString() ?? "");

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Instance Id");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text(ActiveRenderer?.Field.InstanceId.ToString() ?? "");

            ImGui.EndTable();
        }

        if (ImGui.BeginTable("Players", 1)) {
            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Player Name");

            if (ActiveRenderer is not null) {
                foreach ((int id, FieldPlayer player) in ActiveRenderer.Field.GetPlayers()) {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(player.Value.Character.Name);
                }
            }

            ImGui.EndTable();
        }

        if (ImGui.BeginTable("Npcs", 3)) {
            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

            ImGui.TableSetColumnIndex(0);
            ImGui.Text("Npc Name");
            ImGui.TableSetColumnIndex(1);
            ImGui.Text("Npc Id");
            ImGui.TableSetColumnIndex(2);
            ImGui.Text("Npc Level");
            //ImGui.TableSetColumnIndex(3);
            //ImGui.Text("Npc Type");

            if (ActiveRenderer is not null) {
                foreach (FieldNpc npc in ActiveRenderer.Field.EnumerateNpcs()) {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(npc.Value.Metadata.Name);
                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(npc.Value.Id.ToString());
                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text(npc.Value.Metadata.Basic.Level.ToString());
                }
            }

            ImGui.EndTable();
        }

        ImGui.End();
    }
}
