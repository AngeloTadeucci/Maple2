namespace Maple2.Server.DebugGame.Graphics.Resources;

public class RenderPass {
    public string TargetName = string.Empty;

    public VertexShader? VertexShader = null;
    public PixelShader? PixelShader = null;

    public void CleanUp() {
        VertexShader?.CleanUp();
        PixelShader?.CleanUp();
    }
}

public class ShaderPipeline {
    public string Name = string.Empty;
    public RenderPass? RenderPass = null;

    public void CleanUp() {
        RenderPass?.CleanUp();
    }
}
