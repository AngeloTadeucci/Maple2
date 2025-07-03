using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class LapenshardPacket {
    private enum Command : byte {
        Load = 0,
        Equip = 1,
        Unequip = 2,
        Preview = 4,
        Upgrade = 5,
    }

    public static ByteWriter Load(IDictionary<LapenshardSlot, int> items) {
        var pWriter = Packet.Of(SendOp.Lapenshard);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(items.Count);
        foreach ((LapenshardSlot slot, int id) in items) {
            pWriter.Write<LapenshardSlot>(slot);
            pWriter.WriteInt(id);
        }

        return pWriter;
    }

    public static ByteWriter Equip(LapenshardSlot slot, int id) {
        var pWriter = Packet.Of(SendOp.Lapenshard);
        pWriter.Write<Command>(Command.Equip);
        pWriter.Write<LapenshardSlot>(slot);
        pWriter.WriteInt(id);

        return pWriter;
    }

    public static ByteWriter Unequip(LapenshardSlot slot) {
        var pWriter = Packet.Of(SendOp.Lapenshard);
        pWriter.Write<Command>(Command.Unequip);
        pWriter.Write<LapenshardSlot>(slot);

        return pWriter;
    }

    public static ByteWriter Preview() {
        var pWriter = Packet.Of(SendOp.Lapenshard);
        pWriter.Write<Command>(Command.Preview);
        pWriter.WriteInt(10000); // div 100 = Rate

        return pWriter;
    }

    public static ByteWriter Upgrade(long uid, int id, LapenshardSlot slot, bool success = true) {
        var pWriter = Packet.Of(SendOp.Lapenshard);
        pWriter.Write<Command>(Command.Upgrade);
        pWriter.WriteLong(uid);
        pWriter.WriteInt(id);
        pWriter.Write<LapenshardSlot>(slot);
        pWriter.WriteBool(success);

        return pWriter;
    }
}
