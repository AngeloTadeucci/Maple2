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
using System.Numerics;

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

            DebuggerWindow = Window.Create(windowOptions);

            DebuggerWindow.FramebufferResize += OnFramebufferResize;
            DebuggerWindow.Render += OnRender;
            DebuggerWindow.Update += OnUpdate;
            DebuggerWindow.Load += OnLoad;

            Log.Information("Creating window");

            DebuggerWindow.Run();
        }

        private unsafe void DxLog(Message message) {
            if (message.PDescription is null) {
                Log.Information("Null DirectX error");

                return;
            }

            Log.Information(SilkMarshal.PtrToString((nint) message.PDescription) ?? "Unknown DirectX error");
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
                    pFullscreenDesc: (SwapChainFullscreenDesc*)null,
                    pRestrictToOutput: (IDXGIOutput*)null,
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

            ImGuiController = new ImGuiController(this);
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
                    DebuggerWindow?.Dispose();
                    Input?.Dispose();

                    Log.Information("Graphics context cleaning up");
                }
            }

            if (DebuggerWindow is not null) {
                DebuggerWindow.Dispose();

                Log.Information("Window cleaning up");
            }
        }

        public IFieldRenderer FieldAdded(FieldManager field) {
            Log.Information("Field added {Name} [{Id}]", field.Metadata.Name, field.Metadata.Id);

            return new DebugFieldRenderer(this, field);
        }

        public void FieldRemoved(FieldManager field) {
            if (!Fields.TryGetValue(field, out DebugFieldRenderer? renderer)) {
                return;
            }

            Log.Information("Field removed {Name} [{Id}]", field.Metadata.Name, field.Metadata.Id);

            renderer.CleanUp();

            Fields.Remove(field);
        }

        private unsafe void OnFramebufferResize(Vector2D<int> newSize) {
            SilkMarshal.ThrowHResult(DxSwapChain.ResizeBuffers(0, (uint) newSize.X, (uint) newSize.Y, Format.FormatB8G8R8A8Unorm, 0));
        }

        private void OnUpdate(double delta) {

        }

        private unsafe void OnRender(double delta) {
            var framebuffer = DxSwapChain.GetBuffer<ID3D11Texture2D>(0);

            ComPtr<ID3D11RenderTargetView> renderTargetView = default;
            SilkMarshal.ThrowHResult(DxDevice.CreateRenderTargetView(framebuffer, null, ref renderTargetView));

            DxDeviceContext.ClearRenderTargetView(renderTargetView, WindowClearColor);

            Viewport viewport = new Viewport(0, 0, DebuggerWindow!.FramebufferSize.X, DebuggerWindow!.FramebufferSize.Y, 0, 1);
            DxDeviceContext.RSSetViewports(1, ref viewport);

            DxDeviceContext.OMSetRenderTargets(1, ref renderTargetView.Handle, (ID3D11DepthStencilView*)null);

            ImGuiController!.BeginFrame((float)delta);

            VertexShader!.Bind();
            PixelShader!.Bind();
            SampleTexture!.Bind();
            CoreModels!.Quad.Draw();

            // logic

            ImGuiController!.EndFrame();

            DxSwapChain.Present(1, 0);

            renderTargetView.Dispose();
        }
    }
}
