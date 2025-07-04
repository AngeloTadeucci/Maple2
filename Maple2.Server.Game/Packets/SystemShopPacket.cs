using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class SystemShopPacket {
    private enum Command : byte {
        Arena = 3,
        Fishing = 4,
        Mentee = 6,
        Mentor = 7,
        Item = 10,
    }

    public static ByteWriter Arena() {
        var pWriter = Packet.Of(SendOp.SystemShop);
        pWriter.Write<Command>(Command.Arena);
        pWriter.WriteBool(true);

        return pWriter;
    }

    public static ByteWriter Fishing() {
        var pWriter = Packet.Of(SendOp.SystemShop);
        pWriter.Write<Command>(Command.Fishing);
        pWriter.WriteBool(true);

        return pWriter;
    }

    public static ByteWriter Mentee() {
        var pWriter = Packet.Of(SendOp.SystemShop);
        pWriter.Write<Command>(Command.Mentee);
        pWriter.WriteBool(true);

        return pWriter;
    }

    public static ByteWriter Mentor() {
        var pWriter = Packet.Of(SendOp.SystemShop);
        pWriter.Write<Command>(Command.Mentor);
        pWriter.WriteBool(true);

        return pWriter;
    }

    public static ByteWriter Item() {
        var pWriter = Packet.Of(SendOp.SystemShop);
        pWriter.Write<Command>(Command.Item);
        pWriter.WriteBool(true);

        return pWriter;
    }
}
