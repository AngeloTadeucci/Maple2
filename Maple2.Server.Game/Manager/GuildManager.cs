using System.Diagnostics.CodeAnalysis;
using Maple2.Model.Enum;
using Maple2.Model.Error;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Core.Sync;
using Maple2.Server.Game.Packets;
using Maple2.Server.Game.Session;
using Maple2.Server.Game.Util.Sync;
using Maple2.Server.World.Service;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Game.Manager;

public class GuildManager : IDisposable {
    private readonly GameSession session;

    public Guild? Guild { get; private set; }
    public long Id => Guild?.Id ?? 0;

    private GuildTable.Property properties;
    private readonly CancellationTokenSource tokenSource;

    private readonly ILogger logger = Log.Logger.ForContext<GuildManager>();

    public GuildManager(GameSession session) {
        this.session = session;
        UpdateProperties();
        tokenSource = new CancellationTokenSource();

        if (session.Player.Value.Character.GuildId == 0) {
            return;
        }

        GuildInfoResponse response = session.World.GuildInfo(new GuildInfoRequest {
            GuildId = session.Player.Value.Character.GuildId,
        });
        if (response.Guild != null) {
            SetGuild(response.Guild, false);
        }
    }

    public void Dispose() {
        session.Dispose();
        tokenSource.Dispose();

        if (Guild != null) {
            foreach (GuildMember member in Guild.Members.Values) {
                member.Dispose();
            }
        }
    }

    public void Load() {
        if (Guild == null) {
            return;
        }

        session.Send(GuildPacket.Load(Guild));
    }

    public bool SetGuild(GuildInfo info, bool addTag = true) {
        if (Guild != null) {
            return false;
        }

        GuildMember[] members = info.Members.Select(member => {
            if (!session.PlayerInfo.GetOrFetch(member.CharacterId, out PlayerInfo? playerInfo)) {
                return null;
            }

            var result = new GuildMember {
                GuildId = info.Id,
                Info = playerInfo.Clone(),
                Message = member.Message,
                Rank = (byte) member.Rank,
                JoinTime = member.JoinTime,
                LoginTime = member.LoginTime,
                CheckinTime = member.CheckinTime,
                DonationTime = member.DonationTime,
                WeeklyContribution = member.WeeklyContribution,
                TotalContribution = member.TotalContribution,
                DailyDonationCount = member.DailyDonationCount,
            };
            return result;
        }).WhereNotNull().ToArray();

        GuildMember? leader = members.SingleOrDefault(member => member.Rank == 0);
        if (leader == null) {
            logger.Error("Guild {GuildId} does not have a valid leader", info.Id);
            session.Send(GuildPacket.Error(GuildError.s_guild_err_unknown));
            return false;
        }

        var guild = new Guild(info.Id, info.Name, leader) {
            Emblem = info.Emblem,
            Notice = info.Notice,
            CreationTime = info.CreationTime,
            Focus = (GuildFocus) info.Focus,
            Experience = info.Experience,
            Funds = info.Funds,
            HouseRank = info.HouseRank,
            HouseTheme = info.HouseTheme,
            Ranks = info.Ranks.Select((rank, i) => new GuildRank {
                Id = (byte) i,
                Name = rank.Name,
                Permission = (GuildPermission) rank.Permission,
            }).ToArray(),
            Buffs = info.Buffs.Select(buff => new GuildBuff {
                Id = buff.Id,
                Level = buff.Level,
                ExpiryTime = buff.ExpiryTime,
            }).ToList(),
            Npcs = info.Npcs.Select(npc => new GuildNpc {
                Type = (GuildNpcType) npc.Type,
                Level = npc.Level,
            }).ToList(),
        };
        foreach (GuildMember member in members) {
            if (guild.Members.TryAdd(member.CharacterId, member)) {
                BeginListen(member);
            }
        }

        Guild = guild;
        UpdateProperties();

        session.Player.Value.Character.GuildId = Guild.Id;
        session.Player.Value.Character.GuildName = Guild.Name;
        if (addTag) {
            session.Field?.Broadcast(GuildPacket.AddTag(session.PlayerName, Guild.Name));
        }
        return true;
    }

    public void RemoveGuild() {
        if (Guild == null) {
            return;
        }

        Guild = null;
        session.Player.Value.Character.GuildId = 0;
        session.Player.Value.Character.GuildName = string.Empty;
        session.Field?.Broadcast(GuildPacket.RemoveTag(session.PlayerName));
    }

