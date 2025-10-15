using Maple2.Model.Enum;
using Maple2.Model.Game;
using Maple2.Model.Metadata;
using Maple2.Server.Game.Model;
using Maple2.Server.Game.Session;
using Maple2.Tools.Extensions;
using Serilog;

namespace Maple2.Server.Game.Manager.Field;

public class PerformanceStageManager {
    private readonly ILogger logger = Log.Logger.ForContext<PerformanceStageManager>();

    private FieldManager Field { get; }

    private GameSession? performanceOwner; // The session who started the performance
    private int performanceEndTick; // The tick when the performance will end

    public PerformanceStageManager(FieldManager field) {
        Field = field;
    }

    public void EnterExitStage(GameSession session) {
        Field.TriggerObjects.Boxes.TryGetValue(101, out TriggerBox? triggerBox);
        if (triggerBox is null) {
            return;
        }

        int portalId = triggerBox.Contains(session.Player.Position) ? 802 : 803;
        if (!Field.TryGetPortal(portalId, out FieldPortal? portal)) {
            return;
        }

        session.Player.MoveToPortal(portal);
    }

    public void StartPerformance(GameSession session) {
        // Check if someone is already performing
        if (performanceOwner != null) {
            logger.Debug("Player {PlayerName} tried to start performance but stage is already occupied", session.PlayerName);
            return;
        }

        // Start the performance
        performanceOwner = session;
        performanceEndTick = (Field.FieldTick + (long) Constant.MaxPerformanceDuration.TotalMilliseconds).Truncate32();

        logger.Debug("Player {PlayerName} started performance on stage, ends at tick {PerformanceEndTick}", session.PlayerName, performanceEndTick);

        Field.AddFieldProperty(new FieldPropertyMusicConcert {
            CharacterId = session.CharacterId,
            PerformanceEndTick = performanceEndTick,
        });
    }

    public void EndPerformance(GameSession session) {
        if (performanceOwner == null) {
            logger.Warning("Player {PlayerName} tried to end performance but no performance is active", session.PlayerName);
            return;
        }

        // Only the owner can end the performance
        if (performanceOwner.CharacterId != session.CharacterId) {
            logger.Warning("Player {PlayerName} tried to end performance but is not the owner", session.PlayerName);
            return;
        }

        logger.Debug("Player {PlayerName} ended performance at tick {CurrentTick}, scheduled end was {PerformanceEndTick}", session.PlayerName, Field.FieldTickInt, performanceEndTick);

        EndPerformance();
    }

    private void EndPerformance() {
        performanceOwner = null;
        performanceEndTick = 0;
        Field.RemoveFieldProperty(FieldProperty.MusicConcert);
    }

    public bool IsCurrentPerformer(GameSession session) {
        if (performanceOwner == null) {
            return false;
        }

        // If owner is in a party, check if session is in the same party
        if (performanceOwner.Party.Party != null) {
            return session.Party.Party?.Id == performanceOwner.Party.Party.Id;
        }

        // Solo performance, only the owner is performing
        return performanceOwner.CharacterId == session.CharacterId;
    }

    public void Update() {
        if (performanceOwner == null) {
            return;
        }

        // Check if current performance has exceeded time limit
        if (Field.FieldTickInt - performanceEndTick >= 0) {
            logger.Debug("Performance by {PlayerName} has reached time limit at tick {CurrentTick}, scheduled end was {PerformanceEndTick}", performanceOwner.PlayerName, Field.FieldTickInt, performanceEndTick);
            EndPerformance();
            return;
        }

        // Check if the owner has left the field or gone offline - this ends the performance
        bool isConnected = performanceOwner.State == SessionState.Connected;
        int? ownerMapId = performanceOwner.Field?.MapId;
        bool isInPerformanceMap = ownerMapId == Constant.PerformanceMapId;

        if (!isConnected || !isInPerformanceMap) {
            logger.Debug("Performance owner {PlayerName} left the performance map or went offline at tick {CurrentTick}, scheduled end was {PerformanceEndTick}. SessionState={SessionState}, OwnerMapId={OwnerMapId}, PerformanceMapId={PerformanceMapId}",
                performanceOwner.PlayerName, Field.FieldTickInt, performanceEndTick, performanceOwner.State, ownerMapId, Constant.PerformanceMapId);
            EndPerformance();
        }
    }
}
