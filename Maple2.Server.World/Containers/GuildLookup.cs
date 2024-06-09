using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Maple2.Database.Storage;
using Maple2.Model.Error;
using Maple2.Model.Game;

namespace Maple2.Server.World.Containers;

public class GuildLookup : IDisposable {
    private readonly GameStorage gameStorage;
    private readonly TableMetadataStorage tableMetadata;
    private readonly ChannelClientLookup channelClients;
    private readonly PlayerInfoLookup playerLookup;

    private readonly ConcurrentDictionary<long, GuildManager> guilds;

    public GuildLookup(GameStorage gameStorage,
                       TableMetadataStorage tableMetadata,
                       ChannelClientLookup channelClients,
                       PlayerInfoLookup playerLookup) {
        this.gameStorage = gameStorage;
        this.tableMetadata = tableMetadata;
        this.channelClients = channelClients;
        this.playerLookup = playerLookup;

        guilds = new ConcurrentDictionary<long, GuildManager>();
    }

    public void Dispose() {
        // We must dispose all GuildManager to save state.
        foreach (GuildManager manager in guilds.Values) {
            manager.Dispose();
        }
    }

    public bool TryGet(long guildId, [NotNullWhen(true)] out GuildManager? guild) {
        if (guilds.TryGetValue(guildId, out guild)) {
            return true;
        }

        guild = FetchGuild(guildId);
        return guild != null;
    }

    public GuildError Create(string name, long leaderId, out long guildId) {
        guildId = 0;
        using GameStorage.Request db = gameStorage.Context();
        if (db.GuildExists(guildName: name)) {
            return GuildError.s_guild_err_name_exist;
        }

        Guild? guild = db.CreateGuild(name, leaderId);
        if (guild == null) {
            return GuildError.s_guild_err_unknown;
        }
        foreach (GuildMember member in db.GetGuildMembers(playerLookup, guild.Id)) {
            guild.Members.TryAdd(member.CharacterId, member);
        }

        guildId = guild.Id;
        guilds.TryAdd(guildId, new GuildManager(guild) {
            GameStorage = gameStorage,
            ChannelClients = channelClients,
            TableMetadata = tableMetadata,
        });

        return GuildError.none;
    }

    public GuildError Disband(long requestorId, long guildId) {
        if (!TryGet(guildId, out GuildManager? manager)) {
            return GuildError.s_guild_err_null_guild;
        }
        if (requestorId != manager.Guild.LeaderCharacterId) {
            return GuildError.s_guild_err_no_master;
        }
        if (manager.Guild.Members.Count > 1) {
            return GuildError.s_guild_err_exist_member;
        }

        if (!guilds.TryRemove(guildId, out manager)) {
            // Failed to remove guild after validating.
            return GuildError.s_guild_err_unknown;
        }
        manager.Dispose();

        using GameStorage.Request db = gameStorage.Context();
        if (!db.DeleteGuild(guildId)) {
            return GuildError.s_guild_err_unknown;
        }

        return GuildError.none;
    }

    private GuildManager? FetchGuild(long guildId) {
        using GameStorage.Request db = gameStorage.Context();
        Guild? guild = db.GetGuild(guildId);
        if (guild == null) {
            return null;
        }

        foreach (GuildMember member in db.GetGuildMembers(playerLookup, guildId)) {
            guild.Members[member.CharacterId] = member;
        }

        var manager = new GuildManager(guild) {
            GameStorage = gameStorage,
            ChannelClients = channelClients,
            TableMetadata = tableMetadata,
        };
        return guilds.TryAdd(guild.Id, manager) ? manager : null;
    }
}
