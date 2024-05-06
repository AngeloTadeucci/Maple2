using Maple2.Database.Context;
using Maple2.Database.Extensions;
using Maple2.Database.Model;
using Maple2.Model.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Maple2.Database.Storage;

public partial class GameStorage(
    DbContextOptions options,
    ItemMetadataStorage itemMetadata,
    MapMetadataStorage mapMetadata,
    AchievementMetadataStorage achievementMetadata,
    QuestMetadataStorage questMetadata,
    ILogger<GameStorage> logger) {
    private readonly ItemMetadataStorage itemMetadata = itemMetadata;
    private readonly MapMetadataStorage mapMetadata = mapMetadata;
    private readonly AchievementMetadataStorage achievementMetadata = achievementMetadata;
    private readonly QuestMetadataStorage questMetadata = questMetadata;
    private readonly ILogger logger = logger;

    public Request Context() {
        // We use NoTracking by default since most requests are Read or Overwrite.
        // If we need tracking for modifying data, we can set it individually as needed.
        var context = new Ms2Context(options);
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return new Request(this, context, logger);
    }

    public partial class Request(GameStorage game, Ms2Context context, ILogger logger) : DatabaseRequest<Ms2Context>(context, logger) {

        private static PlayerInfo BuildPlayerInfo(Model.Character character, UgcMap indoor, UgcMap? outdoor, AchievementInfo achievementInfo) {
            if (outdoor == null) {
                return new PlayerInfo(character, indoor.Name, achievementInfo);
            }

            return new PlayerInfo(character, outdoor.Name, achievementInfo) {
                PlotMapId = outdoor.MapId,
                PlotNumber = outdoor.Number,
                ApartmentNumber = outdoor.ApartmentNumber,
                PlotExpiryTime = outdoor.ExpiryTime.ToEpochSeconds(),
            };
        }
    }
}
