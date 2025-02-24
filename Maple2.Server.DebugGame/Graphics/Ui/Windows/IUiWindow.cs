namespace Maple2.Server.DebugGame.Graphics.Ui.Windows;

public interface IUiWindow {
    public bool AllowMainWindow { get => false; }
    public bool AllowFieldWindow { get => false; }
    public bool Enabled { get; set; }
    public string TypeName { get; }
    public DebugGraphicsContext? Context { get; protected set; }
    public ImGuiController? ImGuiController { get; protected set; }
    public DebugFieldWindow? FieldWindow { get; protected set; }

    public void Initialize(DebugGraphicsContext context, ImGuiController controller, DebugFieldWindow? fieldWindow);
    public void Render();
}
