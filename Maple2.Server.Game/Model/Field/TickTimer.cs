namespace Maple2.Server.Game.Model;

public class TickTimer(int duration, bool autoRemove = true, int vOffset = 0, string type = "") {
    public int StartTick { get; private set; } = Environment.TickCount;
    public readonly int Duration = duration;
    public readonly bool AutoRemove = autoRemove;
    public readonly int VerticalOffset = vOffset;
    public readonly string Type = type;

    public void Reset() {
        StartTick = Environment.TickCount;
    }

    public bool Expired() => Environment.TickCount - StartTick > Duration;
}
