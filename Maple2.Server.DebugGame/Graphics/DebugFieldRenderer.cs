using Maple2.Server.Game.DebugGraphics;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.DebugGame.Graphics {
    public class DebugFieldRenderer : IFieldRenderer {
        public DebugGraphicsContext Context { get; init; }
        public FieldManager Field { get; init; }
        public bool IsActive {
            get {
                activeMutex.WaitOne();
                bool isActive = activeWindows.Count > 0;
                activeMutex.ReleaseMutex();
                return isActive;
            }
        }

        private HashSet<DebugFieldWindow> activeWindows = [];
        private Mutex activeMutex = new();

        public DebugFieldRenderer(DebugGraphicsContext context, FieldManager field) {
            Context = context;
            Field = field;
        }

        public void Update() {
            if (!IsActive) {
                return;
            }

            if (!Context.HasFieldUpdated(Field)) {
                Context.FieldUpdated(Field);

                Field.Update();
            }
        }

        public void Render(double delta) {

        }

        public void CleanUp() {

        }

        public void AttachWindow(DebugFieldWindow window) {
            activeMutex.WaitOne();

            if (!activeWindows.Contains(window)) {
                activeWindows.Add(window);
            }

            activeMutex.ReleaseMutex();
        }

        public void DetachWindow(DebugFieldWindow window) {
            activeMutex.WaitOne();

            if (activeWindows.Contains(window)) {
                activeWindows.Remove(window);
            }

            activeMutex.ReleaseMutex();
        }
    }
}
