using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class SuperChatPacket {
    private enum Command : byte {
        Select = 0,
        Deselect = 1,
    }

    public static ByteWriter Select(int objectId, int itemId) {
        var pWriter = Packet.Of(SendOp.SuperWorldChat);
        pWriter.Write<Command>(Command.Select);
        pWriter.WriteInt(objectId);
        pWriter.WriteInt(itemId);

        return pWriter;
    }

    public static ByteWriter Deselect(int objectId) {
        var pWriter = Packet.Of(SendOp.SuperWorldChat);
        pWriter.Write<Command>(Command.Deselect);
        pWriter.WriteInt(objectId);

        return pWriter;
    }
}
