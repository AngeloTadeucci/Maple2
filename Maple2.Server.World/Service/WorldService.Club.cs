﻿using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Server.World.Containers;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<ClubInfoResponse> ClubInfo(ClubInfoRequest request, ServerCallContext context) {
        List<ClubManager> clubManagers = clubLookup.TryGetByCharacterId(request.CharacterId);

        return Task.FromResult(new ClubInfoResponse {
            Clubs = {
                clubManagers.Select(manager => ToClubInfo(manager.Club))
            }
        });
    }

    public override Task<ClubResponse> Club(ClubRequest request, ServerCallContext context) {
        switch (request.ClubCase) {
            case ClubRequest.ClubOneofCase.Create:
                return Task.FromResult(CreateClub(request.RequestorId, request.Create));
            case ClubRequest.ClubOneofCase.NewClubInvite:
                return Task.FromResult(NewClubInvite(request.NewClubInvite));
            // case ClubRequest.ClubOneofCase.Disband:
            //     return Task.FromResult(DisbandClub(request.RequestorId, request.Disband));
            // case ClubRequest.ClubOneofCase.Invite:
            //     return Task.FromResult(InviteClub(request.RequestorId, request.Invite));
            // case ClubRequest.ClubOneofCase.RespondInvite:
            //     return Task.FromResult(RespondInviteClub(request.RequestorId, request.RespondInvite));
            // case ClubRequest.ClubOneofCase.Leave:
            //     return Task.FromResult(LeaveClub(request.RequestorId, request.Leave));
            // case ClubRequest.ClubOneofCase.Expel:
            //     return Task.FromResult(ExpelClub(request.RequestorId, request.Expel));
            // case ClubRequest.ClubOneofCase.UpdateMember:
            //     return Task.FromResult(UpdateMember(request.RequestorId, request.UpdateMember));
            default:
                return Task.FromResult(new ClubResponse {
                    Error = (int) ClubError.s_club_err_unknown
                });
        }
    }

    private ClubResponse CreateClub(long requestorId, ClubRequest.Types.Create create) {
        ClubError error = clubLookup.Create(create.ClubName, requestorId, out long clubId);
        if (error != ClubError.none) {
            return new ClubResponse {
                Error = (int) error
            };
        }

        if (!clubLookup.TryGet(clubId, out ClubManager? manager)) {
            return new ClubResponse {
                Error = (int) ClubError.s_club_err_null_club,
            };
        }

        manager.Broadcast(new Channel.Service.ClubRequest {
            Create = new Channel.Service.ClubRequest.Types.Create {
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

        var reply = (ClubInviteReply) invite.Reply;
        ClubError error = manager.NewClubInvite(invite.ReceiverId, reply);
        if (error != ClubError.none) {
            return new ClubResponse {
                Error = (int) error,
            };
        }

        if (reply == ClubInviteReply.Reject) {
            error = clubLookup.Disband(invite.ClubId);
            if (error != ClubError.none) {
                return new ClubResponse {
                    Error = (int) error,
                };
            }
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
                    LoginTime = member.Value.LoginTime,
                    JoinTime = member.Value.JoinTime,
                }),
            },
        };
    }
}