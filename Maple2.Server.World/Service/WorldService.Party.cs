using Grpc.Core;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Game.Party;
using Maple2.Server.World.Containers;
using ChannelPartyRequest = Maple2.Server.Channel.Service.PartyRequest;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<PartyInfoResponse> PartyInfo(PartyInfoRequest request, ServerCallContext context) {
        if (!(request.PartyId != 0 && partyLookup.TryGet(request.PartyId, out PartyManager? manager)) && !(request.CharacterId != 0 && partyLookup.TryGetByCharacter(request.CharacterId, out manager))) {
            return Task.FromResult(new PartyInfoResponse());
        }

        return Task.FromResult(new PartyInfoResponse { Party = ToPartyInfo(manager.Party) });
    }

    public override Task<PartyResponse> Party(PartyRequest request, ServerCallContext context) {
        switch (request.PartyCase) {
            case PartyRequest.PartyOneofCase.Create:
                return Task.FromResult(CreateParty(request.RequestorId, request.Create));
            case PartyRequest.PartyOneofCase.Disband:
                return Task.FromResult(DisbandParty(request.RequestorId, request.Disband));
            case PartyRequest.PartyOneofCase.Invite:
                return Task.FromResult(InviteParty(request.RequestorId, request.Invite));
            case PartyRequest.PartyOneofCase.RespondInvite:
                return Task.FromResult(RespondInviteParty(request.RequestorId, request.RespondInvite));
            case PartyRequest.PartyOneofCase.Leave:
                return Task.FromResult(LeaveParty(request.RequestorId, request.Leave));
            case PartyRequest.PartyOneofCase.Kick:
                return Task.FromResult(KickParty(request.RequestorId, request.Kick));
            case PartyRequest.PartyOneofCase.UpdateLeader:
                return Task.FromResult(UpdatePartyLeader(request.RequestorId, request.UpdateLeader));
            case PartyRequest.PartyOneofCase.ReadyCheck:
                return Task.FromResult(StartReadyCheck(request.RequestorId, request.ReadyCheck));
            case PartyRequest.PartyOneofCase.VoteReply:
                return Task.FromResult(VoteReply(request.RequestorId, request.VoteReply));
            case PartyRequest.PartyOneofCase.VoteKick:
                return Task.FromResult(VoteKick(request.RequestorId, request.VoteKick));
            case PartyRequest.PartyOneofCase.JoinByPartySearch:
                return Task.FromResult(JoinByPartySearch(request.RequestorId, request.JoinByPartySearch));
            case PartyRequest.PartyOneofCase.SetDungeon:
                return Task.FromResult(SetDungeon(request.RequestorId, request.SetDungeon));
            default:
                return Task.FromResult(new PartyResponse { Error = (int) PartyError.none });
        }
    }

    private PartyResponse CreateParty(long requestorId, PartyRequest.Types.Create create) {
        PartyError error = partyLookup.Create(requestorId, out int partyId);
        if (error != PartyError.none) {
            return new PartyResponse { Error = (int) error };
        }
        if (!partyLookup.TryGet(partyId, out PartyManager? manager)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_not_found };
        }

        return new PartyResponse { Party = ToPartyInfo(manager.Party) };
    }

    private PartyResponse DisbandParty(long requestorId, PartyRequest.Types.Disband disband) {
        if (!partyLookup.TryGet(disband.PartyId, out PartyManager? manager)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_not_found };
        }

        manager.Disband(requestorId);
        return new PartyResponse { PartyId = disband.PartyId };
    }

    private PartyResponse InviteParty(long requestorId, PartyRequest.Types.Invite invite) {
        if (!partyLookup.TryGet(invite.PartyId, out PartyManager? manager)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_not_found };
        }
        if (!playerLookup.TryGet(invite.ReceiverId, out PlayerInfo? info)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_cannot_invite };
        }

        PartyError error = manager.Invite(requestorId, info);
        if (error != PartyError.none) {
            return new PartyResponse { Error = (int) error };
        }

        return new PartyResponse { PartyId = invite.PartyId };
    }

    private PartyResponse RespondInviteParty(long characterId, PartyRequest.Types.RespondInvite respond) {
        if (!partyLookup.TryGet(respond.PartyId, out PartyManager? manager)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_not_found };
        }
        string requestorName = manager.ConsumeInvite(characterId);
        if (string.IsNullOrEmpty(requestorName)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_cannot_invite };
        }
        if (!playerLookup.TryGet(characterId, out PlayerInfo? info)) {
            return new PartyResponse { Error = (int) PartyError.none };
        }

        if (respond.Reply == (int) PartyInviteResponse.Accept) {
            PartyError error = manager.Join(info);
            if (error != PartyError.none) {
                return new PartyResponse { Error = (int) error };
            }

            return new PartyResponse { Party = ToPartyInfo(manager.Party) };
        }

        manager.Broadcast(new ChannelPartyRequest {
            InviteReply = new ChannelPartyRequest.Types.InviteReply {
                Name = info.Name,
                Reply = respond.Reply,
            },
        });

        return new PartyResponse { PartyId = manager.Party.Id };
    }

    private PartyResponse LeaveParty(long requestorId, PartyRequest.Types.Leave leave) {
        if (!partyLookup.TryGet(leave.PartyId, out PartyManager? manager)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_not_found };
        }

        PartyError error = manager.Leave(requestorId);
        if (error != PartyError.none) {
            return new PartyResponse { Error = (int) error };
        }

        return new PartyResponse { PartyId = leave.PartyId };
    }

    private PartyResponse KickParty(long requestorId, PartyRequest.Types.Kick kick) {
        if (!partyLookup.TryGet(kick.PartyId, out PartyManager? manager)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_not_found };
        }

        PartyError error = manager.Kick(requestorId, kick.ReceiverId);
        if (error != PartyError.none) {
            return new PartyResponse { Error = (int) error };
        }

        return new PartyResponse { PartyId = kick.PartyId };
    }

    private PartyResponse UpdatePartyLeader(long requestorId, PartyRequest.Types.UpdateLeader update) {
        if (!partyLookup.TryGet(update.PartyId, out PartyManager? manager)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_not_found };
        }

        if (update.CharacterId == 0) {
            manager.FindNewLeader(requestorId);
            return new PartyResponse { PartyId = update.PartyId };
        }

        PartyError error = manager.UpdateLeader(requestorId, update.CharacterId);
        if (error != PartyError.none) {
            return new PartyResponse { Error = (int) error };
        }

        return new PartyResponse { PartyId = update.PartyId };
    }

    private PartyResponse StartReadyCheck(long requestorId, PartyRequest.Types.ReadyCheck readyCheck) {
        if (!partyLookup.TryGet(readyCheck.PartyId, out PartyManager? manager)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_not_found };
        }

        PartyError error = manager.StartReadyCheck(requestorId);
        return new PartyResponse {
            Error = (int) error,
        };
    }

    private PartyResponse VoteReply(long requestorId, PartyRequest.Types.VoteReply reply) {
        if (!partyLookup.TryGet(reply.PartyId, out PartyManager? manager)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_not_found };
        }

        PartyError error = manager.ReadyCheckReply(requestorId, reply.Reply);
        return new PartyResponse {
            Error = (int) error,
        };
    }

    private PartyResponse VoteKick(long requestorId, PartyRequest.Types.VoteKick voteKick) {
        if (!partyLookup.TryGet(voteKick.PartyId, out PartyManager? manager)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_not_found };
        }

        PartyError error = manager.VoteKick(requestorId, voteKick.TargetUserId);
        return new PartyResponse {
            Error = (int) error,
        };
    }

    private PartyResponse JoinByPartySearch(long requestorId, PartyRequest.Types.JoinByPartySearch joinByPartySearch) {
        if (!partyLookup.TryGet(joinByPartySearch.PartyId, out PartyManager? manager)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_not_found };
        }

        if (manager.Party.Search == null) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_invalid_recruit };
        }

        if (!manager.Party.Search.NoApproval) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_wrong_recruit };
        }

        if (manager.Party.Search.Id != joinByPartySearch.PartySearchId) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_wrong_recruit };
        }

        if (manager.Party.Search.Size <= manager.Party.Members.Count) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_full_limit_player };
        }

        if (!playerLookup.TryGet(requestorId, out PlayerInfo? info)) {
            return new PartyResponse { Error = (int) PartyError.none };
        }

        PartyError error = manager.Join(info);
        if (error != PartyError.none) {
            return new PartyResponse { Error = (int) error };
        }

        return new PartyResponse { Party = ToPartyInfo(manager.Party) };
    }

    private PartyResponse SetDungeon(long requestorId, PartyRequest.Types.SetDungeon setDungeon) {
        if (!partyLookup.TryGet(setDungeon.PartyId, out PartyManager? manager)) {
            return new PartyResponse { Error = (int) PartyError.s_party_err_not_found };
        }

        manager.SetDungeon(requestorId, setDungeon.DungeonId, setDungeon.Set, setDungeon.DungeonRoomId);

        return new PartyResponse();
    }

    private static PartyInfo ToPartyInfo(Party party) {
        return new PartyInfo {
            Id = party.Id,
            CreationTime = party.CreationTime,
            LeaderAccountId = party.LeaderAccountId,
            LeaderCharacterId = party.LeaderCharacterId,
            LeaderName = party.LeaderName,
            DungeonId = party.DungeonId,
            DungeonRoomId = party.DungeonLobbyRoomId,
            DungeonSet = party.DungeonSet,
            Members = {
                party.Members.Values.Select(member => new PartyInfo.Types.Member {
                    CharacterId = member.CharacterId,
                    JoinTime = member.JoinTime,
                }),
            },
        };
    }
}
