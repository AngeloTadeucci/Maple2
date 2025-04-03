using Maple2.Model.Game;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;
using Serilog;

namespace Maple2.Server.Game.Manager.Field;

// TODO: Queue of players to enter the stage, time limit, etc
public class PerformanceStageManager {
    private readonly ILogger logger = Log.Logger.ForContext<PerformanceStageManager>();

    private IField Field { get; }

    public PerformanceStageManager(IField field) {
        Field = field;
    }

    public void EnterExitStage(GameSession session) {
        if (Field is not FieldManager fieldManager) {
            return;
        }
        fieldManager.TriggerObjects.Boxes.TryGetValue(101, out TriggerBox? triggerBox);
        if (triggerBox is null) {
            return;
        }

        int portalId = triggerBox.Contains(session.Player.Position) ? 802 : 803;
        if (!Field.TryGetPortal(portalId, out FieldPortal? portal)) {
            return;
        }

        session.Player.MoveToPortal(portal);
    }
}
