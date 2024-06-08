using Maple2.Database.Context;
using Maple2.Database.Extensions;
using Maple2.Database.Model;
using Maple2.Model.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Maple2.Database.Storage;

public partial class GameStorage {
    private readonly ItemMetadataStorage itemMetadata;
    private readonly MapMetadataStorage mapMetadata;
    private readonly AchievementMetadataStorage achievementMetadata;
    private readonly QuestMetadataStorage questMetadata;
    private readonly ILogger logger;
    private readonly DbContextOptions options;

    public GameStorage(DbContextOptions options, ItemMetadataStorage itemMetadata, MapMetadataStorage mapMetadata, AchievementMetadataStorage achievementMetadata,
                       QuestMetadataStorage questMetadata, ILogger<GameStorage> logger) {
        this.options = options;
        this.itemMetadata = itemMetadata;
        this.mapMetadata = mapMetadata;
        this.achievementMetadata = achievementMetadata;
        this.questMetadata = questMetadata;
        this.logger = logger;
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

        private static PlayerInfo BuildPlayerInfo(Model.Character character, UgcMap indoor, UgcMap? outdoor, AchievementInfo achievementInfo, long premiumTime, IList<long> clubs) {
            if (outdoor == null) {
                return new PlayerInfo(character, indoor.Name, achievementInfo, clubs) {
                    PremiumTime = premiumTime,
                };
            }

            return new PlayerInfo(character, outdoor.Name, achievementInfo, clubs) {
                PlotMapId = outdoor.MapId,
                PlotNumber = outdoor.Number,
                PremiumTime = premiumTime,
                ApartmentNumber = outdoor.ApartmentNumber,
                PlotExpiryTime = outdoor.ExpiryTime.ToUnixTimeSeconds(),
            };
        }
    }
}
