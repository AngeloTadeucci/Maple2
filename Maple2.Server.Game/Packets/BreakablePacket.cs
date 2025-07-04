using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;

namespace Maple2.Server.Game.Packets;

public static class BreakablePacket {
    private enum Command : byte {
        BatchUpdate = 0,
        Update = 1,
    }

    public static ByteWriter Update(ICollection<FieldBreakable> breakables) {
        var pWriter = Packet.Of(SendOp.Breakable);
        pWriter.Write<Command>(Command.BatchUpdate);

        long currentTick = Environment.TickCount64;
        pWriter.WriteInt(breakables.Count);
        foreach (FieldBreakable breakable in breakables) {
            pWriter.WriteString(breakable.EntityId);
            pWriter.Write<BreakableState>(breakable.State);
            pWriter.WriteBool(breakable.Visible);
            if (breakable.BaseTick > 0) {
                pWriter.WriteInt((int) (currentTick - breakable.BaseTick));
                pWriter.WriteInt((int) breakable.BaseTick);
            } else {
                pWriter.WriteInt();
                pWriter.WriteInt();
            }
        }

        return pWriter;
    }

    public static ByteWriter Update(FieldBreakable breakable) {
        var pWriter = Packet.Of(SendOp.Breakable);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteString(breakable.EntityId);
        pWriter.Write<BreakableState>(breakable.State);
        pWriter.WriteBool(breakable.Visible);
        if (breakable.BaseTick > 0) {
            pWriter.WriteInt((int) (Environment.TickCount64 - breakable.BaseTick));
            pWriter.WriteInt((int) breakable.BaseTick);
        } else {
            pWriter.WriteInt();
            pWriter.WriteInt();
        }

        return pWriter;
    }
}
