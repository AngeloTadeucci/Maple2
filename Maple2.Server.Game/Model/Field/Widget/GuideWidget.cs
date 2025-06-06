﻿using System.Collections.Concurrent;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model.Widget;

public class GuideWidget : Widget {

    public GuideWidget(FieldManager field) : base(field) {
        Conditions = new ConcurrentDictionary<string, int>();
    }

    public override bool Check(string name, string arg) {
        return int.TryParse(arg, out int value) && Conditions.GetValueOrDefault(name) == value;
    }

    public override void Action(string function, int numericArg, string stringArg) {
    }
}
