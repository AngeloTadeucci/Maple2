using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Server.Channel.Service;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<ClubResponse> Club(ClubRequest request, ServerCallContext context) {
        switch (request.ClubCase) {
            case ClubRequest.ClubOneofCase.Create:
                return Task.FromResult(Create(request.ClubId, request.ReceiverIds, request.Create));
            case ClubRequest.ClubOneofCase.StagedClubFail:
                return Task.FromResult(StagedClubFail(request.ClubId, request.ReceiverIds, request.StagedClubFail));
            case ClubRequest.ClubOneofCase.StagedClubInviteReply:
                return Task.FromResult(StagedClubInviteReply(request.ClubId, request.ReceiverIds, request.StagedClubInviteReply));
            case ClubRequest.ClubOneofCase.Establish:
                return Task.FromResult(Establish(request.ClubId, request.ReceiverIds, request.Establish));
            default:
                return Task.FromResult(new ClubResponse { Error = (int) ClubError.none });
        }
    }

    private ClubResponse Create(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.Create create) {
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_member };
            }

            if (session.Clubs.TryAdd(clubId, new ClubManager(create.Info, session))) {
                session.Send(ClubPacket.Load(session.Clubs[clubId].Club!));
            }
        }

        return new ClubResponse();
    }

    private ClubResponse StagedClubFail(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.StagedClubFail stagedClubFail) {
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_member };
            }

            session.Send(ClubPacket.DeleteStagedClub(clubId, (ClubInviteReply) stagedClubFail.Reply));
        }
        return new ClubResponse();
    }

    private ClubResponse StagedClubInviteReply(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.StagedClubInviteReply stagedClubInviteReply) {
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_member };
            }

            if (!session.Clubs.TryGetValue(clubId, out ClubManager? manager)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_club };
            }

            session.Send(ClubPacket.StagedClubInviteReply(clubId, (ClubInviteReply) stagedClubInviteReply.Reply, stagedClubInviteReply.Name));
        }

        return new ClubResponse();
    }

    private ClubResponse Establish(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.Establish establish) {
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_member };
            }

            if (!session.Clubs.TryGetValue(clubId, out ClubManager? manager) || manager.Club == null) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_club };
            }

            manager.Club.State = ClubState.Established;

            if (receiverId == manager.Club.Leader.CharacterId) {
                session.Send(ClubPacket.Join(manager.Club.Leader, manager.Club.Name));
                session.Send(ClubPacket.Establish(manager.Club));
            }

            session.Send(ClubPacket.StagedClubInviteReply(clubId, ClubInviteReply.Accept, string.Empty));
        }
        return new ClubResponse();
    }
}
