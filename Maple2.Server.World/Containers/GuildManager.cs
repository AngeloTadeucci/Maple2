using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Grpc.Core;
using Maple2.Database.Storage;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Channel.Service;
using Maple2.Tools.Extensions;
using ChannelClient = Maple2.Server.Channel.Service.Channel.ChannelClient;

namespace Maple2.Server.World.Containers;

public class GuildManager : IDisposable {
    public required GameStorage GameStorage { get; init; }
    public required ChannelClientLookup ChannelClients { get; init; }

    public readonly Guild Guild;
    private readonly ConcurrentDictionary<long, (string, DateTime)> pendingInvites;

    public GuildManager(Guild guild) {
        Guild = guild;
        pendingInvites = new ConcurrentDictionary<long, (string, DateTime)>();
    }

    public void Dispose() {
        using GameStorage.Request db = GameStorage.Context();
        db.SaveGuild(Guild);
    }

    public void Broadcast(GuildRequest request) {
        if (request.GuildId > 0 && request.GuildId != Guild.Id) {
            throw new InvalidOperationException($"Broadcasting {request.GuildCase} for incorrect guild: {request.GuildId} => {Guild.Id}");
        }

        request.GuildId = Guild.Id;
        foreach (IGrouping<short, GuildMember> group in Guild.Members.Values.GroupBy(member => member.Info.Channel)) {
            if (!ChannelClients.TryGetClient(group.Key, out ChannelClient? client)) {
                continue;
            }

            request.ReceiverIds.Clear();
            request.ReceiverIds.AddRange(group.Select(member => member.Info.CharacterId));

            try {
                client.Guild(request);
            } catch { /* ignored */ }
        }
    }

    public GuildError Invite(long requestorId, PlayerInfo player) {
        if (!Guild.Members.TryGetValue(requestorId, out GuildMember? requestor)) {
            return GuildError.s_guild_err_null_member;
        }
        GuildRank? rank = Guild.Ranks.ElementAtOrDefault(requestor.Rank);
        if (rank == null || rank.Permission.HasFlag(GuildPermission.InviteMembers)) {
            //return GuildError.s_guild_err_no_authority;
        }
        if (Guild.Members.Count >= Guild.Capacity) {
            return GuildError.s_guild_err_full_member;
        }
        if (Guild.Members.ContainsKey(player.CharacterId)) {
            return GuildError.s_guild_err_already_exist;
        }
        if (!ChannelClients.TryGetClient(player.Channel, out ChannelClient? client)) {
            return GuildError.s_guild_err_wait_inviting;
        }

        try {
            pendingInvites[player.CharacterId] = (requestor.Name, DateTime.Now.AddSeconds(30));
            var request = new GuildRequest {
                GuildId = Guild.Id,
                ReceiverIds = { player.CharacterId },
                Invite = new GuildRequest.Types.Invite {
                    GuildName = Guild.Name,
                    SenderId = requestor.CharacterId,
                    SenderName = requestor.Name,
                },
            };

            GuildResponse response = client.Guild(request);
            return (GuildError) response.Error;
        } catch (RpcException) {
            return GuildError.s_guild_err_unknown;
        }
    }

    public GuildError Join(string requestorName, PlayerInfo info) {
        if (Guild.Members.Count >= Guild.Capacity) {
            return GuildError.s_guild_err_full_member;
        }
        if (Guild.Members.ContainsKey(info.CharacterId)) {
            return GuildError.s_guild_err_already_exist;
        }

        using GameStorage.Request db = GameStorage.Context();
        GuildMember? member = db.CreateGuildMember(Guild.Id, info);
        if (member == null) {
            return GuildError.s_guild_err_fail_addmember;
        }

        // Broadcast before adding this new member.
        Broadcast(new GuildRequest {
            AddMember = new GuildRequest.Types.AddMember {
                CharacterId = member.CharacterId,
                RequestorName = requestorName,
                Rank = member.Rank,
                JoinTime = member.JoinTime,
                LoginTime = member.LoginTime,
            },
        });
        Guild.Members.TryAdd(member.CharacterId, member);
        return GuildError.none;
    }

