using System.Collections.Concurrent;

namespace Maple2.Server.Game.Model;

public class Widget {
    public readonly string Type;
    public readonly ConcurrentDictionary<string, int> Conditions;

    public Widget(string type) {
        Type = type;
        Conditions = new ConcurrentDictionary<string, int>();
    }
}
