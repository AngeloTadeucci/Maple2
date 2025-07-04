using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class SetCraftModePacket {
    private enum Command : byte {
        Stop = 0,
        Plot = 1,
        Liftable = 2,
    }

    public static ByteWriter Stop(int playerObjectId) {
        var pWriter = Packet.Of(SendOp.SetCraftMode);
        pWriter.WriteInt(playerObjectId);
        pWriter.Write<Command>(Command.Stop);

        return pWriter;
    }

    public static ByteWriter Plot(int playerObjectId, HeldCube cube) {
        var pWriter = Packet.Of(SendOp.SetCraftMode);
        pWriter.WriteInt(playerObjectId);
        pWriter.Write<Command>(Command.Plot);
        pWriter.WriteClass<HeldCube>(cube);
        pWriter.WriteInt();

        return pWriter;
    }

    public static ByteWriter Liftable(int playerObjectId, HeldCube cube) {
        var pWriter = Packet.Of(SendOp.SetCraftMode);
        pWriter.WriteInt(playerObjectId);
        pWriter.Write<Command>(Command.Liftable);
        pWriter.WriteClass<HeldCube>(cube);
        pWriter.WriteInt(2);

        return pWriter;
    }
}
