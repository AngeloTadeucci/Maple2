using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class WeddingBillboardPacket {
    private enum Command : byte {
        Load = 4,
    }

    public static ByteWriter Load(IList<WeddingHall> halls) {
        var pWriter = Packet.Of(SendOp.WeddingBillboard);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(halls.Count);
        foreach (WeddingHall hall in halls) {
            pWriter.WriteLong(hall.MarriageId);
            pWriter.WriteClass<WeddingHall>(hall);
        }

        return pWriter;
    }
}
