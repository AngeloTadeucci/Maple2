using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;
using Maple2.Tools.Extensions;

namespace Maple2.Server.Game.Packets;

public static class MarriagePacket {
    private enum Command : byte {
        Load = 0,
        Proposal = 3,
    }

    public static ByteWriter Load(Marriage marriage) {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.Load);
        pWriter.WriteClass<Marriage>(marriage);

        return pWriter;
    }

    public static ByteWriter Proposal(string name, long characterId) {
        var pWriter = Packet.Of(SendOp.Wedding);
        pWriter.Write<Command>(Command.Proposal);
        pWriter.WriteLong(characterId);
        pWriter.WriteUnicodeString(name);

        return pWriter;
    }
}
