using Maple2.Database.Storage;
using Maple2.Model.Common;
using Maple2.Model.Game;
using Maple2.Model.Metadata;

namespace Maple2.Server.Game.Manager.Field;

public class HomeFieldManager : FieldManager {
    public long OwnerId => Home.AccountId;
    public Home Home { get; init; }

    public HomeSurvey? HomeSurvey { get; private set; }

    public HomeFieldManager(Home home, MapMetadata mapMetadata, UgcMapMetadata ugcMetadata, MapEntityMetadata entities, NpcMetadataStorage npcMetadata)
        : base(mapMetadata, ugcMetadata, entities, npcMetadata) {
        Home = home;
    }

    public override void Init() {
        base.Init();

        using GameStorage.Request db = GameStorage.Context();
        foreach (Plot plot in db.LoadPlotsForMap(MapId, OwnerId)) {
            Plots[plot.Number] = plot;
        }

        Plots.Values
            .SelectMany(c => c.Cubes.Values)
            .Where(p => p.Interact?.PortalSettings is not null)
            .ToList()
            .ForEach(cubePortal => SpawnCubePortal(cubePortal));
    }

    public void SetHomeSurvey(HomeSurvey survey) {
        HomeSurvey = survey;
    }

    public void RemoveHomeSurvey() {
        HomeSurvey = null;
    }
}
