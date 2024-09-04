using System.Runtime.InteropServices;
using Maple2.Model.Enum;

namespace Maple2.Model.Game;

[StructLayout(LayoutKind.Sequential, Size = 6)]
public readonly struct FieldInstance {
    public readonly bool BlockChangeChannel;
    public readonly InstanceType InstanceType;
    public readonly int InstanceId;

    public FieldInstance(bool blockChangeChannel, InstanceType instanceType, int instanceId) {
        BlockChangeChannel = blockChangeChannel;
        InstanceType = instanceType;
        InstanceId = instanceId;
    }
}
