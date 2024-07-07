using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.PacketHandlers;

public class MentorHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.Mentor;

    private enum Command : byte {
        Reward = 0,
        AssignReturning = 4,
        Invite = 5, // by name
        AssignMentor = 8,
        Load = 9,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Load:
                HandleLoad(session);
                break;
        }
    }

    private void HandleLoad(GameSession session) {
        session.Send(MentorPacket.Load());
        session.Send(MentorPacket.Unknown10());
        session.Send(MentorPacket.Unknown11());
        session.Send(MentorPacket.Unknown12());
        session.Send(MentorPacket.Unknown15(10));
        session.Send(MentorPacket.MenteeInvitations());
        //session.Send(MentorPacket.UpdateRole(session.Player.ObjectId));
        //session.Send(MentorPacket.MenteeList());
    }
}
