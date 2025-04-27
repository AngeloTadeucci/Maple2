using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Login.Session;

namespace Maple2.Server.Login.PacketHandlers;

public class ResponseHeartbeat : PacketHandler<LoginSession> {
    public override RecvOp OpCode => RecvOp.ResponseHeartbeat;

    public override void Handle(LoginSession session, IByteReader packet) {
        int serverTick = packet.ReadInt();
        int clientTick = packet.ReadInt();

        // should we do something with the ticks?
    }
}
