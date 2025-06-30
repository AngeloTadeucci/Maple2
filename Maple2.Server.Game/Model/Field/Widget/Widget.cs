using System.Collections.Concurrent;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model.Widget;

public class Widget : IWidget {
    public FieldManager Field { get; }
    public ConcurrentDictionary<string, int> Conditions { get; set; }
    public virtual void Action(string function, int numericArg, string stringArg) {
    }
    public virtual bool Check(string name, string arg) {
        return false;
    }

    public Widget(FieldManager field) {
        Field = field;
        Conditions = new ConcurrentDictionary<string, int>();
    }
}