    public GuildError Expel(long requestorId, long characterId) {
        if (!Guild.Members.TryGetValue(requestorId, out GuildMember? requestor)) {
            return GuildError.s_guild_err_null_member;
        }
        GuildRank? rank = Guild.Ranks.ElementAtOrDefault(requestor.Rank);
        if (rank == null || !rank.Permission.HasFlag(GuildPermission.ExpelMembers)) {
            return GuildError.s_guild_err_no_authority;
        }
        if (characterId == Guild.LeaderCharacterId) {
            return GuildError.s_guild_err_expel_target_master;
        }
        if (!Guild.Members.TryGetValue(characterId, out GuildMember? member)) {
            return GuildError.s_guild_err_not_join_member;
        }
        if (member.Rank < requestor.Rank) {
            return GuildError.s_guild_err_no_authority;
        }

        using GameStorage.Request db = GameStorage.Context();
        if (!db.DeleteGuildMember(Guild.Id, member.CharacterId)) {
            return GuildError.s_guild_err_unknown;
        }

        Broadcast(new GuildRequest {
            RemoveMember = new GuildRequest.Types.RemoveMember {
                CharacterId = member.CharacterId,
                RequestorName = requestor.Name,
            },
        });
        Guild.Members.TryRemove(member.CharacterId, out _);
        return GuildError.none;
    }

    public GuildError Leave(long characterId) {
        if (characterId == Guild.LeaderCharacterId) {
            return GuildError.s_guild_err_cannot_leave_master;
        }
        if (!Guild.Members.TryGetValue(characterId, out GuildMember? member)) {
            return GuildError.s_guild_err_not_join_member;
        }

        using GameStorage.Request db = GameStorage.Context();
        if (!db.DeleteGuildMember(Guild.Id, member.CharacterId)) {
            return GuildError.s_guild_err_unknown;
        }

        Guild.Members.TryRemove(member.CharacterId, out _);
        Broadcast(new GuildRequest {
            RemoveMember = new GuildRequest.Types.RemoveMember {
                CharacterId = member.CharacterId,
            },
        });
        return GuildError.none;
    }

    public GuildError UpdateMember(long requestorId, long characterId, byte? rankId, string? message) {
        if (!Guild.Members.TryGetValue(requestorId, out GuildMember? requestor)) {
            return GuildError.s_guild_err_null_member;
        }
        if (!Guild.Members.TryGetValue(characterId, out GuildMember? member)) {
            return GuildError.s_guild_err_null_member;
        }

        if (rankId != null) {
            if (member.Rank == rankId) {
                return GuildError.s_guild_err_set_grade_failed;
            }

            if (requestorId != Guild.LeaderCharacterId) {
                return GuildError.s_guild_err_no_authority;
            }
            GuildRank? newRank = Guild.Ranks.ElementAtOrDefault((byte) rankId);
            if (newRank == null) {
                return GuildError.s_guild_err_invalid_grade_index;
            }

            member.Rank = (byte) rankId;
            Broadcast(new GuildRequest {
                UpdateMember = new GuildRequest.Types.UpdateMember {
                    RequestorId = requestor.CharacterId,
                    CharacterId = member.CharacterId,
                    Rank = member.Rank,
                },
            });
        }
        if (message != null) {
            if (message.Length > Constant.MaxMottoLength) {
                return GuildError.s_guild_err_none;
            }

            member.Message = message;
            Broadcast(new GuildRequest {
                UpdateMember = new GuildRequest.Types.UpdateMember {
                    RequestorId = requestor.CharacterId,
                    CharacterId = member.CharacterId,
                    Message = member.Message,
                },
            });
        }

        return GuildError.none;
    }

    public string ConsumeInvite(long characterId) {
        foreach ((long id, (string name, DateTime expiryTime)) in pendingInvites) {
            // Remove any expired entries while iterating.
            if (expiryTime < DateTime.Now) {
                pendingInvites.Remove(id, out _);
                continue;
            }

            if (id == characterId) {
                pendingInvites.Remove(id, out _);
                return name;
            }
        }

        return string.Empty;
    }
}
