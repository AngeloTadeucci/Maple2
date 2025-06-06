using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Game.PacketHandlers.Field;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class CharacterInfoHandler : FieldPacketHandler {
    public override RecvOp OpCode => RecvOp.CharacterInfo;

    public override void Handle(GameSession session, IByteReader packet) {
        long characterId = packet.ReadLong();

        if (session.FindSession(characterId, out GameSession? other)) {
            session.Send(PlayerInfoPacket.Load(other));
            return;
        }

        // TODO: If player is on another channel, we should read from db.
        session.Send(PlayerInfoPacket.NotFound(characterId));
    }
}
