using System.Collections.Concurrent;
using Maple2.Trigger.Enum;

namespace Maple2.Server.Game.Model;

public class Widget(WidgetType type) {
    public readonly WidgetType Type = type;
    public readonly ConcurrentDictionary<string, string> Conditions = new();

}
