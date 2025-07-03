using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class ItemDismantlePacket {
    private enum Command : byte {
        Stage = 1,
        Remove = 2,
        Result = 3,
        Preview = 5,
    }

    public static ByteWriter Stage(long itemUid, short slot, int amount) {
        var pWriter = Packet.Of(SendOp.ItemDismantle);
        pWriter.Write<Command>(Command.Stage);
        pWriter.WriteLong(itemUid);
        pWriter.WriteShort(slot);
        pWriter.WriteInt(amount);

        return pWriter;
    }

    public static ByteWriter Remove(long itemUid) {
        var pWriter = Packet.Of(SendOp.ItemDismantle);
        pWriter.Write<Command>(Command.Remove);
        pWriter.WriteLong(itemUid);

        return pWriter;
    }

    public static ByteWriter Result(IDictionary<int, int> rewards) {
        var pWriter = Packet.Of(SendOp.ItemDismantle);
        pWriter.Write<Command>(Command.Result);
        pWriter.WriteBool(true); // success?
        pWriter.WriteInt(rewards.Count);
        foreach ((int id, int amount) in rewards) {
            pWriter.WriteInt(id);
            pWriter.WriteInt(amount);
        }

        return pWriter;
    }

    public static ByteWriter Preview(IDictionary<int, (int Min, int Max)> rewards) {
        var pWriter = Packet.Of(SendOp.ItemDismantle);
        pWriter.Write<Command>(Command.Preview);
        pWriter.WriteInt(rewards.Count);
        foreach ((int id, (int min, int max)) in rewards) {
            pWriter.WriteInt(id);
            pWriter.WriteInt(min);
            pWriter.WriteInt(max);
        }

        return pWriter;
    }
}
