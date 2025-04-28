using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class ResponseHeartbeat : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.ResponseHeartbeat;

    public override void Handle(GameSession session, IByteReader packet) {
        int serverTick = packet.ReadInt();
        int clientTick = packet.ReadInt();

        session.Latency = Environment.TickCount - serverTick;
        if (session.Latency > Constant.MaxAllowedLatency) {
#if !DEBUG
            session.Disconnect();
#endif
            return;
        }

        if (serverTick == 0 || clientTick == 0) {
            return;
        }

        if (session is { LastClientTick: 0, LastServerTick: 0 }) {
            session.LastClientTick = clientTick;
            session.LastServerTick = serverTick;
            return;
        }

        int serverDelta = serverTick - session.LastServerTick;
        int clientDelta = clientTick - session.LastClientTick;

        session.LastClientTick = clientTick;
        session.LastServerTick = serverTick;
    }
}
