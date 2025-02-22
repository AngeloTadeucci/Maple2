using Maple2.Model.Game;
using Maple2.Server.Game.Session;
using Serilog;

namespace Maple2.Server.Game.Manager.Field;

// TODO: Queue of players to enter the stage, time limit, etc
public class PerformanceStageManager {
    private readonly ILogger logger = Log.Logger.ForContext<PerformanceStageManager>();

    private FieldManager Field { get; }

    public PerformanceStageManager(FieldManager field) {
        Field = field;
    }

    public void EnterExitStage(GameSession session) {
        Field.TriggerObjects.Boxes.TryGetValue(101, out TriggerBox? triggerBox);
        if (triggerBox is null) {
            return;
        }

        bool insideStage = triggerBox.Contains(session.Player.Position);
        Field.MoveToPortal(session, insideStage ? 802 : 803);
    }
}
