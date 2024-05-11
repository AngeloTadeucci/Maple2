using System.Collections.Concurrent;
using Maple2.Trigger.Enum;

namespace Maple2.Server.Game.Model;

public class Widget {
    public readonly WidgetType Type;
    public readonly ConcurrentDictionary<string, string> Conditions;

    public Widget(WidgetType type) {
        Type = type;
        Conditions = new ConcurrentDictionary<string, string>();
    }
}
