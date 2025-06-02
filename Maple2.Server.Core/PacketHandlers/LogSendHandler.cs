using System.Text;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Helpers;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Network;

namespace Maple2.Server.Core.PacketHandlers;

// Note: socket_exception debug offset includes +6 bytes from encrypted header
public abstract class LogSendHandler<T> : PacketHandler<T> where T : Session {
    public override RecvOp OpCode => RecvOp.LogSend;

    private enum Command : byte {
        Log = 0,
        Metric = 1,
    }

    public override void Handle(T session, IByteReader packet) {
        byte unknown = packet.ReadByte();
        if (unknown != 0) {
            return;
        }

        var command = packet.Read<Command>();
        switch (command) {
            case Command.Log:
                var builder = new StringBuilder();
                string message = packet.ReadUnicodeString();
                builder.Append(message);
                if (message.Contains("exception")) {
                    // Read remaining string
                    string debug = packet.ReadUnicodeString();
                    try {
                        SockExceptionInfo exceptionInfo = ErrorParserHelper.Parse(debug);
                        byte[]? lastPacket = session.GetLastSentPacket(exceptionInfo.SendOp);

                        Logger.Error("[{message}] [SendOp: {sendOp}] [Offset: {offset}] [Hint: {hint}] [LastPacket:\n{packetDump}]",
                            message, exceptionInfo.SendOp, exceptionInfo.Offset, exceptionInfo.Hint, lastPacket?.ToHexString(lastPacket.Length, ' ') ?? "No packet found");
                    } catch (ArgumentException) {
                        Logger.Error("[{Message}] {Debug}", message, debug);
                    }

                    session.OnError?.Invoke(session, debug);
                    return;
                } else {
                    message = packet.ReadUnicodeString();
                    builder.Append(message);
                }

                Logger.Information("Client Log: {Builder}", builder);
                break;
            case Command.Metric:
                byte count = packet.ReadByte();
                for (byte i = 0; i < count; i++) {
                    packet.ReadString(); // StatisticType
                    packet.Read<float>(); // Average
                    packet.Read<float>(); // Standard Deviation
                    packet.Read<int>(); // Data Points
                    packet.Read<float>(); // Min
                    packet.Read<float>(); // Max
                }
                break;
        }
    }
}
