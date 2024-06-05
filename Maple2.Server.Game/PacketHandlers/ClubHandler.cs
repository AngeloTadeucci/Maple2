using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Party;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.World.Service;
using Maple2.Tools.Extensions;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;


namespace Maple2.Server.Game.PacketHandlers;

public class ClubHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.Club;

    private enum Command : byte {
        Create = 1,
        NewClubInvite = 3,
        SendInvite = 6,
        InviteResponse = 8,
        Leave = 10,
        Buff = 13,
        Rename = 14,
    }

    #region Autofac Autowired
    // ReSharper disable MemberCanBePrivate.Global
    public required WorldClient World { private get; init; }
    // ReSharper restore All
    #endregion

    public override void Handle(GameSession session, IByteReader packet) {
        var command = packet.Read<Command>();
        switch (command) {
            case Command.Create:
                HandleCreate(session, packet);
                break;
            case Command.NewClubInvite:
                HandleNewClubInvite(session, packet);
                break;
            // case Command.SendInvite:
            //     SendInvite(session, packet);
            //     break;
            // case Command.InviteResponse:
            //     InviteResponse(session, packet);
            //     break;
            // case Command.Leave:
            //     Leave(session, packet);
            //     break;
            // case Command.Buff:
            //     Buff(session, packet);
            //     break;
            // case Command.Rename:
            //     Rename(session, packet);
            //     break;
        }
    }

    private void HandleCreate(GameSession session, IByteReader packet) {
        Party? party = session.Party.Party;
        if (party is null || party.LeaderCharacterId != session.Player.Value.Character.Id) {
            return;
        }

        // TODO: Check if player is in 3 clubs already

        if (party.Members.Any(member => !member.Value.Info.Online) /**|| party.Members.Any(member => member.Value.Info.Clubs.Count >= Constant.ClubMaxCount)**/) {
            session.Send(ClubPacket.Error(ClubError.s_club_err_notparty_alllogin));
            return;
        }

        string clubName = packet.ReadUnicodeString();
        if (clubName.Contains(' ')) {
            session.Send(ClubPacket.Error(ClubError.s_club_err_clubname_has_blank));
            return;
        }

        try {
            var request = new ClubRequest {
                RequestorId = session.CharacterId,
                Create = new ClubRequest.Types.Create {
                    ClubName = clubName,
                },
            };
            ClubResponse response = World.Club(request);
            var error = (ClubError) response.Error;
            if (error != ClubError.none) {
                session.Send(ClubPacket.Error(error));
            }
        } catch (RpcException ex) {
            Logger.Error(ex, "Failed to create guild: {Name}", clubName);
            session.Send(ClubPacket.Error(ClubError.s_club_err_unknown));
        }
    }

    private void HandleNewClubInvite(GameSession session, IByteReader packet) {
        long clubId = packet.ReadLong();
        var reply = packet.Read<ClubInviteReply>();

        ClubResponse response = World.Club(new ClubRequest {
            NewClubInvite = new ClubRequest.Types.NewClubInvite {
                ClubId = clubId,
                ReceiverId = session.CharacterId,
                Reply = (int) reply,
            },
        });

        var error = (ClubError) response.Error;
        if (error != ClubError.none) {
            session.Send(ClubPacket.Error(error));
        }
    }
}
