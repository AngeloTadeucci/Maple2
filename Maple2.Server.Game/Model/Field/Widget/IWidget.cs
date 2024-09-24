using System.Collections.Concurrent;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model.Widget;

public interface IWidget {
    public FieldManager Field { get; }
    public ConcurrentDictionary<string, int> Conditions { get; set; }

    void Action(string function, int arg, string arg2);
}
