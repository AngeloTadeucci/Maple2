using System.Collections.Concurrent;
using System.Numerics;
using Maple2.PacketLib.Tools;
using Maple2.Server.Game.Manager.Field;
using Maple2.Tools;
using Serilog;

namespace Maple2.Server.Game.Model.Widget;

public class SurvivalContentsWidget : Widget, IByteSerializable {
    private Vector3 StormCenter;
    private Vector3 SafeZoneCenter;

    public SurvivalContentsWidget(FieldManager field) : base(field) {
        Conditions = new ConcurrentDictionary<string, int>();
        StormCenter = new Vector3(0, 0, 0);
        SafeZoneCenter = new Vector3(0, 0, 0);
    }

    public override void Action(string function, int numericArg, string stringArg) {
        switch (function) {
            case "StormData":
                StormData(stringArg);
                break;
            case "EnterStep":
                EnterStep(stringArg);
                break;
            case "ExitStep":
                ExitStep(stringArg);
                break;
            default:
                Log.Logger.Warning($"Unknown function called on SurvivalContentsWidget: {function}");
                break;
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
