using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class EmotePacket {
    private enum Command : byte {
        Load = 0,
        Learn = 1,
        Error = 3,
    }

    public static ByteWriter Load(IList<Emote> emotes) {
        var pWriter = Packet.Of(SendOp.Emote);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteInt(emotes.Count);
        foreach (Emote emote in emotes) {
            pWriter.Write<Emote>(emote);
        }

        return pWriter;
    }

    public static ByteWriter Learn(Emote emote) {
        var pWriter = Packet.Of(SendOp.Emote);
        pWriter.Write<Command>(Command.Learn);
        pWriter.Write<Emote>(emote);

        return pWriter;
    }

    public static ByteWriter Error(EmoteError error) {
        var pWriter = Packet.Of(SendOp.Emote);
        pWriter.Write<Command>(Command.Error);
        pWriter.Write<EmoteError>(error);

        return pWriter;
    }
}
