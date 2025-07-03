using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class ItemRepackPacket {
    private enum Command : byte {
        Open = 0,
        Commit = 2,
        Error = 3,
    }

    public static ByteWriter Open(long itemUid) {
        var pWriter = Packet.Of(SendOp.ItemRepackage);
        pWriter.Write<Command>(Command.Open);
        pWriter.WriteLong(itemUid);

        return pWriter;
    }

    public static ByteWriter Commit(Item item) {
        var pWriter = Packet.Of(SendOp.ItemRepackage);
        pWriter.Write<Command>(Command.Commit);
        pWriter.WriteShort();
        pWriter.WriteLong(item.Uid);
        pWriter.WriteClass<Item>(item);

        return pWriter;
    }

    public static ByteWriter Error(ItemRepackError error) {
        var pWriter = Packet.Of(SendOp.ItemRepackage);
        pWriter.Write<Command>(Command.Error);
        pWriter.WriteByte();
        pWriter.Write<ItemRepackError>(error);

        return pWriter;
    }
}
