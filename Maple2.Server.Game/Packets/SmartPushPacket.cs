using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class SmartPushPacket {
    private enum Command : byte {
        Activate = 1,
    }

    public static ByteWriter ActivateEffect(SmartPushCurrencyType currencyType, int buffId) {
        var pWriter = Packet.Of(SendOp.SmartPush);
        pWriter.Write<Command>(Command.Activate);
        pWriter.Write<SmartPushCurrencyType>(currencyType);
        pWriter.WriteInt(buffId);

        return pWriter;
    }

    public static ByteWriter ActivateGather(int id, int amount) {
        var pWriter = Packet.Of(SendOp.SmartPush);
        pWriter.Write<Command>(Command.Activate);
        pWriter.WriteInt(id);
        pWriter.WriteInt(amount);

        return pWriter;
    }
}
