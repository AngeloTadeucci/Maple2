using Maple2.Server.Game.DebugGraphics;
using Maple2.Server.Game.Manager.Field;
using Maple2.Server.DebugGame.Graphics.Resources;
using Serilog;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Maple2.Server.DebugGame.Graphics.Assets;
using Maple2.Server.DebugGame.Graphics.Scene;
using Maple2.Tools;
using Maple2.Tools.Extensions;
using System.Numerics;
using Maple2.Server.Game.Model;

namespace Maple2.Server.DebugGame.Graphics;

public class DebugGraphicsContext : IGraphicsContext {
    private const bool ForceDxvk = false;
    public static readonly Vector2D<int> DefaultWindowSize = new Vector2D<int>(800, 600);
    public static readonly Vector2D<int> DefaultFieldWindowSize = new Vector2D<int>(1920, 1080);
    public static readonly float[] WindowClearColor = [0.0f, 0.0f, 0.0f, 1.0f];
    public static readonly ILogger Logger = Log.Logger.ForContext<DebugGraphicsContext>();

    private readonly Dictionary<FieldManager, DebugFieldRenderer> fields = [];

    public IWindow? DebuggerWindow { get; set; }
    private IInputContext? Input { get; set; }

    private D3D11? D3d11 { get; set; }
    private DXGI? Dxgi { get; set; }
    public D3DCompiler? Compiler { get; private set; }

    public ComPtr<ID3D11Device> DxDevice { get; private set; }
    public ComPtr<ID3D11DeviceContext> DxDeviceContext { get; private set; }
    public ComPtr<IDXGIFactory2> DxFactory { get; private set; }
    private ComPtr<IDXGISwapChain1> DxSwapChain { get; set; }

    private string resourceRootPath = "";

    public ShaderPipeline? ScreenPipeline { get; private set; }
    public ShaderPipeline? DebugScenePipeline { get; private set; }
    public ShaderPipeline? WireframePipeline { get; private set; }
    public ShaderPipelines? ShaderPipelines { get; private set; }
    public SceneState SceneState { get; init; }

    // 3D rendering resources
    private ComPtr<ID3D11Texture2D> DepthStencilBuffer { get; set; }
    private ComPtr<ID3D11DepthStencilView> DepthStencilView { get; set; }
    private ComPtr<ID3D11DepthStencilState> DepthStencilState { get; set; }
    private ComPtr<ID3D11RasterizerState> SolidRasterizerState { get; set; }
    private ComPtr<ID3D11RasterizerState> WireframeRasterizerState { get; set; }
    // private ComPtr<ID3D11Buffer> ConstantBuffer { get; set; }

    public CoreModels? CoreModels { get; private set; }
    private Texture? sampleTexture;
    private ImGuiController? ImGuiController { get; set; }

    // Rendering state
    public bool WireframeMode { get; set; } = true; // Start in wireframe mode

    private readonly List<DebugFieldRenderer> fieldRenderers = [];
    private readonly Mutex fieldRendererMutex = new();
    public DebugFieldRenderer[] FieldRenderers {
        get {
            fieldRendererMutex.WaitOne();
            DebugFieldRenderer[] renderers = fieldRenderers.ToArray();
            fieldRendererMutex.ReleaseMutex();

            return renderers;
        }
    }
    public IReadOnlyList<DebugFieldWindow> FieldWindows => fieldWindows;
    private readonly List<DebugFieldWindow> fieldWindows = [];
    private readonly HashSet<FieldManager> updatedFields = [];
    private readonly object updatedFieldsLock = new();
    private int deltaIndex;
    private readonly List<int> deltaTimes = [];
    public int DeltaAverage { get; private set; }

    // Cleanup tracking
    private bool isCleanedUp;

    // Performance tracking
    public int DeltaMin { get; private set; }
    public int DeltaMax { get; private set; }
    private DateTime lastTime = DateTime.Now;
    private bool IsClosing { get; set; }

    public DebugGraphicsContext() {
        SceneState = new(this);
    }

