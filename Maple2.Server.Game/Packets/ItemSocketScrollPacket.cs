using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class ItemSocketScrollPacket {
    private enum Command : byte {
        UseScroll = 0,
        Unlock = 2,
        Error = 3,
    }

    public static ByteWriter UseScroll(Item scroll, ItemSocketScrollMetadata metadata) {
        var pWriter = Packet.Of(SendOp.ItemSocketScroll);
        pWriter.Write<Command>(Command.UseScroll);
        pWriter.WriteLong(scroll.Uid);
        pWriter.WriteInt(10000); // div 100 = Rate
        pWriter.WriteByte(metadata.SocketCount);

        return pWriter;
    }

    public static ByteWriter Unlock(Item item, bool success) {
        var pWriter = Packet.Of(SendOp.ItemSocketScroll);
        pWriter.Write<Command>(Command.Unlock);
        pWriter.WriteBool(success);
        pWriter.WriteLong(item.Uid);
        pWriter.WriteByte();
        pWriter.WriteInt(10000);
        pWriter.WriteClass<ItemSocket>(item.Socket ?? ItemSocket.Default);

        return pWriter;
    }

    public static ByteWriter Error(ItemSocketScrollError error) {
        var pWriter = Packet.Of(SendOp.ItemSocketScroll);
        pWriter.Write<Command>(Command.Error);
        pWriter.WriteBool(false); // no idea
        pWriter.Write<ItemSocketScrollError>(error);

        return pWriter;
    }
}
