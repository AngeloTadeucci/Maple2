using ImGuiNET;
using Silk.NET.Maths;
using System.Numerics;

namespace Maple2.Server.DebugGame.Graphics;

public class ImGuiController {
    public DebugGraphicsContext Context { get; init; }
    public IntPtr ImGuiContext { get; private set; }

    public ImGuiController(DebugGraphicsContext context) {
        Context = context;
    }

    public void Initialize() {

        ImGuiContext = ImGui.CreateContext();
        ImGui.SetCurrentContext(ImGuiContext);
        
        SetImGuiWindowData(1.0f / 60.0f);
    }

    public void CleanUp() {

    }

    private void SetImGuiWindowData(float delta) {
        Vector2D<int> frameSize = Context.DebuggerWindow!.FramebufferSize;

        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new Vector2(frameSize.X, frameSize.Y);
        io.DisplayFramebufferScale = new Vector2(1, 1);
        io.DeltaTime = delta;
    }

    public void BeginFrame(float delta) {
        SetImGuiWindowData(delta);
        
        ImGui.NewFrame();
    }

    public void EndFrame() {
        ImGui.Render();
    }
}