    public void RunDebugger() {
        DebuggerWindow!.Initialize();

        bool subWindowsUpdating = false;

        while (!(DebuggerWindow?.IsClosing ?? true) || subWindowsUpdating) {
            if (DebuggerWindow is not null && !DebuggerWindow.IsClosing) {
                try {
                    UpdateWindow(DebuggerWindow, IsClosing);

                    if (DebuggerWindow is not null && DebuggerWindow.IsClosing) {
                        CleanUp();
                    }
                } catch (Exception ex) {
                    Logger.Warning(ex, "Exception during window update");
                    CleanUp();
                    break;
                }
            }

            DebugFieldWindow[] windows = fieldWindows.ToArray();

            subWindowsUpdating = false;

            foreach (DebugFieldWindow window in windows) {
                if (!window.IsInitialized) {
                    window.Initialize();
                }

                if (window.DebuggerWindow is not null) {
                    bool isStillOpen = UpdateWindow(window.DebuggerWindow, window.IsClosing);

                    subWindowsUpdating |= isStillOpen;

                    if (!isStillOpen) {
                        window.CleanUp();
                    }
                }
            }
        }
    }

    public bool UpdateWindow(IView window, bool shouldClose) {
        bool startedOpen = !window.IsClosing;

        if (shouldClose && !window.IsClosing) {
            window.Close();
        }

        if (!window.IsClosing) {
            window.DoEvents();
        }

        if (!window.IsClosing) {
            window.DoUpdate();
        }

        if (!window.IsClosing) {
            window.DoRender();
        }

        bool hasUpdated = !window.IsClosing;

        if (startedOpen && window.IsClosing) {
            window.DoEvents();
            window.Reset();
        }

        return hasUpdated;
    }

    private string GetWindowName() {
        return $"Maple2 Visual Debugger";
    }

    public void Initialize() {
        if (DebuggerWindow is not null) {
            CleanUp();
        }

        var windowOptions = WindowOptions.Default;
        windowOptions.Size = DefaultWindowSize;
        windowOptions.Title = GetWindowName();
        windowOptions.API = GraphicsAPI.None;
        windowOptions.ShouldSwapAutomatically = false;

        DebuggerWindow = Window.Create(windowOptions);

        DebuggerWindow.FramebufferResize += OnFramebufferResize;
        DebuggerWindow.Render += OnRender;
        DebuggerWindow.Update += OnUpdate;
        DebuggerWindow.Load += OnLoad;
        DebuggerWindow.Closing += OnClose;

        Logger.Information("Creating window");

        new Thread(RunDebugger).Start();
    }

    public static unsafe void DxLog(Message message) {
        if (message.PDescription is null) {
            Logger.Error("Null DirectX error");

            return;
        }

        Logger.Error(SilkMarshal.PtrToString((nint) message.PDescription) ?? "Unknown DirectX error");
    }

    private void OnClose() {
        DebugFieldWindow[] windows = fieldWindows.ToArray();

        foreach (DebugFieldWindow window in windows) {
            window.Close();
        }

        IsClosing = true;
    }

