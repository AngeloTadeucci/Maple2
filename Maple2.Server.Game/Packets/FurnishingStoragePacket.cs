using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class FurnishingStoragePacket {
    private enum Command : byte {
        StartList = 1,
        Count = 2,
        Add = 3,
        Remove = 4,
        Purchase = 5,
        Update = 7,
        EndList = 8,
    }

    public static ByteWriter StartList() {
        var pWriter = Packet.Of(SendOp.FurnishingStorage);
        pWriter.Write<Command>(Command.StartList);

        return pWriter;
    }

    public static ByteWriter Count(int count) {
        var pWriter = Packet.Of(SendOp.FurnishingStorage);
        pWriter.Write<Command>(Command.Count);
        pWriter.WriteInt(count);

        return pWriter;
    }

    public static ByteWriter Add(Item item) {
        var pWriter = Packet.Of(SendOp.FurnishingStorage);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteInt(item.Id);
        pWriter.WriteLong(item.Uid);
        pWriter.WriteByte((byte) item.Rarity);
        pWriter.WriteInt(item.Slot);
        pWriter.WriteClass<Item>(item);

        return pWriter;
    }

    public static ByteWriter Remove(long itemUid) {
        var pWriter = Packet.Of(SendOp.FurnishingStorage);
        pWriter.Write<Command>(Command.Remove);
        pWriter.WriteLong(itemUid);

        return pWriter;
    }

    public static ByteWriter Purchase(long itemUid, int amount) {
        var pWriter = Packet.Of(SendOp.FurnishingStorage);
        pWriter.Write<Command>(Command.Purchase);
        pWriter.WriteLong(itemUid);
        pWriter.WriteInt(amount);

        return pWriter;
    }

    public static ByteWriter Update(long itemUid, int amount) {
        var pWriter = Packet.Of(SendOp.FurnishingStorage);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteLong(itemUid);
        pWriter.WriteInt(amount);

        return pWriter;
    }

    public static ByteWriter EndList() {
        var pWriter = Packet.Of(SendOp.FurnishingStorage);
        pWriter.Write<Command>(Command.EndList);

        return pWriter;
    }
}
