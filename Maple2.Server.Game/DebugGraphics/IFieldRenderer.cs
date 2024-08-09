namespace Maple2.Server.Game.DebugGraphics {
    public interface IFieldRenderer {
        public bool IsActive { get; }
        public void Render(double delta);
    }
}
