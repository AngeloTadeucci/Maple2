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
using ImGuiNET;
using Maple2.Tools.Extensions;

namespace Maple2.Server.DebugGame.Graphics {
    public class DebugGraphicsContext : IGraphicsContext {
        public static readonly bool ForceDXVK = false;
        public static readonly Vector2D<int> DefaultWindowSize = new Vector2D<int>(800, 600);
        public static readonly float[] WindowClearColor = { 0.0f, 0.0f, 0.0f, 1.0f };
        public static readonly ILogger Logger = Log.Logger.ForContext<DebugGraphicsContext>();

        public readonly Dictionary<FieldManager, DebugFieldRenderer> Fields;

        public IWindow? DebuggerWindow { get; private set; }
        public IInputContext? Input { get; private set; }

        public D3D11? D3d11 { get; private set; }
        public DXGI? Dxgi { get; private set; }
        public D3DCompiler? Compiler { get; private set; }

        public ComPtr<ID3D11Device> DxDevice { get; private set; }
        public ComPtr<ID3D11DeviceContext> DxDeviceContext { get; private set; }
        public ComPtr<IDXGIFactory2> DxFactory { get; private set; }
        public ComPtr<IDXGISwapChain1> DxSwapChain { get; private set; }
        public VertexShader? VertexShader { get; private set; }
        public PixelShader? PixelShader { get; private set; }

        public CoreModels? CoreModels { get; private set; }
        public Texture? SampleTexture;
        public ImGuiController? ImGuiController { get; private set; }

        private string resourceRootPath = "";

        private List<DebugFieldRenderer> fieldRenderers = new();
        private Mutex fieldRendererMutex = new();
        public DebugFieldRenderer[] FieldRenderers {
            get {
                fieldRendererMutex.WaitOne();
                DebugFieldRenderer[] renderers = fieldRenderers.ToArray();
                fieldRendererMutex.ReleaseMutex();

                return renderers;
            }
        }
        public IReadOnlyList<DebugFieldWindow> FieldWindows { get => fieldWindows; }
        private List<DebugFieldWindow> fieldWindows = new();
        private HashSet<FieldManager> updatedFields = new();
        private int deltaIndex = 0;
        private List<int> deltaTimes = new();
        public int DeltaAverage { get; private set; }
        public int DeltaMin { get; private set; }
        public int DeltaMax { get; private set; }
        private DateTime lastTime = DateTime.Now;
        public bool IsClosing { get; private set; }

        public DebugGraphicsContext() {
            Fields = new Dictionary<FieldManager, DebugFieldRenderer>();
        }