    private void OnLoad() {
        Input = DebuggerWindow!.CreateInput();

        Dxgi = DXGI.GetApi(DebuggerWindow);
        D3d11 = D3D11.GetApi(DebuggerWindow);
        Compiler = D3DCompiler.GetApi();

        Texture.TextureRootPath = GetResourceRootPath("Textures");
        ShaderPipelines.AssetRootPath = GetResourceRootPath("");

        unsafe {
            ComPtr<ID3D11Device> device = default;
            ComPtr<ID3D11DeviceContext> deviceContext = default;

            SilkMarshal.ThrowHResult(D3d11.CreateDevice(
                pAdapter: default(ComPtr<IDXGIAdapter>),
                DriverType: D3DDriverType.Hardware,
                Software: 0,
                Flags: (uint) CreateDeviceFlag.Debug,
                pFeatureLevels: null,
                FeatureLevels: 0,
                SDKVersion: D3D11.SdkVersion,
                ppDevice: ref device,
                pFeatureLevel: null,
                ppImmediateContext: ref deviceContext));

            DxDevice = device;
            DxDeviceContext = deviceContext;

            if (OperatingSystem.IsWindows()) {
                // DirectX debug logging not supported for DXVK
                DxDevice.SetInfoQueueCallback(DxLog);
            }

            var swapChainDescription = new SwapChainDesc1 {
                BufferCount = 2, // double buffered
                Format = Format.FormatB8G8R8A8Unorm, // 32 bit RGBA format
                BufferUsage = DXGI.UsageRenderTargetOutput,
                SwapEffect = SwapEffect.FlipDiscard, // don't keep old output from previous frames
                SampleDesc = new SampleDesc(
                    count: 1, // 1 buffer sample per pixel (AA needs more)
                    quality: 0), // no antialiasing
            };

            // Factory1 adds DXGI 1.1 support & Factory2 adds DXGI 1.2 support
            DxFactory = Dxgi.CreateDXGIFactory<IDXGIFactory2>();

            IDXGISwapChain1* swapChain = null;

            SilkMarshal.ThrowHResult(DxFactory.CreateSwapChainForHwnd(
                pDevice: (IUnknown*) (ID3D11Device*) DxDevice,
                hWnd: DebuggerWindow!.Native!.DXHandle!.Value,
                pDesc: &swapChainDescription,
                pFullscreenDesc: (SwapChainFullscreenDesc*) null,
                pRestrictToOutput: (IDXGIOutput*) null,
                ppSwapChain: &swapChain));

            DxSwapChain = swapChain;
        }

        // Create 3D rendering resources
        Create3DRenderingResources();

        ShaderPipelines ??= new(this);

        LoadPipelines();

        CoreModels = new CoreModels(this);

        Logger.Information("Graphics context initialized");

        sampleTexture = new Texture(this);
        sampleTexture.Load(Path.Combine(Texture.TextureRootPath, "sample_derp_wave.png"));

        ImGuiController = new ImGuiController(this, Input, ImGuiWindowType.Main);

        if (DebuggerWindow is not null) {
            ImGuiController.Initialize(DebuggerWindow);
        }
    }

    private void LoadPipelines() {
        ShaderPipelines!.LoadPipelines();

        if (!ShaderPipelines!.Pipelines.TryGetValue("Screen", out ShaderPipeline? screenPipeline)) {
            ShaderPipelines!.ExternalError();

            Logger.Error("No 'Screen' shader pipeline defined");
        }

        if (!ShaderPipelines!.Pipelines.TryGetValue("DebugScene", out ShaderPipeline? debugScenePipeline)) {
            ShaderPipelines!.ExternalError();

            Logger.Error("No 'DebugScene' shader pipeline defined");
        }

        if (!ShaderPipelines!.Pipelines.TryGetValue("Wireframe", out ShaderPipeline? wireframePipeline)) {
            ShaderPipelines!.ExternalError();

            Logger.Error("No 'DebugScene' shader pipeline defined");
        }

        ScreenPipeline = screenPipeline;
        DebugScenePipeline = debugScenePipeline;
        WireframePipeline = wireframePipeline;
    }

    private unsafe void Create3DRenderingResources() {
        Vector2D<int> windowSize = DebuggerWindow!.FramebufferSize;

        // Create depth stencil buffer
        var depthBufferDesc = new Texture2DDesc {
            Width = (uint) windowSize.X,
            Height = (uint) windowSize.Y,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.FormatD24UnormS8Uint,
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint) BindFlag.DepthStencil,
            CPUAccessFlags = 0,
            MiscFlags = 0,
        };

        ID3D11Texture2D* depthStencilBuffer = null;
        SilkMarshal.ThrowHResult(DxDevice.CreateTexture2D(&depthBufferDesc, (SubresourceData*) null, &depthStencilBuffer));
        DepthStencilBuffer = depthStencilBuffer;

        // Create depth stencil view
        var depthStencilViewDesc = new DepthStencilViewDesc {
            Format = Format.FormatD24UnormS8Uint,
            ViewDimension = DsvDimension.Texture2D,
            Flags = 0,
        };
        depthStencilViewDesc.Anonymous.Texture2D.MipSlice = 0;

        ID3D11DepthStencilView* depthStencilView = null;
        SilkMarshal.ThrowHResult(DxDevice.CreateDepthStencilView((ID3D11Resource*) DepthStencilBuffer.Handle, &depthStencilViewDesc, &depthStencilView));
        DepthStencilView = depthStencilView;

