using Grpc.Core;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Club;
using Maple2.Model.Metadata;
using Maple2.Server.World.Containers;
using ChannelClubRequest = Maple2.Server.Channel.Service.ClubRequest;


namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<ClubInfoResponse> ClubInfo(ClubInfoRequest request, ServerCallContext context) {
        List<ClubManager> clubManagers = clubLookup.TryGetByCharacterId(request.CharacterId);

        return Task.FromResult(new ClubInfoResponse {
            Clubs = { clubManagers.Select(manager => ToClubInfo(manager.Club)) },
        });
    }

    public override Task<ClubResponse> Club(ClubRequest request, ServerCallContext context) {
        switch (request.ClubCase) {
            case ClubRequest.ClubOneofCase.Create:
                return Task.FromResult(CreateClub(request.RequestorId, request.Create));
            case ClubRequest.ClubOneofCase.NewClubInvite:
                return Task.FromResult(NewClubInvite(request.NewClubInvite));
            case ClubRequest.ClubOneofCase.Invite:
                return Task.FromResult(InviteClub(request.RequestorId, request.Invite));
            case ClubRequest.ClubOneofCase.RespondInvite:
                return Task.FromResult(RespondInviteClub(request.RequestorId, request.RespondInvite));
            case ClubRequest.ClubOneofCase.Leave:
                return Task.FromResult(LeaveClub(request.RequestorId, request.Leave));
            case ClubRequest.ClubOneofCase.Rename:
                return Task.FromResult(Rename(request.RequestorId, request.Rename));
            default:
                return Task.FromResult(new ClubResponse { Error = (int) ClubError.s_club_err_unknown });
        }
    }

    private ClubResponse CreateClub(long requestorId, ClubRequest.Types.Create create) {
        if (!partyLookup.TryGetByCharacter(requestorId, out PartyManager? partyManager)) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_unknown,
            };
        }

        if (partyManager.Party.Members.Any(member => !member.Value.Info.Online)) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_notparty_alllogin,
            };
        }

        if (partyManager.Party.Members.Any(member => member.Value.Info.ClubIds.Count >= Constant.ClubMaxCount)) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_full_club_member,
            };
        }

        if (create.ClubName.Contains(' ')) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_clubname_has_blank,
            };
        }

        ClubError error = clubLookup.Create(create.ClubName, requestorId, out long clubId);
        if (error != ClubError.none) {
            return new ClubResponse {
                Error = (int) error,
            };
        }

        if (!clubLookup.TryGet(clubId, out ClubManager? manager)) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_null_club,
            };
        }

        manager.Broadcast(new ChannelClubRequest {
            Create = new ChannelClubRequest.Types.Create {
                Info = ToClubInfo(manager.Club),
            },
            ClubId = manager.Club.Id,
        });

        return new ClubResponse();
    }

    private ClubResponse NewClubInvite(ClubRequest.Types.NewClubInvite invite) {
        if (!clubLookup.TryGet(invite.ClubId, out ClubManager? manager)) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_null_club,
            };
        }

        var reply = (Model.Enum.ClubResponse) invite.Reply;
        ClubError error = manager.NewClubInvite(invite.ReceiverId, reply);
        if (error != ClubError.none) {
            return new ClubResponse {
                Error = (int) error,
            };
        }

        if (reply == Model.Enum.ClubResponse.Reject) {
            error = clubLookup.Disband(invite.ClubId);
            if (error != ClubError.none) {
                return new ClubResponse {
                    Error = (int) error,
                };
            }
        }
        return new ClubResponse();
    }

    private ClubResponse InviteClub(long requestorId, ClubRequest.Types.Invite invite) {
        if (!clubLookup.TryGet(invite.ClubId, out ClubManager? manager)) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_null_club,
            };
        }

        if (!playerLookup.TryGet(invite.ReceiverId, out PlayerInfo? info)) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_null_user,
            };
        }

        ClubError error = manager.Invite(requestorId, info);
        if (error != ClubError.none) {
            return new ClubResponse {
                Error = (int) error,
            };
        }

        return new ClubResponse();
    }

    private ClubResponse RespondInviteClub(long characterId, ClubRequest.Types.RespondInvite respond) {
        if (!clubLookup.TryGet(respond.ClubId, out ClubManager? manager)) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_null_club,
            };
        }

        string requestorName = manager.ConsumeInvite(characterId);
        if (string.IsNullOrEmpty(requestorName)) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_unknown,
            };
        }

        if (!playerLookup.TryGet(characterId, out PlayerInfo? info)) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_null_user,
            };
        }

        manager.Broadcast(new ChannelClubRequest {
            InviteReply = new ChannelClubRequest.Types.InviteReply {
                RequestorName = requestorName,
                Accept = respond.Accept,
            },
            ClubId = manager.Club.Id,
        });

        if (respond.Accept) {
            ClubError error = manager.Join(requestorName, info);
            if (error != ClubError.none) {
                return new ClubResponse {
                    Error = (int) error,
                };
            }

            return new ClubResponse {
                Club = ToClubInfo(manager.Club),
            };
        }

        return new ClubResponse();
    }

    private ClubResponse LeaveClub(long requesterId, ClubRequest.Types.Leave leave) {
        if (!clubLookup.TryGet(leave.ClubId, out ClubManager? manager)) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_null_club,
            };
        }

        if (manager.Club.Members.Count <= 2) {
            clubLookup.Disband(manager.Club.Id);
            return new ClubResponse();
        }
        ClubError error = manager.Leave(requesterId);
        if (error != ClubError.none) {
            return new ClubResponse {
                Error = (int) error,
            };
        }

        return new ClubResponse();
    }

    private ClubResponse Rename(long requesterId, ClubRequest.Types.Rename rename) {
        if (!clubLookup.TryGet(rename.ClubId, out ClubManager? manager)) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_null_club,
            };
        }

        if (manager.Club.LeaderId != requesterId) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_no_master,
            };
        }

        ClubError error = manager.Rename(rename.Name);
        if (error != ClubError.none) {
            return new ClubResponse {
                Error = (int) error,
            };
        }

        return new ClubResponse();
    }

    private static ClubInfo ToClubInfo(Club club) {
        return new ClubInfo {
            Id = club.Id,
            Name = club.Name,
            LeaderId = club.Leader.Info.CharacterId,
            LeaderName = club.Leader.Info.Name,
            CreationTime = club.CreationTime,
            State = (int) club.State,
            Members = {
                club.Members.Select(member => new ClubInfo.Types.Member {
                    CharacterId = member.Value.Info.CharacterId,
                    CharacterName = member.Value.Info.Name,
                    JoinTime = member.Value.JoinTime,
                }),
            },
        };
    }
}
