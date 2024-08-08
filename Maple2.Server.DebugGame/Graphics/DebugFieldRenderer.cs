using Maple2.Server.Game.DebugGraphics;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.DebugGame.Graphics {
    public class DebugFieldRenderer : IFieldRenderer {
        public DebugGraphicsContext Context { get; init; }
        public FieldManager Field { get; init; }
        public bool IsActive {
            get {
                activeMutex.WaitOne();
                bool isActive = this.isActive;
                activeMutex.ReleaseMutex();
                return isActive;
            }
            set {
                activeMutex.WaitOne();
                isActive = value;
                activeMutex.ReleaseMutex();
            }
        }
        private bool isActive;
        private Mutex activeMutex;

        public DebugFieldRenderer(DebugGraphicsContext context, FieldManager field) {
            Context = context;
            Field = field;
        }

        public void Update() {
            if (!IsActive) {
                return;
            }

            Field.Update();
        }

        public void Render(long tickCount) {

        }

        public void CleanUp() {

        }
    }
}
