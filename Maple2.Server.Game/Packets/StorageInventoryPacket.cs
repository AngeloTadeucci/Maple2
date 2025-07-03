using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class StorageInventoryPacket {
    private enum Command : byte {
        Add = 0,
        Remove = 1,
        Move = 2,
        UpdateMesos = 3,
        SlotsUsed = 4,
        Load = 5,
        Reload = 8,
        Update = 9,
        Reset = 11,
        SlotsExpanded = 13,
        OpenDialog = 14,
        Error = 16,
    }

    public static ByteWriter Add(Item item) {
        var pWriter = Packet.Of(SendOp.StorageInventory);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteLong(); // Unknown (0)
        pWriter.WriteInt(item.Id);
        pWriter.WriteLong(item.Uid);
        pWriter.WriteShort(item.Slot);
        pWriter.WriteInt(item.Rarity);
        pWriter.WriteClass<Item>(item);

        return pWriter;
    }

    public static ByteWriter Remove(long uid) {
        var pWriter = Packet.Of(SendOp.StorageInventory);
        pWriter.Write<Command>(Command.Remove);
        pWriter.WriteLong(); // Unknown (0)
        pWriter.WriteLong(uid);

        return pWriter;
    }

    public static ByteWriter Move(long dstUid, short srcSlot, long srcUid, short dstSlot) {
        var pWriter = Packet.Of(SendOp.StorageInventory);
        pWriter.Write<Command>(Command.Move);
        pWriter.WriteLong(); // Unknown (0)
        pWriter.WriteLong(dstUid);
        pWriter.WriteShort(srcSlot);
        pWriter.WriteLong(srcUid);
        pWriter.WriteShort(dstSlot);

        return pWriter;
    }

    public static ByteWriter UpdateMesos(long mesos) {
        var pWriter = Packet.Of(SendOp.StorageInventory);
        pWriter.Write<Command>(Command.UpdateMesos);
        pWriter.WriteLong(mesos);

        return pWriter;
    }

    public static ByteWriter SlotsUsed(short slotsUsed) {
        var pWriter = Packet.Of(SendOp.StorageInventory);
        pWriter.Write<Command>(Command.SlotsUsed);
        pWriter.WriteLong(); // Unknown (0)
        pWriter.WriteShort(slotsUsed);

        return pWriter;
    }

    public static ByteWriter Load(ICollection<Item> items) {
        var pWriter = Packet.Of(SendOp.StorageInventory);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteLong(); // Unknown (0)
        pWriter.WriteShort((short) items.Count);
        foreach (Item item in items) {
            pWriter.WriteInt(item.Id);
            pWriter.WriteLong(item.Uid);
            pWriter.WriteShort(item.Slot);
            pWriter.WriteInt(item.Rarity);
            pWriter.WriteClass<Item>(item);
        }

        return pWriter;
    }

    // Used for sorting
    public static ByteWriter Reload(ICollection<Item> items) {
        var pWriter = Packet.Of(SendOp.StorageInventory);
        pWriter.Write<Command>(Command.Reload);
        pWriter.WriteLong(); // Unknown (0)
        pWriter.WriteShort((short) items.Count);
        foreach (Item item in items) {
            pWriter.WriteInt(item.Id);
            pWriter.WriteLong(item.Uid);
            pWriter.WriteShort(item.Slot);
            pWriter.WriteInt(item.Rarity);
            pWriter.WriteClass<Item>(item);
        }

        return pWriter;
    }

    public static ByteWriter Update(long uid, int remaining) {
        var pWriter = Packet.Of(SendOp.StorageInventory);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteLong(); // Unknown (0)
        pWriter.WriteLong(uid);
        pWriter.WriteInt(remaining);

        return pWriter;
    }

    public static ByteWriter Reset() {
        var pWriter = Packet.Of(SendOp.StorageInventory);
        pWriter.Write<Command>(Command.Reset);

        return pWriter;
    }

    public static ByteWriter SlotsExpanded(int expansion) {
        var pWriter = Packet.Of(SendOp.StorageInventory);
        pWriter.Write<Command>(Command.SlotsExpanded);
        pWriter.WriteInt(expansion);

        return pWriter;
    }

    public static ByteWriter OpenDialog() {
        var pWriter = Packet.Of(SendOp.StorageInventory);
        pWriter.Write<Command>(Command.OpenDialog);

        return pWriter;
    }

    public static ByteWriter Error(StorageInventoryError error) {
        var pWriter = Packet.Of(SendOp.StorageInventory);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<StorageInventoryError>(error);

        return pWriter;
    }
}