        public void RunDebugger() {
            DebuggerWindow!.Initialize();

            bool subWindowsUpdating = false;

            while (!(DebuggerWindow?.IsClosing ?? true) || subWindowsUpdating) {
                if (DebuggerWindow is not null && !DebuggerWindow.IsClosing) {
                    UpdateWindow(DebuggerWindow, IsClosing);

                    if (DebuggerWindow.IsClosing) {
                        CleanUp();
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
            bool startedOpen = window.IsClosing;

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
            unsafe {
                if (DebuggerWindow is not null) {
                    CleanUp();
                }
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

            Dxgi = DXGI.GetApi(DebuggerWindow, ForceDXVK);
            D3d11 = D3D11.GetApi(DebuggerWindow, ForceDXVK);
            Compiler = D3DCompiler.GetApi();

            Shader.ShaderRootPath = GetResourceRootPath("Shaders");
            Texture.TextureRootPath = GetResourceRootPath("Textures");

            unsafe {
                ComPtr<ID3D11Device> device = default;
                ComPtr<ID3D11DeviceContext> deviceContext = default;

                SilkMarshal.ThrowHResult(D3d11.CreateDevice(
                    pAdapter: default(ComPtr<IDXGIAdapter>),
                    DriverType: D3DDriverType.Hardware,
                    Software: default,
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
                        quality: 0) // no antialiasing
                };

                // Factory1 adds DXGI 1.1 support & Factory2 adds DXGI 1.2 support
                DxFactory = Dxgi.CreateDXGIFactory<IDXGIFactory2>();

                IDXGISwapChain1* swapChain = default;

                SilkMarshal.ThrowHResult(DxFactory.CreateSwapChainForHwnd(
                    pDevice: (IUnknown*) (ID3D11Device*) DxDevice,
                    hWnd: DebuggerWindow!.Native!.DXHandle!.Value,
                    pDesc: &swapChainDescription,
                    pFullscreenDesc: (SwapChainFullscreenDesc*) null,
                    pRestrictToOutput: (IDXGIOutput*) null,
                    ppSwapChain: &swapChain));

                DxSwapChain = swapChain;
            }

            if (VertexShader is null) {
                VertexShader = new VertexShader(this);
            }

            if (PixelShader is null) {
                PixelShader = new PixelShader(this);
            }

            VertexShader.Load("screenVertex.hlsl", "vs_main");
            PixelShader.Load("screenPixel.hlsl", "ps_main");

            this.CoreModels = new CoreModels(this);

            Logger.Information("Graphics context initialized");

            SampleTexture = new Texture(this);
            SampleTexture.Load("sample_derp_wave.png");

            ImGuiController = new ImGuiController(this, Input, ImGuiWindowType.Main);

            ImGuiController.Initialize(DebuggerWindow);
        }

        private string GetResourceRootPath(string rootPath) {
            resourceRootPath = Environment.CurrentDirectory;

            if (File.Exists("root_path.txt")) {
                resourceRootPath = Path.Combine(resourceRootPath, File.ReadLines("root_path.txt").First());
            }

            return Path.GetFullPath(Path.Combine(resourceRootPath, rootPath));
        }

        public void CleanUp() {
            unsafe {
                if (DxDevice.Handle is not null) {
                    VertexShader?.CleanUp();
                    PixelShader?.CleanUp();
                    DxDevice.Dispose();
                    DxDeviceContext.Dispose();
                    DxSwapChain.Dispose();
                    Input?.Dispose();

                    VertexShader = default;
                    PixelShader = default;
                    DxDevice = default;
                    DxDeviceContext = default;
                    DxSwapChain = default;
                    Input = default;

                    Logger.Information("Graphics context cleaning up");
                }
            }

            if (DebuggerWindow is not null) {
                DebuggerWindow.Dispose();

                DebuggerWindow = default;

                Logger.Information("Window cleaning up");
            }
        }

        private static int CompareRenderers(DebugFieldRenderer item1, DebugFieldRenderer item2) {
            if (item1.Field.MapId < item2.Field.MapId) {
                return -1;
            }

            if (item1.Field.MapId > item2.Field.MapId) {
                return 1;
            }

            if (item1.Field.InstanceId < item2.Field.InstanceId) {
                return -1;
            }

            if (item1.Field.InstanceId > item2.Field.InstanceId) {
                return 1;
            }

            return 0;
        }

        public IFieldRenderer FieldAdded(FieldManager field) {
            Logger.Information("Field added {Name} [{Id}]", field.Metadata.Name, field.Metadata.Id);

            DebugFieldRenderer renderer = new DebugFieldRenderer(this, field);

            fieldRendererMutex.WaitOne();
            int index = fieldRenderers.AddSorted(renderer, Comparer<DebugFieldRenderer>.Create(CompareRenderers));
            fieldRendererMutex.ReleaseMutex();

            return renderer;
        }

        public void FieldRemoved(FieldManager field) {
            if (!Fields.TryGetValue(field, out DebugFieldRenderer? renderer)) {
                return;
            }

            Logger.Information("Field removed {Name} [{Id}]", field.Metadata.Name, field.Metadata.Id);

            renderer.CleanUp();

            fieldRendererMutex.WaitOne();
            fieldRenderers.RemoveSorted(renderer, Comparer<DebugFieldRenderer>.Create(CompareRenderers));
            fieldRendererMutex.ReleaseMutex();

            Fields.Remove(field);
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
            // there is currently a bug with resizing where the framebuffer positioning doesn't take into account title bar size
            SilkMarshal.ThrowHResult(DxSwapChain.ResizeBuffers(0, (uint) newSize.X, (uint) newSize.Y, Format.FormatB8G8R8A8Unorm, 0));
        }

        public bool HasFieldUpdated(FieldManager field) {
            return updatedFields.Contains(field);
        }

        public void FieldUpdated(FieldManager field) {
            if (!updatedFields.Contains(field)) {
                updatedFields.Add(field);
            }
        }

        private void OnUpdate(double delta) {
            updatedFields.Clear();
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

            DxDeviceContext.ClearRenderTargetView(renderTargetView, WindowClearColor);

            Viewport viewport = new Viewport(0, 0, DebuggerWindow!.FramebufferSize.X, DebuggerWindow!.FramebufferSize.Y, 0, 1);
            DxDeviceContext.RSSetViewports(1, ref viewport);

            DxDeviceContext.OMSetRenderTargets(1, ref renderTargetView.Handle, (ID3D11DepthStencilView*) null);

            ImGuiController!.BeginFrame((float) delta);

            #region Render code
            // Begin region for render code

            // A vertex + pixel shader required to draw meshes
            VertexShader!.Bind();
            PixelShader!.Bind();
            SampleTexture!.Bind(); // bind textures to active GPU texture samplers for access in shaders
            CoreModels!.Quad.Draw(); // draw a full screen quad/rectangle

            // logic
            //FieldListWindow();
            //RendererListWindow();

            // End region for render code
            #endregion

            ImGuiController!.EndFrame();

            DxSwapChain.Present(1, 0);

            renderTargetView.Dispose();
            framebuffer.Dispose();
        }
    }
}
