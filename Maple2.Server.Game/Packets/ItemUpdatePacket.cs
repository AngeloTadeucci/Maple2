using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class ItemUpdatePacket {
    public static ByteWriter Update(IActor<Player> player, Item item) {
        var pWriter = Packet.Of(SendOp.ItemUpdate);
        pWriter.WriteInt(player.ObjectId);
        pWriter.WriteLong(item.Uid);
        pWriter.WriteClass<Item>(item);
        return pWriter;
    }
}
