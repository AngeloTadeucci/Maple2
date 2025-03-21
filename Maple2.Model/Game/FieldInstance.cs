using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public sealed class FieldInstance : IByteSerializable {
    public static FieldInstance Default = new(InstanceType.none, 0);

    public readonly InstanceType InstanceType;
    public readonly int InstanceId;

    public FieldInstance(InstanceType instanceType, int instanceId) {
        InstanceType = instanceType;
        InstanceId = instanceId;
    }

    public void WriteTo(IByteWriter writer) {
        writer.Write<InstanceType>(InstanceType);
        writer.WriteInt(InstanceId);
    }
}
