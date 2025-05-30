using Maple2.Model.Enum;
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
        MentorList = 2,
        AssignMentee = 4,
        Invite = 5, // by name
        AcceptMentor = 6,
        DeclineMentor = 7,
        AssignMentor = 8,
        Load = 9,
    }

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Reward:
                HandleReward(session);
                break;
            case Command.AssignMentee:
                HandleAssignMentee(session);
                break;
            case Command.Load:
                HandleLoad(session);
                break;
            case Command.MentorList:
                HandleMentorList(session, packet);
                break;
        }
    }

    private void HandleReward(GameSession session) {
        session.Send(MentorPacket.Unknown12());
    }

    private void HandleAssignMentee(GameSession session) {
        session.Mentoring.UpdateRole(MentorRole.Mentee);
    }

    private void HandleMentorList(GameSession session, IByteReader packet) {
        int unknown = packet.ReadInt(); // 0
        session.Send(MentorPacket.MenteeInvitations());
    }

    private void HandleLoad(GameSession session) {
        session.Send(MentorPacket.MenteeInvitations());
        session.Send(MentorPacket.Load());
        //session.Send(MentorPacket.MyList());
        //  session.Send(MentorPacket.LoginPoints());
        //  session.Send(MentorPacket.DailyPoints());
        //  session.Send(MentorPacket.Unknown12());
        //   session.Send(MentorPacket.Unknown15(10));
        //   session.Send(MentorPacket.MenteeInvitations());
        //  session.Send(MentorPacket.Unknown16());
        //session.Send(MentorPacket.UpdateRole(session.Player.ObjectId));
        //session.Send(MentorPacket.MenteeList());
    }
}
