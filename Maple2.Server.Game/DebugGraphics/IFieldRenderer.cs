namespace Maple2.Server.Game.DebugGraphics {
    public interface IFieldRenderer {
        public bool IsActive { get; set; }
        public void Render(long tickCount);
    }
}
