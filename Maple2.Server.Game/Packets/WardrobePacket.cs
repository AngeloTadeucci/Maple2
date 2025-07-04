using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class WardrobePacket {
    private enum Command : byte {
        Load = 5,
    }

    public static ByteWriter Load(int index, Wardrobe wardrobe) {
        var pWriter = Packet.Of(SendOp.Wardrobe);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(index);
        pWriter.WriteClass<Wardrobe>(wardrobe);

        return pWriter;
    }
}
