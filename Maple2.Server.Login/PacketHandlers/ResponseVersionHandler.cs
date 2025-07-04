using Maple2.PacketLib.Tools;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Login.Session;

namespace Maple2.Server.Login.PacketHandlers;

public class ResponseVersionHandler : ResponseVersionHandler<LoginSession> {
    public ResponseVersionHandler() { }

    public override void Handle(LoginSession session, IByteReader packet) {
        base.Handle(session, packet);

        session.Send(RequestPacket.Login());
    }
}
