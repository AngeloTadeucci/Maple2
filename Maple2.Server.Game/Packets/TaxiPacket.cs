using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class TaxiPacket {
    public static ByteWriter RevealTaxi(params int[] taxis) {
        var pWriter = Packet.Of(SendOp.Taxi);
        pWriter.WriteInt(taxis.Length);
        foreach (int taxi in taxis) {
            pWriter.WriteInt(taxi);
        }
        pWriter.WriteBool(true); // s_reveal_taxi_station

        return pWriter;
    }
}
