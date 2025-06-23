using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Core.Packets;

public static class NoticePacket {
    [Flags]
    public enum Flags : short {
        Message = 1,
        Alert = 4,
        Mint = 16,
        MessageBox = 64,
        Disconnect = 128, // Disconnect after OK.
        LargeAlert = 512,
        Banner = 1024,
    }

    private enum Command : byte {
        Notice = 4,
        Disconnect = 5,
    }

    public static ByteWriter Message(string message, bool htmlEncoded = false) {
        var text = new InterfaceText(message, htmlEncoded);
        var pWriter = Packet.Of(SendOp.Notice);
        pWriter.Write<Command>(Command.Notice);
        pWriter.Write<Flags>(Flags.Message);
        pWriter.WriteClass<InterfaceText>(text);

        return pWriter;
    }

    public static ByteWriter Message(InterfaceText text, Flags flags = Flags.Message) {
        var pWriter = Packet.Of(SendOp.Notice);
        pWriter.Write<Command>(Command.Notice);
        pWriter.Write<Flags>(flags);
        pWriter.WriteClass<InterfaceText>(text);

        return pWriter;
    }

    public static ByteWriter MessageBox(InterfaceText text) {
        var pWriter = Packet.Of(SendOp.Notice);
        pWriter.Write<Command>(Command.Notice);
        pWriter.Write<Flags>(Flags.MessageBox);
        pWriter.WriteClass<InterfaceText>(text);

        return pWriter;
    }

    public static ByteWriter Notice(Flags flags, InterfaceText text, short duration = 0) {
        var pWriter = Packet.Of(SendOp.Notice);
        pWriter.Write<Command>(Command.Notice);
        pWriter.Write<Flags>(flags);
        pWriter.WriteClass<InterfaceText>(text);
        if (flags.HasFlag(Flags.Mint)) {
            pWriter.WriteShort(duration);
        } else if (flags.HasFlag(Flags.LargeAlert)) {
            pWriter.WriteInt(duration);
        }

        return pWriter;
    }

    // Disconnects user and displays a message box.
    public static ByteWriter Disconnect(InterfaceText text) {
        var pWriter = Packet.Of(SendOp.Notice);
        pWriter.Write<Command>(Command.Disconnect);
        pWriter.Write<Flags>(Flags.MessageBox);
        pWriter.WriteClass<InterfaceText>(text);

        return pWriter;
    }
}
