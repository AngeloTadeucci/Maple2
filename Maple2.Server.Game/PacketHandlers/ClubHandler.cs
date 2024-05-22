
using Grpc.Core;
using Maple2.Model.Error;
using Maple2.Model.Game.Party;
using Maple2.PacketLib.Tools;
using Maple2.Server.Core.Constants;
using Maple2.Server.Core.Modules;
using Maple2.Server.Core.PacketHandlers;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Session;
using Maple2.Server.World.Service;
using WorldClient = Maple2.Server.World.Service.World.WorldClient;


public class ClubHandler : PacketHandler<GameSession> {
    public override RecvOp OpCode => RecvOp.Club;

    private enum Command : byte {
        Create = 1,
        NewClubInvite = 3,
        SendInvite = 6,
        InviteResponse = 8,
        Leave = 10,
        Buff = 13,
        Rename = 14
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
                Create(session, packet);
                break;
                // case Command.NewClubInvite:
                //     NewClubInvite(session, packet);
                //     break;
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

    private void Create(GameSession session, IByteReader packet) {
        Party? party = session.Party.Party;
        if (party is null || party.LeaderCharacterId != session.Player.Value.Character.Id) {
            return;
        }

        if (party.Members.Any(member => !member.Value.Info.Online) /**|| party.Members.Any(member => member.Value.Info.Clubs.Count >= Constant.ClubMaxCount)**/) {
            session.Send(ClubPacket.ErrorNotice(ClubError.s_club_err_notparty_alllogin));
            return;
        }

        string clubName = packet.ReadUnicodeString();
        if (clubName.Contains(' ')) {
            session.Send(ClubPacket.ErrorNotice(ClubError.s_club_err_clubname_has_blank));
            return;
        }

        try {
            var request = new ClubRequest {
                RequestorId = session.CharacterId,
                Create = new ClubRequest.Types.Create {
                    ClubName = clubName,
                }
            };
            ClubResponse response = World.Club(request);
            var error = (ClubError) response.Error;
            if (error != ClubError.none) {
                session.Send(ClubPacket.ErrorNotice(error));
                return;
            }

            ClubManager clubManager = new ClubManager(response.Club, session);

            if (clubManager.Club is null) {
                session.Send(ClubPacket.ErrorNotice(ClubError.s_club_err_unknown));
                return;
            }

            session.Clubs.TryAdd(response.ClubId, clubManager);
            session.Send(ClubPacket.Create(clubManager.Club));

        } catch (RpcException ex) {
            Logger.Error(ex, "Failed to create guild: {Name}", clubName);
            session.Send(ClubPacket.ErrorNotice(ClubError.s_club_err_unknown));
        }
    }
}
