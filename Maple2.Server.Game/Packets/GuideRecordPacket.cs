using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class GuideRecordPacket {

    public static ByteWriter Load(IDictionary<int, int> records) {
        var pWriter = Packet.Of(SendOp.GuideRecord);
        pWriter.WriteInt(records.Count);
        foreach ((int recordId, int step) in records) {
            pWriter.WriteInt(recordId);
            pWriter.WriteInt(step);
        }
        pWriter.WriteByte();

        return pWriter;
    }
}
