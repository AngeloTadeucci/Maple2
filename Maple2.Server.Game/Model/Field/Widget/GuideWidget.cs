using System.Collections.Concurrent;
using System.Reflection;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model.Widget;

public class GuideWidget : Widget {

    public GuideWidget(FieldManager field) : base(field) {
        Conditions = new ConcurrentDictionary<string, int>();
    }

    public override bool Check(string name, string arg) {
        return Conditions.GetValueOrDefault(name) == 1;
    }

    public override void Action(string function, int numericArg, string stringArg) {
        MethodInfo? method = GetType().GetMethod(function, BindingFlags.NonPublic | BindingFlags.Instance);
        if (method != null) {
            method.Invoke(this, [stringArg, numericArg]);
        }
    }
}
