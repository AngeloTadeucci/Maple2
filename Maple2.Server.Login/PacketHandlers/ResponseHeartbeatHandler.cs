using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Login.Session;

namespace Maple2.Server.Login.PacketHandlers;

public class ResponseHeartbeatHandler : PacketHandler<LoginSession> {
    public override RecvOp OpCode => RecvOp.ResponseHeartbeat;

    public override void Handle(LoginSession session, IByteReader packet) {
        int serverTick = packet.ReadInt();
        int clientTick = packet.ReadInt();



        if (serverTick == 0 || clientTick == 0) {
            return;
        }

        if (session is { ClientTick: 0, ServerTick: 0 }) {
            session.ClientTick = clientTick;
            session.ServerTick = serverTick;
            return;
        }

        int serverDelta = serverTick - session.ServerTick;
        int clientDelta = clientTick - session.ClientTick;
        session.ClientTick = clientTick;
        session.ServerTick = serverTick;
    }
}
