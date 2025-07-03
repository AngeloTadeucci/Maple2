using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class ItemLockPacket {
    private enum Command : byte {
        Stage = 1,
        Unstage = 2,
        Commit = 3,
        Error = 4,
    }

    public static ByteWriter Stage(long itemUid, short slot) {
        var pWriter = Packet.Of(SendOp.ItemLock);
        pWriter.Write<Command>(Command.Stage);
        pWriter.WriteLong(itemUid);
        pWriter.WriteShort(slot);

        return pWriter;
    }

    public static ByteWriter Unstage(long itemUid) {
        var pWriter = Packet.Of(SendOp.ItemLock);
        pWriter.Write<Command>(Command.Unstage);
        pWriter.WriteLong(itemUid);

        return pWriter;
    }

    public static ByteWriter Commit(ICollection<Item> items) {
        var pWriter = Packet.Of(SendOp.ItemLock);
        pWriter.Write<Command>(Command.Commit);
        pWriter.WriteByte((byte) items.Count);

        foreach (Item item in items) {
            pWriter.WriteLong(item.Uid);
            pWriter.WriteClass<Item>(item);
        }

        return pWriter;
    }

    // 1: s_itemlock_unknown_err, "System Error: Item. code = {0}"
    // 3: s_err_inventory, "Your inventory is full."
    // default: s_itemlock_unknown_err, "System Error: Item. code = {0}"
    public static ByteWriter Error(int errorCode = 3) {
        var pWriter = Packet.Of(SendOp.ItemLock);
        pWriter.Write<Command>(Command.Error);
        pWriter.WriteInt(errorCode);

        return pWriter;
    }
}
