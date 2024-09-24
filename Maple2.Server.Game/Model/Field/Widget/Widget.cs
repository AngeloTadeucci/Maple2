using System.Collections.Concurrent;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model.Widget;

public class Widget : IWidget {
    public FieldManager Field { get; }
    public ConcurrentDictionary<string, int> Conditions { get; set; }
    public virtual void Action(string function, int arg, string arg2) {
    }

    public Widget(FieldManager field) {
        Field = field;
        Conditions = new ConcurrentDictionary<string, int>();
    }
}