        // Create depth stencil state
        var depthStencilDesc = new DepthStencilDesc {
            DepthEnable = true,
            DepthWriteMask = DepthWriteMask.All,
            DepthFunc = ComparisonFunc.Less,
            StencilEnable = false,
        };

        ID3D11DepthStencilState* depthStencilState = null;
        SilkMarshal.ThrowHResult(DxDevice.CreateDepthStencilState(&depthStencilDesc, &depthStencilState));
        DepthStencilState = depthStencilState;

        // Create rasterizer states
        var solidRasterizerDesc = new RasterizerDesc {
            FillMode = FillMode.Solid,
            CullMode = CullMode.Back,
            FrontCounterClockwise = false,
            DepthBias = 0,
            DepthBiasClamp = 0.0f,
            SlopeScaledDepthBias = 0.0f,
            DepthClipEnable = true,
            ScissorEnable = false,
            MultisampleEnable = false,
            AntialiasedLineEnable = false,
        };

        ID3D11RasterizerState* solidRasterizerState = null;
        SilkMarshal.ThrowHResult(DxDevice.CreateRasterizerState(&solidRasterizerDesc, &solidRasterizerState));
        SolidRasterizerState = solidRasterizerState;

        RasterizerDesc wireframeRasterizerDesc = solidRasterizerDesc;
        wireframeRasterizerDesc.FillMode = FillMode.Wireframe;
        wireframeRasterizerDesc.CullMode = CullMode.None;

        ID3D11RasterizerState* wireframeRasterizerState = null;
        SilkMarshal.ThrowHResult(DxDevice.CreateRasterizerState(&wireframeRasterizerDesc, &wireframeRasterizerState));
        WireframeRasterizerState = wireframeRasterizerState;

