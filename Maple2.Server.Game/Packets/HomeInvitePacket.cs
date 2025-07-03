using Maple2.Model.Game;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Packets;

namespace Maple2.Server.Game.Packets;

public static class HomeInvitePacket {
    private enum Command : byte {
        Invite = 2,
    }

    public static ByteWriter Invite(Character character) {
        var pWriter = Packet.Of(SendOp.HomeInvite);
        pWriter.Write(Command.Invite);
        pWriter.WriteLong(character.AccountId);
        pWriter.WriteUnicodeString(character.Name);

        return pWriter;
    }
}
