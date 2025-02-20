using Maple2.Model.Game;

namespace Maple2.Server.Game.Manager.Field;

public partial class FieldManager {
    public readonly long OwnerId;

    public HomeSurvey? HomeSurvey { get; private set; }

    public void SetHomeSurvey(HomeSurvey survey) {
        HomeSurvey = survey;
    }

    public void RemoveHomeSurvey() {
        HomeSurvey = null;
    }
}
