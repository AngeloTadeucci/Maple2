using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Model;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class BuffPacket {
    private enum Command : byte {
        Add = 0,
        Remove = 1,
        Update = 2,
    }

    public static ByteWriter Add(Buff buff) {
        var pWriter = Packet.Of(SendOp.Buff);
        pWriter.Write<Command>(Command.Add);
        pWriter.WriteInt(buff.Owner.ObjectId);
        pWriter.WriteInt(buff.ObjectId);
        pWriter.WriteInt(buff.Caster.ObjectId);
        pWriter.WriteClass<Buff>(buff);

        return pWriter;
    }

    public static ByteWriter Remove(Buff buff) {
        var pWriter = Packet.Of(SendOp.Buff);
        pWriter.Write<Command>(Command.Remove);
        pWriter.WriteInt(buff.Owner.ObjectId);
        pWriter.WriteInt(buff.ObjectId);
        pWriter.WriteInt(buff.Caster.ObjectId);

        return pWriter;
    }

    public static ByteWriter Update(Buff buff, BuffFlag flag = BuffFlag.UpdateBuff | BuffFlag.UpdateShield) {
        var pWriter = Packet.Of(SendOp.Buff);
        pWriter.Write<Command>(Command.Update);
        pWriter.WriteInt(buff.Owner.ObjectId);
        pWriter.WriteInt(buff.ObjectId);
        pWriter.WriteInt(buff.Caster.ObjectId);
        pWriter.Write<BuffFlag>(flag);
        if (flag.HasFlag(BuffFlag.UpdateBuff)) {
            buff.WriteAdditionalEffect(pWriter);
        }
        if (flag.HasFlag(BuffFlag.UpdateShield)) {
            buff.WriteShieldHealth(pWriter);
        }

        return pWriter;
    }
}
