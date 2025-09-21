﻿using Maple2.Database.Context;
using Maple2.Database.Extensions;
using Maple2.Database.Model;
using Maple2.Model.Enum;
using Maple2.Model.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    private readonly ItemMetadataStorage itemMetadata;
    private readonly MapMetadataStorage mapMetadata;
    private readonly AchievementMetadataStorage achievementMetadata;
    private readonly QuestMetadataStorage questMetadata;
    private readonly ServerTableMetadataStorage serverTableMetadata;
    private readonly TableMetadataStorage tableMetadata;
    private readonly FunctionCubeMetadataStorage functionCubeMetadata;
    private readonly ILogger logger;
    private readonly DbContextOptions options;

    public GameStorage(DbContextOptions options, ItemMetadataStorage itemMetadata, MapMetadataStorage mapMetadata, AchievementMetadataStorage achievementMetadata,
                       QuestMetadataStorage questMetadata, TableMetadataStorage tableMetadata, ServerTableMetadataStorage serverTableMetadata, ILogger<GameStorage> logger, FunctionCubeMetadataStorage functionCubeMetadata) {
        this.options = options;
        this.itemMetadata = itemMetadata;
        this.mapMetadata = mapMetadata;
        this.achievementMetadata = achievementMetadata;
        this.questMetadata = questMetadata;
        this.logger = logger;
        this.functionCubeMetadata = functionCubeMetadata;
        this.tableMetadata = tableMetadata;
        this.serverTableMetadata = serverTableMetadata;

        var context = new MetadataContext(options);

        // check Ingest has been run
        if (!context.Database.CanConnect()) {
            throw new Exception("Game database not found, did you run Maple2.File.Ingest?");
        }
    }

    public Request Context() {
        // We use NoTracking by default since most requests are Read or Overwrite.
        // If we need tracking for modifying data, we can set it individually as needed.
        var context = new Ms2Context(options);
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return new Request(this, context, logger);
    }

    public partial class Request : DatabaseRequest<Ms2Context> {
        private readonly GameStorage game;
        public Request(GameStorage game, Ms2Context context, ILogger logger) : base(context, logger) {
            this.game = game;
        }

        private static PlayerInfo BuildPlayerInfo(Model.Character character, string permissions, UgcMap indoor, UgcMap? outdoor, AchievementInfo achievementInfo, long guildId, string guildName, long premiumTime, IList<long> clubs) {
            if (outdoor == null) {
                return new PlayerInfo(character, indoor.Name, achievementInfo, clubs) {
                    PremiumTime = premiumTime,
                    LastOnlineTime = character.LastModified.ToEpochSeconds(),
                    GuildId = guildId,
                    GuildName = guildName,
                    AccountAdminPermissions = Enum.Parse<AdminPermissions>(permissions, true),
                };
            }

            return new PlayerInfo(character, outdoor.Name, achievementInfo, clubs) {
                PlotMapId = outdoor.MapId,
                PlotNumber = outdoor.Number,
                PremiumTime = premiumTime,
                ApartmentNumber = outdoor.ApartmentNumber,
                PlotExpiryTime = outdoor.ExpiryTime.ToUnixTimeSeconds(),
                LastOnlineTime = character.LastModified.ToEpochSeconds(),
                GuildId = guildId,
                GuildName = guildName,
            };
        }
    }
}
