using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class QuizEventPacket {
    private enum Command : byte {
        Question = 0,
        Answer = 1,
    }

    public static ByteWriter Question(string category, string question, string answer, int duration) {
        var pWriter = Packet.Of(SendOp.QuizEvent);
        pWriter.Write<Command>(Command.Question);
        pWriter.WriteUnicodeString(category);
        pWriter.WriteUnicodeString(question);
        pWriter.WriteUnicodeString(answer);
        pWriter.WriteInt(duration);

        return pWriter;
    }

    public static ByteWriter Answer(bool isTrue, string answer, int duration) {
        var pWriter = Packet.Of(SendOp.QuizEvent);
        pWriter.Write<Command>(Command.Answer);
        pWriter.WriteBool(isTrue);
        pWriter.WriteUnicodeString(answer);
        pWriter.WriteInt(duration);

        return pWriter;
    }
}
