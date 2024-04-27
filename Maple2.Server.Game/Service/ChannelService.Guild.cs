using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Server.Channel.Service;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;

namespace Maple2.Server.Game.Service;

public partial class ChannelService {
    public override Task<GuildResponse> Guild(GuildRequest request, ServerCallContext context) {
        switch (request.GuildCase) {
            case GuildRequest.GuildOneofCase.Invite:
                return Task.FromResult(GuildInvite(request.GuildId, request.ReceiverIds, request.Invite));
            case GuildRequest.GuildOneofCase.InviteReply:
                return Task.FromResult(GuildInviteReply(request.ReceiverIds, request.InviteReply));
            case GuildRequest.GuildOneofCase.AddMember:
                return Task.FromResult(AddGuildMember(request.GuildId, request.ReceiverIds, request.AddMember));
            case GuildRequest.GuildOneofCase.RemoveMember:
                return Task.FromResult(RemoveGuildMember(request.ReceiverIds, request.RemoveMember));
            case GuildRequest.GuildOneofCase.UpdateMember:
                return Task.FromResult(UpdateGuildMember(request.ReceiverIds, request.UpdateMember));
            default:
                return Task.FromResult(new GuildResponse { Error = (int) GuildError.s_guild_err_none });
        }
    }

    private GuildResponse GuildInvite(long guildId, IEnumerable<long> receiverIds, GuildRequest.Types.Invite invite) {
        // There really should only be 1 receiver here, but whatever.
        foreach (long receiverId in receiverIds) {
            if (!server.GetSession(receiverId, out GameSession? session)) {
                return new GuildResponse { Error = (int) GuildError.s_guild_err_wait_inviting };
            }
            if (session.Buddy.IsBlocked(invite.SenderId)) {
                return new GuildResponse { Error = (int) GuildError.s_guild_err_blocked };
            }
            if (session.Guild.Guild != null) {
                return new GuildResponse { Error = (int) GuildError.s_guild_err_has_guild };
            }

            var guildInvite = new GuildInvite {
                GuildId = guildId,
                GuildName = invite.GuildName,
                SenderName = invite.SenderName,
                ReceiverName = session.PlayerName,
            };
            session.Send(GuildPacket.InviteInfo(guildInvite));
        }

        return new GuildResponse();
    }

    private GuildResponse GuildInviteReply(IEnumerable<long> receiverIds, GuildRequest.Types.InviteReply reply) {
        foreach (long characterId in receiverIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            session.Send(GuildPacket.NotifyInvite(reply.Name, (GuildInvite.Response) reply.Reply));
        }

        return new GuildResponse();
    }

    private GuildResponse AddGuildMember(long guildId, IEnumerable<long> receiverIds, GuildRequest.Types.AddMember add) {
        if (!playerInfos.GetOrFetch(add.CharacterId, out PlayerInfo? info)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_fail_addmember };
        }

        foreach (long characterId in receiverIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            // Intentionally create a separate GuildMember instance for each session.
            session.Guild.AddMember(add.RequestorName, new GuildMember {
                GuildId = guildId,
                Info = info.Clone(),
                Rank = (byte) add.Rank,
                JoinTime = add.JoinTime,
                LoginTime = add.LoginTime,
            });
        }

        return new GuildResponse();
    }

    private GuildResponse RemoveGuildMember(IEnumerable<long> receiverIds, GuildRequest.Types.RemoveMember remove) {
        foreach (long characterId in receiverIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            if (characterId == remove.CharacterId) {
                session.Send(GuildPacket.NotifyExpel(remove.RequestorName));
                session.Guild.RemoveGuild();
            } else {
                session.Guild.RemoveMember(remove.CharacterId, remove.RequestorName);
            }
        }

        return new GuildResponse();
    }

    private GuildResponse UpdateGuildMember(IEnumerable<long> receiverIds, GuildRequest.Types.UpdateMember update) {
        foreach (long characterId in receiverIds) {
            if (!server.GetSession(characterId, out GameSession? session)) {
                continue;
            }

            if (update.HasRank) {
                session.Guild.UpdateMemberRank(update.RequestorId, update.CharacterId, (byte) update.Rank);
            }
            if (update.HasMessage) {
                session.Guild.UpdateMemberMessage(update.CharacterId, update.Message);
            }
        }

        return new GuildResponse();
    }
}
