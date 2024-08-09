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
        private Mutex fieldRenderersMutex = new();
        private DebugFieldRenderer? selectedRenderer = null;
        private List<DebugFieldWindow> fieldWindows = new();
        private Mutex fieldWindowsMutex = new();
        private DebugFieldWindow? selectedWindow = null;
        private HashSet<FieldManager> updatedFields = new();

        public DebugGraphicsContext() {
            Fields = new Dictionary<FieldManager, DebugFieldRenderer>();
        }

        public void Initialize() {
            unsafe {
                if (DebuggerWindow is not null) {
                    CleanUp();
                }
            }

            var windowOptions = WindowOptions.Default;
            windowOptions.Size = DefaultWindowSize;
            windowOptions.Title = "Maple2 Visual Debugger";
            windowOptions.API = GraphicsAPI.None;
            windowOptions.FramesPerSecond = 60;
            windowOptions.UpdatesPerSecond = 60;

            DebuggerWindow = Window.Create(windowOptions);

            DebuggerWindow.FramebufferResize += OnFramebufferResize;
            DebuggerWindow.Render += OnRender;
            DebuggerWindow.Update += OnUpdate;
            DebuggerWindow.Load += OnLoad;
            DebuggerWindow.Closing += OnClose;
            DebuggerWindow.IsEventDriven = false;

            Log.Information("Creating window");

            new Thread(DebuggerWindow.Run).Start();
        }

        public static unsafe void DxLog(Message message) {
            if (message.PDescription is null) {
                Log.Information("Null DirectX error");

                return;
            }

            Log.Information(SilkMarshal.PtrToString((nint) message.PDescription) ?? "Unknown DirectX error");
        }

        private void OnClose() {
            fieldWindowsMutex.WaitOne();
            foreach (DebugFieldWindow window in fieldWindows) {
                window.ForceClose();
            }
            fieldWindowsMutex.ReleaseMutex();
        }

        private void OnLoad() {
            Input = DebuggerWindow!.CreateInput();

            Dxgi = DXGI.GetApi(DebuggerWindow, ForceDXVK);
            D3d11 = D3D11.GetApi(DebuggerWindow, ForceDXVK);
            Compiler = D3DCompiler.GetApi();

            Shader.ShaderRootPath = GetResourceRootPath("Shaders");
            Texture.TextureRootPath = GetResourceRootPath("Textures");

            // input.Keyboards[].Keydown = OnKeyDown(IKeyboard keyboard, Key key, int scancode)
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

            Log.Information("Graphics context initialized");

            SampleTexture = new Texture(this);
            SampleTexture.Load("sample_derp_wave.png");

            ImGuiController = new ImGuiController(this, Input);

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

                    Log.Information("Graphics context cleaning up");
                }
            }

            if (DebuggerWindow is not null) {
                DebuggerWindow.Dispose();

                DebuggerWindow = default;

                Log.Information("Window cleaning up");
            }
        }

        private static int CompareRenderers(DebugFieldRenderer item1, DebugFieldRenderer item2) {
            if (item1.Field.MapId < item2.Field.MapId) {
                return -1;
            }

            if (item1.Field.MapId > item2.Field.MapId) {
                return 1;
            }

            if (item1.Field.InstanceId < item1.Field.InstanceId) {
                return -1;
            }

            if (item1.Field.InstanceId > item2.Field.InstanceId) {
                return 1;
            }

            return 0;
        }

        public IFieldRenderer FieldAdded(FieldManager field) {
            Log.Information("Field added {Name} [{Id}]", field.Metadata.Name, field.Metadata.Id);

            DebugFieldRenderer renderer = new DebugFieldRenderer(this, field);

            fieldRenderersMutex.WaitOne();
            int index = fieldRenderers.AddSorted(renderer, Comparer<DebugFieldRenderer>.Create(CompareRenderers));
            fieldRenderersMutex.ReleaseMutex();

            return renderer;
        }

        public void FieldRemoved(FieldManager field) {
            if (!Fields.TryGetValue(field, out DebugFieldRenderer? renderer)) {
                return;
            }

            Log.Information("Field removed {Name} [{Id}]", field.Metadata.Name, field.Metadata.Id);

            renderer.CleanUp();

            fieldRenderersMutex.WaitOne();
            fieldRenderers.RemoveSorted(renderer, Comparer<DebugFieldRenderer>.Create(CompareRenderers));
            fieldRenderersMutex.ReleaseMutex();

            Fields.Remove(field);
        }

        public DebugFieldWindow FieldWindowOpened() {
            DebugFieldWindow window = new DebugFieldWindow(this);

            fieldWindowsMutex.WaitOne();
            fieldWindows.Add(window);
            fieldWindowsMutex.ReleaseMutex();

            window.Initialize();

            return window;
        }

        public void FieldWindowClosed(DebugFieldWindow window) {
            fieldWindowsMutex.WaitOne();
            fieldWindows.Remove(window);
            fieldWindowsMutex.ReleaseMutex();

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
            fieldWindowsMutex.WaitOne();
            DebugFieldWindow[] windows = fieldWindows.ToArray();
            fieldWindowsMutex.ReleaseMutex();

            updatedFields.Clear();

            foreach (DebugFieldWindow window in windows) {
                window.OnUpdate(delta);
            }
        }

        private unsafe void OnRender(double delta) {
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
            FieldListWindow();
            RendererListWindow();

            // End region for render code
            #endregion

            ImGuiController!.EndFrame();

            DxSwapChain.Present(1, 0);

            renderTargetView.Dispose();
            framebuffer.Dispose();

            fieldWindowsMutex.WaitOne();
            DebugFieldWindow[] windows = fieldWindows.ToArray();
            fieldWindowsMutex.ReleaseMutex();

            foreach (DebugFieldWindow window in windows) {
                window.OnRender(delta);
            }
        }

        private void FieldListWindow() {
            ImGui.Begin("Fields");

            bool selectFieldDisabled = selectedWindow is null || selectedRenderer is null;

            if (selectFieldDisabled) {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button("Select field") && !selectFieldDisabled) {
                selectedWindow!.SetActiveRenderer(selectedRenderer);
            }

            if (selectFieldDisabled) {
                ImGui.EndDisabled();
            }

            if (ImGui.BeginTable("Active fields", 3)) {
                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Map Id");
                ImGui.TableSetColumnIndex(1);
                ImGui.Text("Map Name");
                ImGui.TableSetColumnIndex(2);
                ImGui.Text("Instance Id");

                int index = 0;
                fieldRenderersMutex.WaitOne();
                foreach (DebugFieldRenderer renderer in fieldRenderers) {
                    ImGui.TableNextRow();

                    bool selected = renderer == selectedRenderer;
                    bool nextSelected = false;

                    ImGui.TableSetColumnIndex(0);
                    nextSelected |= ImGui.Selectable(string.Format("{0}##Active fields {1} 0", renderer.Field.MapId, index), selected);
                    ImGui.TableSetColumnIndex(1);
                    nextSelected |= ImGui.Selectable(string.Format("{0}##Active fields {1} 1", renderer.Field.Metadata.Name, index), selected);
                    ImGui.TableSetColumnIndex(2);
                    nextSelected |= ImGui.Selectable(string.Format("{0}##Active fields {1} 2", renderer.Field.InstanceId, index), selected);

                    if (nextSelected) {
                        selectedRenderer = renderer;
                    }

                    ++index;
                }
                fieldRenderersMutex.ReleaseMutex();

                ImGui.EndTable();
            }

            ImGui.End();
        }

        private void RendererListWindow() {
            ImGui.Begin("Windows");

            bool newWindowDisabled = false;

            fieldWindowsMutex.WaitOne();
            DebugFieldWindow[] windows = fieldWindows.ToArray();
            fieldWindowsMutex.ReleaseMutex();

            foreach (DebugFieldWindow window in windows) {
                newWindowDisabled |= window.IsClosing;
            }

            if (newWindowDisabled) {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button("New window") && !newWindowDisabled) {
                selectedWindow = FieldWindowOpened();
            }

            if (newWindowDisabled) {
                ImGui.EndDisabled();
            }

            bool selectWindowDisabled = selectedWindow is null;

            if (selectWindowDisabled) {
                ImGui.BeginDisabled();
            }

            ImGui.SameLine();

            bool closeWindow = ImGui.Button("Close window");

            if (closeWindow && !selectWindowDisabled) {
                selectedWindow!.Close();
            }

            if (selectWindowDisabled) {
                ImGui.EndDisabled();
            }

            if (closeWindow) {
                selectedWindow = null;
            }

            if (ImGui.BeginTable("Active windows", 4)) {
                ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Window");
                ImGui.TableSetColumnIndex(1);
                ImGui.Text("Map Id");
                ImGui.TableSetColumnIndex(2);
                ImGui.Text("Map Name");
                ImGui.TableSetColumnIndex(3);
                ImGui.Text("Instance Id");

                int index = 0;
                fieldWindowsMutex.WaitOne();
                foreach (DebugFieldWindow window in fieldWindows) {
                    ImGui.TableNextRow();

                    bool selected = window == selectedWindow;
                    bool nextSelected = false;

                    ImGui.TableSetColumnIndex(0);
                    nextSelected |= ImGui.Selectable(string.Format("{0}##Active windows {1} 0", window.WindowName, index), selected);
                    ImGui.TableSetColumnIndex(1);
                    nextSelected |= ImGui.Selectable(string.Format("{0}##Active windows {1} 1", window.ActiveRenderer?.Field.MapId.ToString() ?? "", index), selected);
                    ImGui.TableSetColumnIndex(2);
                    nextSelected |= ImGui.Selectable(string.Format("{0}##Active windows {1} 2", window.ActiveRenderer?.Field.Metadata.Name ?? "", index), selected);
                    ImGui.TableSetColumnIndex(3);
                    nextSelected |= ImGui.Selectable(string.Format("{0}##Active windows {1} 3", window.ActiveRenderer?.Field.InstanceId.ToString() ?? "", index), selected);

                    if (nextSelected) {
                        selectedWindow = window;
                        selectedRenderer = window.ActiveRenderer;
                    }

                    ++index;
                }
                fieldWindowsMutex.ReleaseMutex();

                ImGui.EndTable();
            }

            ImGui.End();
        }
    }
}
