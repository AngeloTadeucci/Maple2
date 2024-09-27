using Maple2.Model.Common;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public class CubeSettings : IByteSerializable {
    public virtual CubeSettings Clone() {
        return (CubeSettings) MemberwiseClone();
    }
    public virtual void WriteTo(IByteWriter writer) { }
}

public sealed class CubePortalSettings : CubeSettings {
    public string PortalName { get; set; }
    public PortalActionType Method { get; set; }
    public CubePortalDestination Destination { get; set; }
    public string DestinationTarget { get; set; }
    public int PortalObjectId { get; set; }

    public override CubeSettings Clone() {
        return (CubePortalSettings) MemberwiseClone();
    }

    public CubePortalSettings() {
        PortalName = string.Empty;
        DestinationTarget = string.Empty;
    }

    public override void WriteTo(IByteWriter writer) {
        writer.WriteUnicodeString(PortalName);
        writer.WriteByte((byte) Method);
        writer.Write<CubePortalDestination>(Destination);
        writer.WriteUnicodeString(DestinationTarget);
    }

    public void SetName(Vector3B position) {
        PortalName = $"Portal_{Math.Abs(position.X):D2}.{Math.Abs(position.Y):D2}.{Math.Abs(position.Z):D2}";
    }
}