        // Create constant buffer for matrices and color
        // var constantBufferDesc = new BufferDesc {
        //     ByteWidth = 256, // 3 * 64 bytes for matrices + 16 bytes for color (rounded up to 256 for alignment)
        //     Usage = Usage.Dynamic,
        //     BindFlags = (uint) BindFlag.ConstantBuffer,
        //     CPUAccessFlags = (uint) CpuAccessFlag.Write,
        //     MiscFlags = 0,
        // };
        //
        // ID3D11Buffer* constantBuffer = null;
        // SilkMarshal.ThrowHResult(DxDevice.CreateBuffer(&constantBufferDesc, (SubresourceData*) null, &constantBuffer));
        // ConstantBuffer = constantBuffer;
    }


    /// <summary>
    /// Toggles wireframe rendering mode
    /// </summary>
    public void ToggleWireframeMode() {
        WireframeMode = !WireframeMode;
        Logger.Information("Wireframe mode: {Mode}", WireframeMode ? "ON" : "OFF");
    }

    public void CleanUp() {
        if (isCleanedUp) return; // Prevent multiple cleanup calls
        isCleanedUp = true;

        unsafe {
            if (DxDevice.Handle is not null) {
                ShaderPipelines?.CleanUp();

                // Clean up 3D resources
                DepthStencilBuffer.Dispose();
                DepthStencilView.Dispose();
                DepthStencilState.Dispose();
                SolidRasterizerState.Dispose();
                WireframeRasterizerState.Dispose();
                // ConstantBuffer.Dispose();

                DxDevice.Dispose();
                DxDeviceContext.Dispose();
                DxSwapChain.Dispose();
                Input?.Dispose();

                ShaderPipelines = null;
                DxDevice = default;
                DxDeviceContext = default;
                DxSwapChain = default;
                DepthStencilBuffer = default;
                DepthStencilView = default;
                DepthStencilState = default;
                SolidRasterizerState = default;
                WireframeRasterizerState = default;
                // ConstantBuffer = default;
                Input = null;

                Logger.Information("Graphics context cleaning up");
            }
        }

        try {
            if (DebuggerWindow is not null) {
                DebuggerWindow.Dispose();
                DebuggerWindow = null;
                Logger.Information("Window cleaning up");
            }
        } catch (Exception ex) {
            Logger.Warning(ex, "Exception during window cleanup");
        }
    }

    private static int CompareRenderers(DebugFieldRenderer item1, DebugFieldRenderer item2) {
        if (item1.Field.MapId < item2.Field.MapId) {
            return -1;
        }

        if (item1.Field.MapId > item2.Field.MapId) {
            return 1;
        }

        if (item1.Field.RoomId < item2.Field.RoomId) {
            return -1;
        }

        if (item1.Field.RoomId > item2.Field.RoomId) {
            return 1;
        }

        return 0;
    }

    public IFieldRenderer FieldAdded(FieldManager field) {
        Logger.Information("Field added {Name} [{Id}]", field.Metadata.Name, field.Metadata.Id);

        DebugFieldRenderer renderer = new DebugFieldRenderer(this, field);

        fieldRendererMutex.WaitOne();
        fieldRenderers.AddSorted(renderer, Comparer<DebugFieldRenderer>.Create(CompareRenderers));
        fieldRendererMutex.ReleaseMutex();

        fields[field] = renderer;

        return renderer;
    }

    public void FieldRemoved(FieldManager field) {
        if (!fields.TryGetValue(field, out DebugFieldRenderer? renderer)) {
            return;
        }

        Logger.Information("Field removed {Name} [{Id}]", field.Metadata.Name, field.Metadata.Id);

        renderer.CleanUp();

        fieldRendererMutex.WaitOne();
        fieldRenderers.RemoveSorted(renderer, Comparer<DebugFieldRenderer>.Create(CompareRenderers));
        fieldRendererMutex.ReleaseMutex();

        fields.Remove(field);
    }

    public DebugFieldWindow FieldWindowOpened() {
        DebugFieldWindow window = new DebugFieldWindow(this);

        fieldWindows.Add(window);

        return window;
    }

    public void FieldWindowClosed(DebugFieldWindow window) {
        fieldWindows.Remove(window);

        window.SetActiveRenderer(null);
    }

    private unsafe void OnFramebufferResize(Vector2D<int> newSize) {
        // Validate size to prevent errors with invalid dimensions
        if (newSize.X <= 0 || newSize.Y <= 0) {
            return; // Skip resize if dimensions are invalid
        }

        // there is currently a bug with resizing where the framebuffer positioning doesn't take into account title bar size
        SilkMarshal.ThrowHResult(DxSwapChain.ResizeBuffers(0, (uint) newSize.X, (uint) newSize.Y, Format.FormatB8G8R8A8Unorm, 0));

        // Recreate depth buffer for new size
        if (DepthStencilBuffer.Handle != null) {
            DepthStencilBuffer.Dispose();
            DepthStencilView.Dispose();

            // Recreate depth buffer with new size
            var depthBufferDesc = new Texture2DDesc {
                Width = (uint) newSize.X,
                Height = (uint) newSize.Y,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.FormatD24UnormS8Uint,
                SampleDesc = new SampleDesc(1, 0),
                Usage = Usage.Default,
                BindFlags = (uint) BindFlag.DepthStencil,
                CPUAccessFlags = 0,
                MiscFlags = 0,
            };

            ID3D11Texture2D* depthStencilBuffer = null;
            SilkMarshal.ThrowHResult(DxDevice.CreateTexture2D(&depthBufferDesc, (SubresourceData*) null, &depthStencilBuffer));
            DepthStencilBuffer = depthStencilBuffer;

            var depthStencilViewDesc = new DepthStencilViewDesc {
                Format = Format.FormatD24UnormS8Uint,
                ViewDimension = DsvDimension.Texture2D,
                Flags = 0,
            };
            depthStencilViewDesc.Anonymous.Texture2D.MipSlice = 0;

            ID3D11DepthStencilView* depthStencilView = null;
            SilkMarshal.ThrowHResult(DxDevice.CreateDepthStencilView((ID3D11Resource*) DepthStencilBuffer.Handle, &depthStencilViewDesc, &depthStencilView));
            DepthStencilView = depthStencilView;
        }

        // Update projection matrix for new aspect ratio
        // UpdateProjectionMatrix();
    }

    public bool HasFieldUpdated(FieldManager field) {
        lock (updatedFieldsLock) {
            return updatedFields.Contains(field);
        }
    }

    public void FieldUpdated(FieldManager field) {
        lock (updatedFieldsLock) {
            updatedFields.Add(field);
        }
    }

    private void OnUpdate(double delta) {
        lock (updatedFieldsLock) {
            updatedFields.Clear();
        }

        if (ShaderPipelines is null) {
            return;
        }

        if (ShaderPipelines.CompilingShaders && ShaderPipelines.RenderingFrames == 0) {
            ShaderPipelines.CleanUp();

            LoadPipelines();
        }
    }

    private void UpdateDeltaTracker() {
        DateTime currentTime = DateTime.Now;

        int deltaMs = (int) ((currentTime.Ticks - lastTime.Ticks) / TimeSpan.TicksPerMillisecond);

        int timeLeftToWait = int.Max(0, 15 - deltaMs - 1);

        while (timeLeftToWait > 0) {
            Thread.Sleep(1);

            currentTime = DateTime.Now;

            deltaMs = (int) ((currentTime.Ticks - lastTime.Ticks) / TimeSpan.TicksPerMillisecond);
            timeLeftToWait = int.Max(0, 16 - deltaMs - 1);
        }

        lastTime = currentTime;

        if (deltaTimes.Count < 50) {
            deltaTimes.Add(deltaMs);
        } else {
            deltaTimes[deltaIndex] = deltaMs;
            deltaIndex = (deltaIndex + 1) % 50;
        }

        int total = 0;

        DeltaMin = deltaTimes.FirstOrDefault();
        DeltaMax = 0;

        foreach (int delta in deltaTimes) {
            total += delta;

            DeltaMin = int.Min(DeltaMin, delta);
            DeltaMax = int.Max(DeltaMax, delta);
        }

        DeltaAverage = total / deltaTimes.Count;
    }

    private unsafe void OnRender(double delta) {
        if (!(ShaderPipelines?.CanRender ?? false)) {
            return;
        }

        ShaderPipelines?.StartedFrame();
        UpdateDeltaTracker();

        DebuggerWindow!.MakeCurrent();

        ComPtr<ID3D11Texture2D> framebuffer = DxSwapChain.GetBuffer<ID3D11Texture2D>(0);

        ComPtr<ID3D11RenderTargetView> renderTargetView = default;
        SilkMarshal.ThrowHResult(DxDevice.CreateRenderTargetView(framebuffer, null, ref renderTargetView));

        // Clear both color and depth buffers
        DxDeviceContext.ClearRenderTargetView(renderTargetView, WindowClearColor);
        DxDeviceContext.ClearDepthStencilView(DepthStencilView, (uint) (ClearFlag.Depth | ClearFlag.Stencil), 1.0f, 0);

        Viewport viewport = new Viewport(0, 0, DebuggerWindow!.FramebufferSize.X, DebuggerWindow!.FramebufferSize.Y, 0, 1);
        DxDeviceContext.RSSetViewports(1, in viewport);

        // Set render targets with depth buffer
        DxDeviceContext.OMSetRenderTargets(1, in renderTargetView.Handle, DepthStencilView);

        // Set depth stencil state
        DxDeviceContext.OMSetDepthStencilState(DepthStencilState, 1);

        // Set rasterizer state based on wireframe mode
        DxDeviceContext.RSSetState(WireframeMode ? WireframeRasterizerState : SolidRasterizerState);

        ImGuiController!.BeginFrame((float) delta);
        ImGuiController!.EndFrame();

        DxSwapChain.Present(1, 0);

        ShaderPipelines?.EndedFrame();

        renderTargetView.Dispose();
        framebuffer.Dispose();
    }

    public void SetWireframeRasterizer() {
        // Set wireframe rasterizer state
        // This would typically involve setting D3D11 rasterizer state
        // For now, we'll rely on the WireframeMode flag
    }

    public void SetSolidRasterizer() {
        // Set solid rasterizer state
        // This would typically involve setting D3D11 rasterizer state
        // For now, we'll rely on the WireframeMode flag
    }

    private string GetResourceRootPath(string rootPath) {
        resourceRootPath = Environment.CurrentDirectory;

        if (File.Exists("root_path.txt")) {
            resourceRootPath = Path.Combine(resourceRootPath, File.ReadLines("root_path.txt").First());
        }

        return Path.GetFullPath(Path.Combine(resourceRootPath, rootPath));
    }
}