    public bool AddMember(string requestorName, GuildMember member) {
        if (Guild == null) {
            return false;
        }
        if (!Guild.Members.TryAdd(member.CharacterId, member)) {
            return false;
        }

        BeginListen(member);
        Guild.AchievementInfo += member.Info.AchievementInfo;
        session.Send(GuildPacket.Joined(requestorName, member));
        return true;
    }

    public bool RemoveMember(long characterId, string requestorName = "") {
        if (Guild == null) {
            return false;
        }
        if (!Guild.Members.TryRemove(characterId, out GuildMember? member)) {
            return false;
        }
        Guild.AchievementInfo -= member.Info.AchievementInfo;
        EndListen(member);

        session.Send(string.IsNullOrEmpty(requestorName)
            ? GuildPacket.NotifyLeave(member.Name)
            : GuildPacket.NotifyExpelMember(requestorName, member.Name));
        return true;
    }

    public bool UpdateMemberRank(long requestorId, long characterId, byte rankId) {
        GuildMember? requestor = GetMember(requestorId);
        GuildMember? member = GetMember(characterId);
        if (requestor == null || member == null) {
            return false;
        }

        member.Rank = rankId;
        session.Send(GuildPacket.NotifyUpdateMemberRank(requestor.Name, member.Name, rankId));
        return true;
    }

    public bool UpdateMemberMessage(long characterId, string message) {
        GuildMember? member = GetMember(characterId);
        if (member == null) {
            return false;
        }

        member.Message = message;
        session.Send(GuildPacket.NotifyUpdateMemberMessage(member));
        return true;
    }

    public GuildRank? GetRank(long characterId) {
        if (Guild == null || !Guild.Members.TryGetValue(characterId, out GuildMember? member)) {
            return null;
        }

        return Guild.Ranks.ElementAtOrDefault(member.Rank);
    }

    public GuildMember? GetMember(string name) {
        return Guild?.Members.Values.FirstOrDefault(member => member.Name == name);
    }

    public GuildMember? GetMember(long characterId) {
        if (Guild?.Members.TryGetValue(characterId, out GuildMember? member) == true) {
            return member;
        }

        return null;
    }

    [MemberNotNull(nameof(properties))]
    private void UpdateProperties() {
        int experience = Guild?.Experience ?? 0;
        KeyValuePair<short, GuildTable.Property> result = session.TableMetadata.GuildTable.Properties
            .OrderBy(entry => entry.Value.Experience)
            .MinBy(entry => entry.Value.Experience > experience);
        properties = result.Value;

        if (Guild != null) {
            Guild.Capacity = properties.Capacity;
        }
    }

    #region PlayerInfo Events
    private void BeginListen(GuildMember member) {
        // Clean up previous token if necessary
        if (member.TokenSource != null) {
            logger.Warning("BeginListen called on Member {Id} that was already listening", member.CharacterId);
            EndListen(member);
        }

        member.TokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token);
        CancellationToken token = member.TokenSource.Token;
        var listener = new PlayerInfoListener(UpdateField.Guild, (type, info) => SyncUpdate(token, member.CharacterId, type, info));
        session.PlayerInfo.Listen(member.Info.CharacterId, listener);
    }

    private void EndListen(GuildMember member) {
        member.TokenSource?.Cancel();
        member.TokenSource?.Dispose();
        member.TokenSource = null;
    }

    private bool SyncUpdate(CancellationToken cancel, long id, UpdateField type, IPlayerInfo info) {
        if (cancel.IsCancellationRequested || Guild == null || !Guild.Members.TryGetValue(id, out GuildMember? member)) {
            return true;
        }

        bool wasOnline = member.Info.Online;
        string name = member.Info.Name;
        member.Info.Update(type, info);
        member.LoginTime = info.UpdateTime;

        if (name != member.Info.Name) {
            session.Send(GuildPacket.UpdateMemberName(name, member.Name));
        }

        if (type == UpdateField.Map) {
            session.Send(GuildPacket.UpdateMemberMap(member.Name, member.Info.MapId));
        } else {
            session.Send(GuildPacket.UpdateMember(member.Info));
        }

        if (member.Info.Online != wasOnline) {
            session.Send(member.Info.Online
                ? GuildPacket.NotifyLogin(member.Name)
                : GuildPacket.NotifyLogout(member.Name, member.LoginTime));
        }
        return false;
    }
    #endregion
}
