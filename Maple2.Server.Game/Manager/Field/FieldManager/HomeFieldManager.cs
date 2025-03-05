using Maple2.Database.Storage;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Manager.Field;

namespace Maple2.Server.Game.Model.Room;

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
    }


    public void SetHomeSurvey(HomeSurvey survey) {
        HomeSurvey = survey;
    }

    public void RemoveHomeSurvey() {
        HomeSurvey = null;
    }
}
