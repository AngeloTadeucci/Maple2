using Maple2.Database.Storage;
using Maple2.Model.Enum;
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

        List<PlotCube> cubePortals = Plots.FirstOrDefault().Value.Cubes.Values
            .Where(x => x.Interact?.PortalSettings is not null)
            .ToList();

        foreach (PlotCube cubePortal in cubePortals) {
            SpawnCubePortal(cubePortal);
        }

        List<PlotCube> lifeSkillCubes = Plots.FirstOrDefault().Value.Cubes.Values
            .Where(x => x.HousingCategory is HousingCategory.Ranching or HousingCategory.Farming)
            .ToList();

        foreach (PlotCube cube in lifeSkillCubes) {
            AddFieldFunctionInteract(cube);
        }
    }

    public void SetHomeSurvey(HomeSurvey survey) {
        HomeSurvey = survey;
    }

    public void RemoveHomeSurvey() {
        HomeSurvey = null;
    }
}
