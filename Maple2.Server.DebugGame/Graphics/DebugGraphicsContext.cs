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

    // Enhanced shader support
    private VertexShader? VertexShader { get; set; }
    private PixelShader? PixelShader { get; set; }
    public VertexShader? WireframeVertexShader { get; private set; }
    public PixelShader? WireframePixelShader { get; private set; }

    // 3D rendering resources
    private ComPtr<ID3D11Texture2D> DepthStencilBuffer { get; set; }
    private ComPtr<ID3D11DepthStencilView> DepthStencilView { get; set; }
    private ComPtr<ID3D11DepthStencilState> DepthStencilState { get; set; }
    private ComPtr<ID3D11RasterizerState> SolidRasterizerState { get; set; }
    private ComPtr<ID3D11RasterizerState> WireframeRasterizerState { get; set; }
    private ComPtr<ID3D11Buffer> ConstantBuffer { get; set; }

    public CoreModels? CoreModels { get; private set; }
    private Texture? sampleTexture;
    private ImGuiController? ImGuiController { get; set; }

    // Camera controller interface for all camera functionality
    public ICameraController CameraController { get; private set; } = null!;

    // Available controller implementations
    private FreeCameraController freeCameraController = null!;
    private FollowCameraController followCameraController = null!;

    // Shared camera instance
    private readonly Camera sharedCamera = new();

    // Global follow state (independent of active controller)
    private bool hasManuallyStoppedFollowing = false;

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

        // Use application base directory where files are copied during build
        Shader.ShaderRootPath = Path.Combine(AppContext.BaseDirectory, "Shaders");
        Texture.TextureRootPath = Path.Combine(AppContext.BaseDirectory, "Textures");

        // Set AssetRootPath for shader include system
        ShaderPipelines.AssetRootPath = AppContext.BaseDirectory;

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

        // Create shaders
        VertexShader ??= new VertexShader(this);

        PixelShader ??= new PixelShader(this);

        WireframeVertexShader ??= new VertexShader(this);

        WireframePixelShader ??= new PixelShader(this);

        VertexShader.Load(Path.Combine(Shader.ShaderRootPath, "screenVertex.hlsl"), "vs_main");
        PixelShader.Load(Path.Combine(Shader.ShaderRootPath, "screenPixel.hlsl"), "ps_main");
        WireframeVertexShader.Load(Path.Combine(Shader.ShaderRootPath, "wireframeVertex.hlsl"), "vs_main");
        WireframePixelShader.Load(Path.Combine(Shader.ShaderRootPath, "wireframePixel.hlsl"), "ps_main");

        // Create 3D rendering resources
        Create3DRenderingResources();

        CoreModels = new CoreModels(this);

        Logger.Information("Graphics context initialized");

        sampleTexture = new Texture(this);
        sampleTexture.Load(Path.Combine(Texture.TextureRootPath, "sample_derp_wave.png"));

        // Initialize camera controllers
        freeCameraController = new FreeCameraController(sharedCamera);
        followCameraController = new FollowCameraController(sharedCamera);

        // Start with free camera controller as default
        CameraController = freeCameraController;

        Vector2D<int> windowSize = DebuggerWindow?.FramebufferSize ?? DefaultWindowSize;
        sharedCamera.UpdateProjectionMatrix(windowSize.X, windowSize.Y);
        CameraController.SetDefaultRotation(); // Set default rotation for MapleStory 2

        ImGuiController = new ImGuiController(this, Input, ImGuiWindowType.Main);

        if (DebuggerWindow is not null) {
            ImGuiController.Initialize(DebuggerWindow);
        }
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
        var constantBufferDesc = new BufferDesc {
            ByteWidth = 256, // 3 * 64 bytes for matrices + 16 bytes for color (rounded up to 256 for alignment)
            Usage = Usage.Dynamic,
            BindFlags = (uint) BindFlag.ConstantBuffer,
            CPUAccessFlags = (uint) CpuAccessFlag.Write,
            MiscFlags = 0,
        };

        ID3D11Buffer* constantBuffer = null;
        SilkMarshal.ThrowHResult(DxDevice.CreateBuffer(&constantBufferDesc, (SubresourceData*) null, &constantBuffer));
        ConstantBuffer = constantBuffer;
    }

    public void UpdateProjectionMatrix() {
        Vector2D<int> windowSize = DebuggerWindow?.FramebufferSize ?? DefaultWindowSize;
        CameraController.Camera.UpdateProjectionMatrix(windowSize.X, windowSize.Y);
    }

    public void UpdateViewMatrix() {
        CameraController.Camera.UpdateViewMatrix();
    }

    /// <summary>
    /// Switches to the free camera controller
    /// </summary>
    private void SwitchToFreeCameraController() {
        if (CameraController != freeCameraController) {
            Logger.Information("Switching from {OldController} to FreeCameraController", GetCurrentControllerType());
            CameraController = freeCameraController;
            Logger.Information("Switched to free camera controller - IsFollowing={IsFollowing}", CameraController.IsFollowingPlayer);
        } else {
            Logger.Information("Already using FreeCameraController - no switch needed");
        }
    }

    /// <summary>
    /// Switches to the follow camera controller
    /// </summary>
    private void SwitchToFollowCameraController() {
        if (CameraController != followCameraController) {
            CameraController = followCameraController;
            Logger.Information("Switched to follow camera controller");
        }
    }

    /// <summary>
    /// Gets the current controller type name for UI display
    /// </summary>
    public string GetCurrentControllerType() {
        return CameraController switch {
            FreeCameraController => "Free Camera",
            FollowCameraController => "Follow Camera",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Whether the user has manually stopped following (global state)
    /// </summary>
    public bool HasManuallyStopped => hasManuallyStoppedFollowing;

    /// <summary>
    /// Starts following a player and automatically switches to follow camera controller
    /// </summary>
    public void StartFollowingPlayer(long playerId) {
        hasManuallyStoppedFollowing = false; // Reset manual stop flag when starting to follow
        SwitchToFollowCameraController();
        followCameraController.StartFollowingPlayer(playerId);
    }

    /// <summary>
    /// Stops following a player and automatically switches to free camera controller
    /// </summary>
    public void StopFollowingPlayer() {
        Logger.Information("StopFollowingPlayer called - before: IsFollowing={IsFollowing}, Controller={Controller}",
            CameraController.IsFollowingPlayer, GetCurrentControllerType());

        hasManuallyStoppedFollowing = true; // Remember that user manually stopped
        followCameraController.StopFollowingPlayer();
        SwitchToFreeCameraController();

        Logger.Information("StopFollowingPlayer called - after: IsFollowing={IsFollowing}, Controller={Controller}, HasManuallyStopped={HasManuallyStopped}",
            CameraController.IsFollowingPlayer, GetCurrentControllerType(), HasManuallyStopped);
    }

    /// <summary>
    /// Updates player follow position (only works when follow controller is active)
    /// </summary>
    public void UpdatePlayerFollow(Vector3 playerPosition) {
        if (CameraController == followCameraController) {
            followCameraController.UpdatePlayerFollow(playerPosition);
        }
    }

    public void UpdateConstantBuffer(Matrix4x4 worldMatrix) {
        UpdateConstantBuffer(worldMatrix, new Vector4(1.0f, 1.0f, 1.0f, 1.0f)); // Default white color
    }

    // Current color for rendering
    private Vector4 currentColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // Default white

    public void SetColor(float r, float g, float b, float a) {
        currentColor = new Vector4(r, g, b, a);
    }

    public void UpdateConstantBuffer(Matrix4x4 worldMatrix, bool useCurrentColor) {
        UpdateConstantBuffer(worldMatrix, useCurrentColor ? currentColor : new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
    }

    public unsafe void UpdateConstantBuffer(Matrix4x4 worldMatrix, Vector4 color) {
        if (ConstantBuffer.Handle == null) return;

        MappedSubresource mappedResource;
        SilkMarshal.ThrowHResult(DxDeviceContext.Map(ConstantBuffer, 0, Map.WriteDiscard, 0, &mappedResource));

        // Write matrices and color to constant buffer (world, view, projection, color)
        float* data = (float*) mappedResource.PData;

        // Copy world matrix (transposed)
        Matrix4x4 worldTransposed = Matrix4x4.Transpose(worldMatrix);
        data[0] = worldTransposed.M11;
        data[1] = worldTransposed.M12;
        data[2] = worldTransposed.M13;
        data[3] = worldTransposed.M14;
        data[4] = worldTransposed.M21;
        data[5] = worldTransposed.M22;
        data[6] = worldTransposed.M23;
        data[7] = worldTransposed.M24;
        data[8] = worldTransposed.M31;
        data[9] = worldTransposed.M32;
        data[10] = worldTransposed.M33;
        data[11] = worldTransposed.M34;
        data[12] = worldTransposed.M41;
        data[13] = worldTransposed.M42;
        data[14] = worldTransposed.M43;
        data[15] = worldTransposed.M44;

        // Copy view matrix (transposed)
        Matrix4x4 viewTransposed = Matrix4x4.Transpose(CameraController.Camera.ViewMatrix);
        data[16] = viewTransposed.M11;
        data[17] = viewTransposed.M12;
        data[18] = viewTransposed.M13;
        data[19] = viewTransposed.M14;
        data[20] = viewTransposed.M21;
        data[21] = viewTransposed.M22;
        data[22] = viewTransposed.M23;
        data[23] = viewTransposed.M24;
        data[24] = viewTransposed.M31;
        data[25] = viewTransposed.M32;
        data[26] = viewTransposed.M33;
        data[27] = viewTransposed.M34;
        data[28] = viewTransposed.M41;
        data[29] = viewTransposed.M42;
        data[30] = viewTransposed.M43;
        data[31] = viewTransposed.M44;

        // Copy projection matrix (transposed)
        Matrix4x4 projTransposed = Matrix4x4.Transpose(CameraController.Camera.ProjectionMatrix);
        data[32] = projTransposed.M11;
        data[33] = projTransposed.M12;
        data[34] = projTransposed.M13;
        data[35] = projTransposed.M14;
        data[36] = projTransposed.M21;
        data[37] = projTransposed.M22;
        data[38] = projTransposed.M23;
        data[39] = projTransposed.M24;
        data[40] = projTransposed.M31;
        data[41] = projTransposed.M32;
        data[42] = projTransposed.M33;
        data[43] = projTransposed.M34;
        data[44] = projTransposed.M41;
        data[45] = projTransposed.M42;
        data[46] = projTransposed.M43;
        data[47] = projTransposed.M44;

        // Copy color
        data[48] = color.X;
        data[49] = color.Y;
        data[50] = color.Z;
        data[51] = color.W;

        DxDeviceContext.Unmap(ConstantBuffer, 0);

        // Bind constant buffer to vertex shader
        ID3D11Buffer* constantBuffers = ConstantBuffer;
        DxDeviceContext.VSSetConstantBuffers(0, 1, &constantBuffers);
    }

    public void CleanUp() {
        if (isCleanedUp) return; // Prevent multiple cleanup calls
        isCleanedUp = true;

        unsafe {
            if (DxDevice.Handle is not null) {
                VertexShader?.CleanUp();
                PixelShader?.CleanUp();
                WireframeVertexShader?.CleanUp();
                WireframePixelShader?.CleanUp();

                // Clean up 3D resources
                DepthStencilBuffer.Dispose();
                DepthStencilView.Dispose();
                DepthStencilState.Dispose();
                SolidRasterizerState.Dispose();
                WireframeRasterizerState.Dispose();
                ConstantBuffer.Dispose();

                DxDevice.Dispose();
                DxDeviceContext.Dispose();
                DxSwapChain.Dispose();
                Input?.Dispose();

                VertexShader = null;
                PixelShader = null;
                WireframeVertexShader = null;
                WireframePixelShader = null;
                DxDevice = default;
                DxDeviceContext = default;
                DxSwapChain = default;
                DepthStencilBuffer = default;
                DepthStencilView = default;
                DepthStencilState = default;
                SolidRasterizerState = default;
                WireframeRasterizerState = default;
                ConstantBuffer = default;
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
        UpdateProjectionMatrix();
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
        DxDeviceContext.RSSetState(CameraController.WireframeMode ? WireframeRasterizerState : SolidRasterizerState);

        ImGuiController!.BeginFrame((float) delta);
        ImGuiController!.EndFrame();

        DxSwapChain.Present(1, 0);

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
}
