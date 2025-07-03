using System.Linq;
using Maple2.Model.Enum;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Core.Packets;
using Maple2.Server.Login.Session;

namespace Maple2.Server.Login.PacketHandlers;

public class ServerEnterHandler : PacketHandler<LoginSession> {
    public override RecvOp OpCode => RecvOp.ServerEnter;

    public ServerEnterHandler() { }

    public override void Handle(LoginSession session, IByteReader packet) {
        packet.Read<Language>();

        session.ListServers();
        session.ListCharacters();
        session.Send(GameEventPacket.Load(session.Server.GetEvents().ToArray()));
    }
}
