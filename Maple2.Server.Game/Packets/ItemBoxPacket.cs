using Maple2.Model.Error;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class ItemBoxPacket {

    public static ByteWriter Open(int sourceItemId, int amount, ItemBoxError error = ItemBoxError.ok) {
        var pWriter = Packet.Of(SendOp.ItemBox);
        pWriter.WriteInt(sourceItemId);
        pWriter.WriteInt(amount);
        pWriter.Write<ItemBoxError>(error);

        return pWriter;
    }
}
