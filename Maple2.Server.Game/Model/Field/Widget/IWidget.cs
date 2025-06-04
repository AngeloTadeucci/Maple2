using System.Collections.Concurrent;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model.Widget;

public interface IWidget {
    public FieldManager Field { get; }
    public ConcurrentDictionary<string, int> Conditions { get; set; }

    /// <summary>
    /// Performs a specific action on the widget based on the provided function and arguments.
    /// </summary>
    /// <param name="function">The name of the action to perform.</param>
    /// <param name="numericArg">A numeric argument for the action.</param>
    /// <param name="stringArg">A string argument for the action.</param>
    void Action(string function, int numericArg, string stringArg);
    bool Check(string name, string arg);
}
