using System.Collections.Concurrent;
using System.Numerics;
using System.Reflection;
using Maple2.PacketLib.Tools;
using Maple2.Server.Game.Manager.Field;
using Maple2.Tools;

namespace Maple2.Server.Game.Model.Widget;

public class SurvivalContentsWidget : Widget, IByteSerializable {
    private Vector3 StormCenter;
    private Vector3 SafeZoneCenter;

    public SurvivalContentsWidget(FieldManager field) : base(field) {
        Conditions = new ConcurrentDictionary<string, int>();
    }

    public override void Action(string function, int numericArg, string stringArg) {
        MethodInfo? method = GetType().GetMethod(function, BindingFlags.NonPublic | BindingFlags.Instance);
        if (method != null) {
            method.Invoke(this, [stringArg, numericArg]);
        }
    }

    private void StormData(string step) {

    }

    private void EnterStep(string step) {
    }

    private void ExitStep(string step) {

    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteShort(500);
        writer.WriteShort(3000); // radius
        writer.WriteShort(1000);
        writer.Write<Vector3>(StormCenter);
        writer.WriteShort(1000); // radius of center
        writer.Write<Vector3>(SafeZoneCenter);
    }
}
