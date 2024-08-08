
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.DebugGraphics {
    // This class does nothing. Intended for use with headless servers
    public class HeadlessGraphicsContext : IGraphicsContext {
        public void Initialize() { }
        public void CleanUp() { }

        public IFieldRenderer? FieldAdded(FieldManager field) {
            return null;
        }

        public void FieldRemoved(FieldManager field) { }
    }
}
