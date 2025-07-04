using Maple2.PacketLib.Tools;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class ResponseVersionHandler : ResponseVersionHandler<GameSession> {
    public override void Handle(GameSession session, IByteReader packet) {
        base.Handle(session, packet);

        session.Send(RequestPacket.Key());
    }
}
