using System.Runtime.InteropServices;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Tools;

namespace Maple2.Model.Game;

public sealed class FieldInstance : IByteSerializable {
    public static FieldInstance Default = new(false, InstanceType.none, 0);

    public readonly bool BlockChangeChannel;
    public readonly InstanceType InstanceType;
    public readonly int InstanceId;

    public FieldInstance(bool blockChangeChannel, InstanceType instanceType, int instanceId) {
        BlockChangeChannel = blockChangeChannel;
        InstanceType = instanceType;
        InstanceId = instanceId;
    }

    public void WriteTo(IByteWriter writer) {
        writer.WriteBool(BlockChangeChannel);
        writer.Write<InstanceType>(InstanceType);
        writer.WriteInt(InstanceId);
    }
}
