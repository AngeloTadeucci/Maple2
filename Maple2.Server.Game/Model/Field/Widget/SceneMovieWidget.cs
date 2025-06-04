using System.Collections.Concurrent;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model.Widget;

public class SceneMovieWidget : Widget {

    public SceneMovieWidget(FieldManager field) : base(field) {
        Conditions = new ConcurrentDictionary<string, int>();
    }

    public override bool Check(string name, string arg) {
        return Conditions.GetValueOrDefault(name) == int.Parse(arg);
    }

    public override void Action(string function, int numericArg, string stringArg) {
        switch (function) {
            case "Clear":
                Clear();
                break;
        }
    }

    private void Clear() {
        Conditions.Clear();
    }
}
