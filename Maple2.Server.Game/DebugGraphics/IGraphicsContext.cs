using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.DebugGraphics {
    public interface IGraphicsContext {
        public void Initialize();
        public void CleanUp();
        public IFieldRenderer? FieldAdded(FieldManager field);
        public void FieldRemoved(FieldManager field);
    }
}
