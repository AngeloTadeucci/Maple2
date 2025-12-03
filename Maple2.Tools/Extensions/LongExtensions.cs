namespace Maple2.Tools.Extensions;

public static class LongExtensions {
    /// <summary>
    /// Truncates a long value to a 32-bit int, handling overflow properly.
    /// This is useful for converting Environment.TickCount64 to match Environment.TickCount behavior.
    /// </summary>
    /// <param name="value">The long value to truncate</param>
    /// <returns>The truncated int value</returns>
    public static int Truncate32(this long value) {
        return (int) (0xFFFFFFFF & value);
    }
}

