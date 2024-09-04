using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Server.World.Containers;
using ChannelGuildRequest = Maple2.Server.Channel.Service.GuildRequest;

namespace Maple2.Server.World.Service;

public partial class WorldService {
    public override Task<GuildInfoResponse> GuildInfo(GuildInfoRequest request, ServerCallContext context) {
        if (!guildLookup.TryGet(request.GuildId, out GuildManager? manager)) {
            return Task.FromResult(new GuildInfoResponse());
        }

        return Task.FromResult(new GuildInfoResponse { Guild = ToGuildInfo(manager.Guild) });
    }

    public override Task<GuildResponse> Guild(GuildRequest request, ServerCallContext context) {
        switch (request.GuildCase) {
            case GuildRequest.GuildOneofCase.Create:
                return Task.FromResult(CreateGuild(request.RequestorId, request.Create));
            case GuildRequest.GuildOneofCase.Disband:
                return Task.FromResult(DisbandGuild(request.RequestorId, request.Disband));
            case GuildRequest.GuildOneofCase.Invite:
                return Task.FromResult(InviteGuild(request.RequestorId, request.Invite));
            case GuildRequest.GuildOneofCase.RespondInvite:
                return Task.FromResult(RespondInviteGuild(request.RequestorId, request.RespondInvite));
            case GuildRequest.GuildOneofCase.Leave:
                return Task.FromResult(LeaveGuild(request.RequestorId, request.Leave));
            case GuildRequest.GuildOneofCase.Expel:
                return Task.FromResult(ExpelGuild(request.RequestorId, request.Expel));
            case GuildRequest.GuildOneofCase.UpdateMember:
                return Task.FromResult(UpdateGuildMember(request.RequestorId, request.UpdateMember));
            case GuildRequest.GuildOneofCase.CheckIn:
                return Task.FromResult(CheckInGuild(request.RequestorId, request.CheckIn));
            case GuildRequest.GuildOneofCase.UpdateLeader:
                return Task.FromResult(UpdateGuildLeader(request.RequestorId, request.UpdateLeader));
            case GuildRequest.GuildOneofCase.UpdateNotice:
                return Task.FromResult(UpdateGuildNotice(request.RequestorId, request.UpdateNotice));
            case GuildRequest.GuildOneofCase.UpdateEmblem:
                return Task.FromResult(UpdateGuildEmblem(request.RequestorId, request.UpdateEmblem));
            case GuildRequest.GuildOneofCase.UpdatePoster:
                return Task.FromResult(UpdateGuildPoster(request.RequestorId, request.UpdatePoster));
            default:
                return Task.FromResult(new GuildResponse { Error = (int) GuildError.s_guild_err_none });
        }
    }

    private GuildResponse CreateGuild(long requestorId, GuildRequest.Types.Create create) {
        GuildError error = guildLookup.Create(create.GuildName, requestorId, out long guildId);
        if (error != GuildError.none) {
            return new GuildResponse { Error = (int) error };
        }
        if (!guildLookup.TryGet(guildId, out GuildManager? manager)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_null_guild };
        }

