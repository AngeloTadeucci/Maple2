namespace Maple2.Server.DebugGame.Graphics.Enum;

[Flags]
public enum ShaderStageFlags {
    None = 0,

    Vertex = (1 << 0),
    Pixel = (1 << 1),
}
