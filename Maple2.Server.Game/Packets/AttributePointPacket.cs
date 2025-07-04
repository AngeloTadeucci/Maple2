using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Manager.Config;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class AttributePointPacket {
    private enum Command : byte {
        Sources = 0,
        Allocation = 1,
    }

    public static ByteWriter Sources(StatAttributes statAttributes) {
        var pWriter = Packet.Of(SendOp.AttributePoint);
        pWriter.Write<Command>(Command.Sources);
        pWriter.WriteInt(statAttributes.TotalPoints);
        pWriter.WriteClass<StatAttributes.PointSources>(statAttributes.Sources);

        return pWriter;
    }

    public static ByteWriter Allocation(StatAttributes statAttributes) {
        var pWriter = Packet.Of(SendOp.AttributePoint);
        pWriter.Write<Command>(Command.Allocation);
        pWriter.WriteInt(statAttributes.TotalPoints);
        pWriter.WriteClass<StatAttributes.PointAllocation>(statAttributes.Allocation);

        return pWriter;
    }
}