        return new GuildResponse { Guild = ToGuildInfo(manager.Guild) };
    }

    // TODO: Send reject mail to any applicants
    private GuildResponse DisbandGuild(long requestorId, GuildRequest.Types.Disband disband) {
        GuildError error = guildLookup.Disband(requestorId, disband.GuildId);
        if (error != GuildError.none) {
            return new GuildResponse { Error = (int) error };
        }

        return new GuildResponse { GuildId = disband.GuildId };
    }

    private GuildResponse InviteGuild(long requestorId, GuildRequest.Types.Invite invite) {
        if (!guildLookup.TryGet(invite.GuildId, out GuildManager? manager)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_null_guild };
        }
        if (!playerLookup.TryGet(invite.ReceiverId, out PlayerInfo? info)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_null_user };
        }

        GuildError error = manager.Invite(requestorId, info);
        if (error != GuildError.none) {
            return new GuildResponse { Error = (int) error };
        }

        return new GuildResponse { GuildId = invite.GuildId };
    }

    private GuildResponse RespondInviteGuild(long requestorId, GuildRequest.Types.RespondInvite respond) {
        if (!guildLookup.TryGet(respond.GuildId, out GuildManager? manager)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_invalid_guild };
        }
        string requestorName = manager.ConsumeInvite(requestorId);
        if (string.IsNullOrEmpty(requestorName)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_null_invite_member };
        }
        if (!playerLookup.TryGet(requestorId, out PlayerInfo? info)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_none };
        }

        if (respond.Accepted) {
            GuildError error = manager.Join(requestorName, info);
            if (error != GuildError.none) {
                return new GuildResponse { Error = (int) error };
            }

            manager.Broadcast(new ChannelGuildRequest {
                InviteReply = new ChannelGuildRequest.Types.InviteReply {
                    Name = info.Name,
                    Reply = (int) GuildInvite.Response.Accept,
                },
            });

            return new GuildResponse { Guild = ToGuildInfo(manager.Guild) };
        }

        manager.Broadcast(new ChannelGuildRequest {
            InviteReply = new ChannelGuildRequest.Types.InviteReply {
                Name = info.Name,
                Reply = (int) GuildInvite.Response.RejectInvite,
            },
        });

        return new GuildResponse { GuildId = manager.Guild.Id };
    }

    private GuildResponse LeaveGuild(long requestorId, GuildRequest.Types.Leave leave) {
        if (!guildLookup.TryGet(leave.GuildId, out GuildManager? manager)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_null_guild };
        }

        GuildError error = manager.Leave(requestorId);
        if (error != GuildError.none) {
            return new GuildResponse { Error = (int) error };
        }

        return new GuildResponse { GuildId = leave.GuildId };
    }

    private GuildResponse ExpelGuild(long requestorId, GuildRequest.Types.Expel expel) {
        if (!guildLookup.TryGet(expel.GuildId, out GuildManager? manager)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_null_guild };
        }

        GuildError error = manager.Expel(requestorId, expel.ReceiverId);
        if (error != GuildError.none) {
            return new GuildResponse { Error = (int) error };
        }

        return new GuildResponse { GuildId = expel.GuildId };
    }

    private GuildResponse UpdateGuildMember(long requestorId, GuildRequest.Types.UpdateMember update) {
        if (!guildLookup.TryGet(update.GuildId, out GuildManager? manager)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_null_guild };
        }

        byte? rankId = update.HasRank ? (byte) update.Rank : null;
        string? message = update.HasMessage ? update.Message : null;
        GuildError error = manager.UpdateMember(requestorId, update.CharacterId, rankId, message);
        if (error != GuildError.none) {
            return new GuildResponse { Error = (int) error };
        }

        return new GuildResponse { GuildId = update.GuildId };
    }

    private GuildResponse CheckInGuild(long requestorId, GuildRequest.Types.CheckIn checkIn) {
        if (!guildLookup.TryGet(checkIn.GuildId, out GuildManager? manager)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_null_guild };
        }

        GuildError error = manager.CheckIn(requestorId);
        if (error != GuildError.none) {
            return new GuildResponse { Error = (int) error };
        }

        return new GuildResponse { GuildId = checkIn.GuildId };
    }

    private GuildResponse UpdateGuildLeader(long requestorId, GuildRequest.Types.UpdateLeader leader) {
        if (!guildLookup.TryGet(leader.GuildId, out GuildManager? manager)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_null_guild };
        }

        GuildError error = manager.UpdateLeader(requestorId, leader.LeaderId);
        if (error != GuildError.none) {
            return new GuildResponse { Error = (int) error };
        }

        return new GuildResponse { GuildId = leader.GuildId };
    }

    private GuildResponse UpdateGuildNotice(long requestorId, GuildRequest.Types.UpdateNotice notice) {
        if (!guildLookup.TryGet(notice.GuildId, out GuildManager? manager)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_null_guild };
        }

        GuildError error = manager.UpdateNotice(requestorId, notice.Message);
        if (error != GuildError.none) {
            return new GuildResponse { Error = (int) error };
        }

        return new GuildResponse { GuildId = notice.GuildId };
    }

    private GuildResponse UpdateGuildEmblem(long requestorId, GuildRequest.Types.UpdateEmblem emblem) {
        if (!guildLookup.TryGet(emblem.GuildId, out GuildManager? manager)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_null_guild };
        }

        GuildError error = manager.UpdateEmblem(requestorId, emblem.Emblem);
        if (error != GuildError.none) {
            return new GuildResponse { Error = (int) error };
        }

        return new GuildResponse { GuildId = emblem.GuildId };
    }

    private GuildResponse UpdateGuildPoster(long requestorId, GuildRequest.Types.UpdatePoster poster) {
        if (!guildLookup.TryGet(poster.GuildId, out GuildManager? manager)) {
            return new GuildResponse { Error = (int) GuildError.s_guild_err_null_guild };
        }

        GuildError error = manager.AddOrUpdatePoster(poster.Id, poster.Picture, poster.OwnerId, poster.OwnerName, poster.ResourceId);
        if (error != GuildError.none) {
            return new GuildResponse { Error = (int) error };
        }

        return new GuildResponse { GuildId = poster.GuildId };
    }

    private static GuildInfo ToGuildInfo(Guild guild) {
        return new GuildInfo {
            Id = guild.Id,
            Name = guild.Name,
            Emblem = guild.Emblem,
            Notice = guild.Notice,
            CreationTime = guild.CreationTime,
            Focus = (int) guild.Focus,
            Experience = guild.Experience,
            Funds = guild.Funds,
            HouseRank = guild.HouseRank,
            HouseTheme = guild.HouseTheme,
            Members = {
                guild.Members.Values.Select(member => new GuildInfo.Types.Member {
                    CharacterId = member.CharacterId,
                    Message = member.Message,
                    Rank = member.Rank,
                    JoinTime = member.JoinTime,
                    CheckinTime = member.CheckinTime,
                    DonationTime = member.DonationTime,
                    WeeklyContribution = member.WeeklyContribution,
                    TotalContribution = member.TotalContribution,
                    DailyDonationCount = member.DailyDonationCount,
                }),
            },
            Ranks = {
                guild.Ranks.Select(rank => new GuildInfo.Types.Rank {
                    Name = rank.Name,
                    Permission = (int) rank.Permission,
                }),
            },
            Buffs = {
                guild.Buffs.Select(buff => new GuildInfo.Types.Buff {
                    Id = buff.Id,
                    Level = buff.Level,
                    ExpiryTime = buff.ExpiryTime,
                }),
            },
            Posters = {
                guild.Posters.Select(poster => new GuildInfo.Types.Poster {
                    Id = poster.Id,
                    Picture = poster.Picture,
                    OwnerId = poster.OwnerId,
                    OwnerName = poster.OwnerName,
                    ResourceId = poster.ResourceId,
                }),
            },
            Npcs = {
                guild.Npcs.Select(npc => new GuildInfo.Types.Npc {
                    Type = (int) npc.Type,
                    Level = npc.Level,
                }),
            },
        };
    }
}
