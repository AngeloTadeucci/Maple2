using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Club;
using Maple2.Model.Metadata;
using Maple2.Server.Channel.Service;
using Maple2.Server.Game.Manager;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using ClubResponse = Maple2.Server.Channel.Service.ClubResponse;

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
            case ClubRequest.ClubOneofCase.Disband:
                return Task.FromResult(Disband(request.ClubId, request.ReceiverIds, request.Disband));
            case ClubRequest.ClubOneofCase.RemoveMember:
                return Task.FromResult(RemoveMember(request.ClubId, request.ReceiverIds, request.RemoveMember));
            case ClubRequest.ClubOneofCase.Invite:
                return Task.FromResult(Invite(request.ClubId, request.ReceiverIds, request.Invite));
            case ClubRequest.ClubOneofCase.InviteReply:
                return Task.FromResult(InviteReply(request.ClubId, request.ReceiverIds, request.InviteReply));
            case ClubRequest.ClubOneofCase.AddMember:
                return Task.FromResult(AddMember(request.ClubId, request.ReceiverIds, request.AddMember));
            case ClubRequest.ClubOneofCase.UpdateLeader:
                return Task.FromResult(UpdateLeader(request.ClubId, request.ReceiverIds, request.UpdateLeader));
            case ClubRequest.ClubOneofCase.Rename:
                return Task.FromResult(Rename(request.ClubId, request.ReceiverIds, request.Rename));
            default:
                return Task.FromResult(new ClubResponse { Error = (int) ClubError.none });
        }
    }

    private ClubResponse Create(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.Create create) {
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_member };
            }

            var clubManager = ClubManager.Create(create.Info, session);
            if (clubManager == null) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_club };
            }
            if (session.Clubs.TryAdd(clubId, clubManager)) {
                session.Send(ClubPacket.Create(clubManager.Club));
            }
        }

        return new ClubResponse();
    }

    private ClubResponse StagedClubFail(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.StagedClubFail stagedClubFail) {
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_member };
            }

            session.Send(ClubPacket.DeleteStagedClub(clubId, (Maple2.Model.Enum.ClubResponse) stagedClubFail.Reply));
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

            session.Send(ClubPacket.StagedClubInviteReply(clubId, (Maple2.Model.Enum.ClubResponse) stagedClubInviteReply.Reply, stagedClubInviteReply.Name));
        }

        return new ClubResponse();
    }

    private ClubResponse Establish(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.Establish establish) {
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_member };
            }

            if (!session.Clubs.TryGetValue(clubId, out ClubManager? manager)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_club };
            }

            manager.Club.State = ClubState.Established;

            if (receiverId == manager.Club.Leader.CharacterId) {
                session.Send(ClubPacket.Join(manager.Club.Leader, manager.Club.Name));
                session.Send(ClubPacket.Establish(manager.Club));
            }

            session.Send(ClubPacket.StagedClubInviteReply(clubId, Maple2.Model.Enum.ClubResponse.Accept, string.Empty));
        }
        return new ClubResponse();
    }

    private ClubResponse Disband(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.Disband disband) {
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_member };
            }

            if (!session.Clubs.TryGetValue(clubId, out ClubManager? manager)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_club };
            }

            manager.Disband();
        }
        return new ClubResponse();
    }

    private ClubResponse RemoveMember(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.RemoveMember remove) {
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                continue;
            }

            if (!session.Clubs.TryGetValue(clubId, out ClubManager? manager)) {
                continue;
            }

            manager.RemoveMember(remove.CharacterId);
        }

        return new ClubResponse();
    }

    private ClubResponse Invite(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.Invite invite) {
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_invite_member };
            }

            if (session.Buddy.IsBlocked(invite.SenderId)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_blocked };
            }

            if (session.Clubs.Count >= Constant.ClubMaxCount) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_full_club_member };
            }

            session.Send(ClubPacket.Invite(new ClubInvite {
                ClubId = clubId,
                Name = invite.ClubName,
                LeaderName = invite.SenderName,
                Invitee = session.PlayerName,
            }));
        }
        return new ClubResponse { Error = (int) ClubError.none };
    }

    private ClubResponse InviteReply(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.InviteReply reply) {
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                continue;
            }

            session.Send(ClubPacket.InviteNotification(clubId, reply.RequestorName, reply.Accept));
        }

        return new ClubResponse();
    }

    private ClubResponse AddMember(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.AddMember add) {
        if (!playerInfos.GetOrFetch(add.CharacterId, out PlayerInfo? info)) {
            return new ClubResponse { Error = (int) ClubError.s_club_err_null_invite_member };
        }

        foreach (long characterId in receiverIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            if (!session.Clubs.TryGetValue(clubId, out ClubManager? manager)) {
                continue;
            }

            var member = new ClubMember {
                ClubId = clubId,
                Info = info.Clone(),
                JoinTime = add.JoinTime,
                LoginTime = add.LoginTime,
            };
            manager.AddMember(add.RequestorName, member);
        }

        return new ClubResponse();
    }

    private ClubResponse UpdateLeader(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.UpdateLeader update) {
        foreach (long characterId in receiverIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            if (!session.Clubs.TryGetValue(clubId, out ClubManager? manager)) {
                continue;
            }

            manager.UpdateLeader(update.CharacterId);
        }

        return new ClubResponse();
    }

    private ClubResponse Rename(long clubId, IEnumerable<long> receiverIds, ClubRequest.Types.Rename rename) {
        foreach (long characterId in receiverIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_member };
            }

            if (!session.Clubs.TryGetValue(clubId, out ClubManager? manager)) {
                return new ClubResponse { Error = (int) ClubError.s_club_err_null_club };
            }

            manager.Rename(rename.Name, rename.ChangedTime);
        }

        return new ClubResponse();
    }
}
