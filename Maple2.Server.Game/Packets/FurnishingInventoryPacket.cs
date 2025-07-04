using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class FurnishingInventoryPacket {
    private enum Command : byte {
        StartList = 0,
        Add = 1,
        Remove = 2,
        Update = 3,
        EndList = 4,
    }

    public static ByteWriter StartList() {
        var pWriter = Packet.Of(SendOp.FurnishingInventory);
        pWriter.Write<Command>(Command.StartList);

        return pWriter;
    }

    public static ByteWriter Add(PlotCube cube) {
        var pWriter = Packet.Of(SendOp.FurnishingInventory);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteClass<PlotCube>(cube);

        return pWriter;
    }

    public static ByteWriter Remove(long itemUid) {
        var pWriter = Packet.Of(SendOp.FurnishingInventory);
        pWriter.Write<Command>(Command.Remove);
        pWriter.WriteLong(itemUid);

        return pWriter;
    }

    public static ByteWriter Update(long itemUid, int amount) {
        var pWriter = Packet.Of(SendOp.FurnishingInventory);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteLong(itemUid);
        pWriter.WriteInt(amount);

        return pWriter;
    }

    public static ByteWriter EndList() {
        var pWriter = Packet.Of(SendOp.FurnishingInventory);
        pWriter.Write<Command>(Command.EndList);

        return pWriter;
    }
}
