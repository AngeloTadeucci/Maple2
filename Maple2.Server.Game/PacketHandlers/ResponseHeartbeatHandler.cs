using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class ResponseHeartbeatHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.ResponseHeartbeat;

    public override void Handle(GameSession session, IByteReader packet) {
        int serverTick = packet.ReadInt();
        int clientTick = packet.ReadInt();

        int latency = Environment.TickCount - serverTick;
        if (latency > Constant.MaxAllowedLatency) {
#if !DEBUG
            session.Disconnect();
#endif
            return;
        }

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
