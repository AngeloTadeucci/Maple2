using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.Model.Metadata;

namespace Maple2.Server.Game.Manager.Field;

public class HomeFieldManager : FieldManager {
    public long OwnerId => Home.AccountId;
    public Home Home { get; init; }

    public HomeSurvey? HomeSurvey { get; private set; }

    private readonly int roomId;

    public HomeFieldManager(Home home, MapMetadata mapMetadata, UgcMapMetadata ugcMetadata, MapEntityMetadata entities, NpcMetadataStorage npcMetadata, int roomId)
        : base(mapMetadata, ugcMetadata, entities, npcMetadata) {
        this.roomId = roomId;
        Home = home;
    }

    public override void Init() {
        base.Init();

        using GameStorage.Request db = GameStorage.Context();
        foreach (Plot plot in db.LoadPlotsForMap(MapId, OwnerId)) {
            // If roomId is -1, it's a planner plot and should start empty.
            if (roomId == -1) {
                plot.Cubes.Clear();
            }
            Plots[plot.Number] = plot;

            plot.Cubes.Values
                .Where(p => p.Interact?.PortalSettings is not null)
                .ToList()
                .ForEach(cubePortal => SpawnCubePortal(cubePortal));

            plot.Cubes.Values
                .Where(plotCube => plotCube.Interact != null)
                .ToList()
                .ForEach(plotCube => AddFieldFunctionInteract(plotCube));
        }
    }

    public void SetHomeSurvey(HomeSurvey survey) {
        HomeSurvey = survey;
    }

    public void RemoveHomeSurvey() {
        HomeSurvey = null;
    }
}
