namespace Maple2.Server.Game.Model;

public class TickTimer {
    public int StartTick { get; private set; }
    public readonly int Duration;
    public readonly bool AutoRemove;
    public readonly int VerticalOffset;
    public readonly string Type;
    public readonly bool Display;

    public TickTimer(int duration, bool autoRemove = true, int vOffset = 0, bool display = false, string type = "") {
        StartTick = Environment.TickCount;
        Duration = duration;
        AutoRemove = autoRemove;
        VerticalOffset = vOffset;
        Type = type;
        Display = display;
    }

    public void Reset() {
        StartTick = Environment.TickCount;
    }

    public bool Expired() => Environment.TickCount - StartTick > Duration;
}
